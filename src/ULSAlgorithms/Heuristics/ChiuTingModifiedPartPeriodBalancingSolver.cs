using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements the modified Part-Period Balancing (mv-PPB) heuristic of
/// Chiu, Ting and Chiu.
/// </summary>
/// <remarks>
/// <para>
/// A standard nearest-EPP PPB plan is first generated. The published
/// post-processing step then tests whether eliminating the final replenishment
/// order by merging it into the preceding order strictly reduces total
/// inventory cost.
/// </para>
/// <para>
/// Reference: S. W. Chiu, C.-K. Ting and Y. P. Chiu,
/// "A modified version of the part period lot-sizing heuristic",
/// International Journal for Engineering Modelling 18(1-2), 59-64, 2005.
/// </para>
/// </remarks>
public sealed class ChiuTingModifiedPartPeriodBalancingSolver : IUlsSolver
{
    public string Name => "Chiu-Ting modified Part-Period Balancing";

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
            var holdingCost =
                horizon > 1
                    ? problem.HoldingCosts[0]
                    : 0.0;

            var epp =
                holdingCost == 0.0
                    ? double.PositiveInfinity
                    : problem.SetupCosts[0] / holdingCost;

            var start =
                ClassicHeuristicGuard.FindNextPositiveDemand(
                    demands,
                    0);

            while (start < horizon)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (double.IsPositiveInfinity(epp))
                {
                    cycleEnds[start] = horizon - 1;
                    break;
                }

                var bestEnd = start;
                var partPeriods = 0.0;
                var bestDifference = epp;

                for (var end = start + 1; end < horizon; end++)
                {
                    partPeriods +=
                        (end - start) *
                        demands[end];

                    if (!double.IsFinite(partPeriods))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow while evaluating modified PPB.");
                    }

                    var difference =
                        Math.Abs(epp - partPeriods);

                    if (difference < bestDifference ||
                        (difference == bestDifference &&
                         end > bestEnd))
                    {
                        bestDifference = difference;
                        bestEnd = end;
                    }

                    if (partPeriods >= epp)
                    {
                        break;
                    }
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
