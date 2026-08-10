using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements Chiu's modified Least Unit Cost heuristic.
/// </summary>
/// <remarks>
/// <para>
/// The ordinary LUC plan is first constructed. A final post-processing test
/// then removes the last replenishment lot when combining it with the preceding
/// lot strictly lowers total relevant cost.
/// </para>
/// <para>
/// Reference: Y. P. Chiu,
/// "A modification of the least unit cost lot-sizing heuristic",
/// Journal of Statistics and Management Systems 7(1), 197-207, 2004,
/// DOI 10.1080/09720510.2004.10701115.
/// </para>
/// </remarks>
public sealed class ChiuModifiedLeastUnitCostSolver : IUlsSolver
{
    public string Name => "Chiu modified Least Unit Cost";

    public UlsSolverKind Kind => UlsSolverKind.Heuristic;

    public static bool IsApplicable(UlsProblem problem) =>
        ClassicHeuristicGuard.HasStationaryRelevantCosts(problem);

    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();
        ClassicHeuristicGuard.ThrowIfNotStationary(problem, Name);

        var horizon = problem.Horizon;
        var buffer = ArrayPool<int>.Shared.Rent(horizon);

        try
        {
            var cycleEnds = buffer.AsSpan(0, horizon);
            cycleEnds.Fill(-1);

            var demands = problem.Demands;
            var setupCost = problem.SetupCosts[0];
            var holdingCost =
                horizon > 1
                    ? problem.HoldingCosts[0]
                    : 0.0;

            var start =
                ClassicHeuristicGuard.FindNextPositiveDemand(
                    demands,
                    0);

            while (start < horizon)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bestEnd = start;
                var quantity = demands[start];
                var accumulatedHolding = 0.0;
                var previousUnitCost = setupCost / quantity;

                for (var end = start + 1; end < horizon; end++)
                {
                    accumulatedHolding +=
                        holdingCost *
                        (end - start) *
                        demands[end];

                    quantity += demands[end];

                    if (!double.IsFinite(accumulatedHolding) ||
                        !double.IsFinite(quantity))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow while evaluating modified LUC.");
                    }

                    var unitCost =
                        (setupCost + accumulatedHolding) /
                        quantity;

                    if (unitCost > previousUnitCost)
                    {
                        break;
                    }

                    previousUnitCost = unitCost;
                    bestEnd = end;
                }

                cycleEnds[start] = bestEnd;

                start =
                    ClassicHeuristicGuard.FindNextPositiveDemand(
                        demands,
                        bestEnd + 1);
            }

            cancellationToken.ThrowIfCancellationRequested();

            LastReplenishmentMergeImprover.TryMergeLastLot(
                problem,
                cycleEnds);

            return HeuristicSolutionBuilder.Build(
                problem,
                cycleEnds,
                Name,
                cancellationToken);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(
                buffer,
                clearArray: false);
        }
    }
}
