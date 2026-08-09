using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements classical Part-Period Balancing (PPB), also commonly called the
/// Least Total Cost rule.
/// </summary>
/// <remarks>
/// The candidate lot is selected so that accumulated part-periods are as close
/// as possible to the Economic Part Period <c>A/h</c>.
/// <para>
/// Early primary reference for the part-period algorithm:
/// J. J. DeMatteis, "An economic lot-sizing technique I: The part-period
/// algorithm", IBM Systems Journal 7(1), 1968.
/// </para>
/// </remarks>
public sealed class PartPeriodBalancingSolver : IUlsSolver
{
    public string Name => "Part-Period Balancing";

    public UlsSolverKind Kind => UlsSolverKind.Heuristic;

    public static bool IsApplicable(UlsProblem problem) =>
        ClassicHeuristicGuard.HasStationaryRelevantCosts(problem);

    public static double GetEconomicPartPeriod(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ClassicHeuristicGuard.ThrowIfNotStationary(
            problem,
            "Part-Period Balancing");

        var holdingCost =
            problem.Horizon > 1 ? problem.HoldingCosts[0] : 0.0;

        return holdingCost == 0.0
            ? double.PositiveInfinity
            : problem.SetupCosts[0] / holdingCost;
    }

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
                horizon > 1 ? problem.HoldingCosts[0] : 0.0;

            var epp = holdingCost == 0.0
                ? double.PositiveInfinity
                : problem.SetupCosts[0] / holdingCost;

            var start =
                ClassicHeuristicGuard.FindNextPositiveDemand(demands, 0);

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
