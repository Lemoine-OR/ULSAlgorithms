using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements Groff's marginal-cost lot-sizing rule.
/// </summary>
/// <remarks>
/// A demand in offset <c>n</c> from the current replenishment period is added
/// while
/// <c>d[t+n] * n * (n+1) &lt;= 2A/h</c>.
/// <para>
/// Reference: G. K. Groff,
/// "A Lot-Sizing Rule for Time-Phased Component Demand",
/// Production and Inventory Management 20(1), 47-53, 1979.
/// </para>
/// </remarks>
public sealed class GroffSolver : IUlsSolver
{
    public string Name => "Groff";

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

            var threshold = holdingCost == 0.0
                ? double.PositiveInfinity
                : (2.0 * setupCost) / holdingCost;

            var start =
                ClassicHeuristicGuard.FindNextPositiveDemand(demands, 0);

            while (start < horizon)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bestEnd = start;

                for (var end = start + 1; end < horizon; end++)
                {
                    var offset = end - start;

                    var marginalCriterion =
                        demands[end] *
                        offset *
                        (offset + 1.0);

                    if (marginalCriterion > threshold)
                    {
                        break;
                    }

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
