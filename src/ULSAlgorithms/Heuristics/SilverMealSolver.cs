using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements the Silver-Meal least-cost-per-period heuristic.
/// </summary>
/// <remarks>
/// Starting with the first uncovered positive demand, the candidate
/// replenishment cycle is extended while the average setup-plus-holding cost
/// per covered calendar period does not increase.
/// <para>
/// Reference: E. A. Silver and H. C. Meal,
/// "A heuristic for selecting lot size quantities for the case of a
/// deterministic time-varying demand rate and discrete opportunities for
/// replenishment", Production and Inventory Management, 14(2), 64-74, 1973.
/// </para>
/// </remarks>
public sealed class SilverMealSolver : IUlsSolver
{
    public string Name => "Silver-Meal";

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
                var accumulatedHolding = 0.0;
                var previousAverage = setupCost;

                for (var end = start + 1; end < horizon; end++)
                {
                    accumulatedHolding +=
                        holdingCost *
                        (end - start) *
                        demands[end];

                    var average =
                        (setupCost + accumulatedHolding) /
                        (end - start + 1);

                    if (average > previousAverage)
                    {
                        break;
                    }

                    previousAverage = average;
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
