using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements the Patterson-LaForge Incremental Part-Period Algorithm (IPPA).
/// </summary>
/// <remarks>
/// <para>
/// The candidate lot is extended while the cumulative incremental holding cost
/// remains no greater than the setup cost:
/// <c>h * sum((t-s) * d[t]) &lt;= A</c>.
/// </para>
/// <para>
/// Reference:
/// J. W. Patterson and R. L. LaForge,
/// "The Incremental Part-Period Algorithm: An Alternative to EOQ",
/// Journal of Purchasing and Materials Management 21(2), 28-33, 1985.
/// DOI: 10.1111/j.1745-493X.1985.tb00132.x.
/// </para>
/// <para>
/// Worst-case time is O(T); auxiliary working memory is O(T).
/// </para>
/// </remarks>
public sealed class PattersonLaForgeIncrementalPartPeriodSolver : IUlsSolver
{
    public string Name =>
        "Patterson-LaForge Incremental Part-Period";

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
                var cumulativeHolding = 0.0;

                for (var candidate = start + 1;
                     candidate < horizon;
                     candidate++)
                {
                    var increment =
                        holdingCost *
                        (candidate - start) *
                        demands[candidate];

                    var nextHolding =
                        cumulativeHolding + increment;

                    if (!double.IsFinite(nextHolding))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow in the IPPA criterion.");
                    }

                    if (nextHolding > setupCost)
                    {
                        break;
                    }

                    cumulativeHolding = nextHolding;
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
