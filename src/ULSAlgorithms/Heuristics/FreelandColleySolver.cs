using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements the Freeland-Colley incremental lot-sizing heuristic.
/// </summary>
/// <remarks>
/// <para>
/// Starting from a replenishment in period <c>s</c>, demand in a later period
/// <c>t</c> is added while its local incremental holding cost
/// <c>h * (t-s) * d[t]</c> does not exceed the setup cost.
/// </para>
/// <para>
/// Reference:
/// J. R. Freeland and J. L. Colley Jr.,
/// "A Simple Heuristic Method for Lot-Sizing in a Time-Phased Reorder System",
/// Production and Inventory Management 23(1), 15-22, 1982.
/// </para>
/// <para>
/// Worst-case time is O(T); auxiliary working memory is O(T).
/// </para>
/// </remarks>
public sealed class FreelandColleySolver : IUlsSolver
{
    public string Name => "Freeland-Colley";

    public UlsSolverKind Kind => UlsSolverKind.Heuristic;

    public static bool IsApplicable(UlsProblem problem) =>
        ClassicHeuristicGuard.HasStationaryRelevantCosts(problem);

    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        ClassicHeuristicGuard.ThrowIfNotStationary(
            problem,
            Name);

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

                var end = start;

                for (var candidate = start + 1;
                     candidate < horizon;
                     candidate++)
                {
                    var incrementalHolding =
                        holdingCost *
                        (candidate - start) *
                        demands[candidate];

                    if (!double.IsFinite(incrementalHolding))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow in the Freeland-Colley criterion.");
                    }

                    if (incrementalHolding > setupCost)
                    {
                        break;
                    }

                    end = candidate;
                }

                cycleEnds[start] = end;

                start =
                    ClassicHeuristicGuard.FindNextPositiveDemand(
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
            ArrayPool<int>.Shared.Return(
                buffer,
                clearArray: false);
        }
    }
}
