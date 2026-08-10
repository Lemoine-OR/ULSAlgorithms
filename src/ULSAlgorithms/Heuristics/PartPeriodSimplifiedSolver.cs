using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements the Part-Period Simplified (PPS) rule, also described as the
/// Least Total Cost (LTC) no-overshoot part-period rule.
/// </summary>
/// <remarks>
/// <para>
/// Starting at the first uncovered positive demand, PPS extends the lot while
/// accumulated part-periods remain less than or equal to the Economic Part
/// Period A/h. Unlike Part-Period Balancing, PPS does not compare the first
/// point above A/h with the last point below it.
/// </para>
/// <para>
/// Primary historical source: J. J. DeMatteis,
/// "An Economic Lot-Sizing Technique I: The Part-Period Algorithm",
/// IBM Systems Journal 7(1), 30-38, 1968.
/// </para>
/// <para>
/// The explicit distinction between Part-Period Simplified and
/// Part-Period Balancing is documented in L. Baciarello, M. D'Avino,
/// R. Onori and M. M. Schiraldi, "Lot Sizing Heuristics Performance",
/// International Journal of Engineering Business Management 5, 2013,
/// DOI 10.5772/56004.
/// </para>
/// </remarks>
public sealed class PartPeriodSimplifiedSolver : IUlsSolver
{
    public string Name => "Part-Period Simplified";

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

                for (var end = start + 1; end < horizon; end++)
                {
                    var candidatePartPeriods =
                        partPeriods +
                        (end - start) *
                        demands[end];

                    if (!double.IsFinite(candidatePartPeriods))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow while accumulating part-periods.");
                    }

                    if (candidatePartPeriods > epp)
                    {
                        break;
                    }

                    partPeriods = candidatePartPeriods;
                    bestEnd = end;
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
