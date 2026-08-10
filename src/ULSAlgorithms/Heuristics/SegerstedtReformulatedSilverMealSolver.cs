using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements the reformulated Silver-Meal (rSM, "Lägsta periodkostnad")
/// heuristic of Segerstedt, Abdul-Jalbar and Samuelsson.
/// </summary>
/// <remarks>
/// <para>
/// Only periods with non-zero demand are candidate extension points. If
/// non-zero demands X-hat_i occur at periods t_i and the current lot starts at
/// t_0, the candidate average is
///
/// C_n = (A + h * sum(i=0..n) (t_i-t_0) X-hat_i)
///       / (t_n-t_0+1).
///
/// The lot ends immediately before the first non-zero candidate for which this
/// average increases.
/// </para>
/// <para>
/// Reference: A. Segerstedt, B. Abdul-Jalbar and B. Samuelsson,
/// "Reformulated Silver-Meal and Similar Lot Sizing Techniques",
/// Axioms 12(7), 661, 2023, DOI 10.3390/axioms12070661.
/// </para>
/// </remarks>
public sealed class SegerstedtReformulatedSilverMealSolver : IUlsSolver
{
    public string Name => "Segerstedt reformulated Silver-Meal";

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
                var accumulatedHolding = 0.0;
                var previousAverage = setupCost;

                var candidate =
                    ClassicHeuristicGuard.FindNextPositiveDemand(
                        demands,
                        start + 1);

                while (candidate < horizon)
                {
                    accumulatedHolding +=
                        holdingCost *
                        (candidate - start) *
                        demands[candidate];

                    if (!double.IsFinite(accumulatedHolding))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow while evaluating reformulated Silver-Meal.");
                    }

                    var elapsedPeriods =
                        candidate - start + 1;

                    var average =
                        (setupCost + accumulatedHolding) /
                        elapsedPeriods;

                    if (average > previousAverage)
                    {
                        break;
                    }

                    previousAverage = average;
                    bestEnd = candidate;

                    candidate =
                        ClassicHeuristicGuard.FindNextPositiveDemand(
                            demands,
                            candidate + 1);
                }

                cycleEnds[start] = bestEnd;

                start =
                    ClassicHeuristicGuard.FindNextPositiveDemand(
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
            ArrayPool<int>.Shared.Return(
                buffer,
                clearArray: false);
        }
    }
}
