using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements the classical Periodic Order Quantity (POQ) rule.
/// </summary>
/// <remarks>
/// POQ converts the EOQ quantity into an integer order interval using the
/// average per-period demand:
/// <c>P = round(sqrt(2A / (h * dBar)))</c>, with a minimum of one period.
/// Each replenishment then covers the demands in the next <c>P</c> calendar
/// periods.
/// </remarks>
public sealed class PeriodicOrderQuantitySolver : IUlsSolver
{
    public string Name => "Periodic Order Quantity";

    public UlsSolverKind Kind => UlsSolverKind.Heuristic;

    public static bool IsApplicable(UlsProblem problem) =>
        ClassicHeuristicGuard.HasStationaryRelevantCosts(problem);

    public static int GetOrderInterval(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ClassicHeuristicGuard.ThrowIfNotStationary(
            problem,
            "Periodic Order Quantity");

        if (problem.TotalDemand == 0.0)
        {
            return 1;
        }

        var averageDemand =
            problem.TotalDemand / problem.Horizon;

        var setupCost = problem.SetupCosts[0];

        var holdingCost =
            problem.Horizon > 1 ? problem.HoldingCosts[0] : 0.0;

        if (holdingCost == 0.0)
        {
            return problem.Horizon;
        }

        if (setupCost == 0.0)
        {
            return 1;
        }

        var continuousInterval =
            Math.Sqrt(
                (2.0 * setupCost) /
                (holdingCost * averageDemand));

        var interval = (int)Math.Round(
            continuousInterval,
            MidpointRounding.AwayFromZero);

        return Math.Clamp(interval, 1, problem.Horizon);
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
            var interval = GetOrderInterval(problem);

            var start =
                ClassicHeuristicGuard.FindNextPositiveDemand(demands, 0);

            while (start < horizon)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var end =
                    Math.Min(
                        horizon - 1,
                        start + interval - 1);

                cycleEnds[start] = end;

                start = ClassicHeuristicGuard.FindNextPositiveDemand(
                    demands,
                    end + 1);
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
