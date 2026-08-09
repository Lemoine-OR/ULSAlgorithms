using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements the classical Least Unit Cost (LUC) heuristic.
/// </summary>
/// <remarks>
/// LUC is analogous to Silver-Meal but divides setup-plus-holding cost by the
/// number of units in the candidate lot rather than by the number of periods.
/// The lot is extended until relevant cost per unit first increases.
/// </remarks>
public sealed class LeastUnitCostSolver : IUlsSolver
{
    public string Name => "Least Unit Cost";

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
                horizon > 1 ? problem.HoldingCosts[0] : 0.0;

            var start =
                ClassicHeuristicGuard.FindNextPositiveDemand(demands, 0);

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

                start = ClassicHeuristicGuard.FindNextPositiveDemand(
                    demands,
                    bestEnd + 1);
            }

            return HeuristicSolutionBuilder.Build(
                problem,
                cycleEnds,
                Name,
                cancellationToken);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(buffer, clearArray: false);
        }
    }
}
