using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements McLaren's Order Moment (MOM) lot-sizing heuristic.
/// </summary>
/// <remarks>
/// <para>
/// MOM combines an EOQ-derived time-between-orders estimate with a
/// part-period target. A lot is extended until accumulated part-periods reach
/// the Order Moment Target (OMT); the triggering demand is then subjected to
/// the published marginal holding/setup test before the lot is closed.
/// </para>
/// <para>
/// Original source: B. J. McLaren,
/// "A Study of Multiple Level Lot Sizing Procedures for Material Requirements
/// Planning Systems", Ph.D. dissertation, Purdue University, 1977.
/// </para>
/// <para>
/// Reconstructed operational rule and formula source:
/// L. Baciarello, M. D'Avino, R. Onori and M. M. Schiraldi,
/// "Lot Sizing Heuristics Performance", International Journal of Engineering
/// Business Management 5, 2013, DOI 10.5772/56004.
/// </para>
/// </remarks>
public sealed class McLarenOrderMomentSolver : IUlsSolver
{
    public string Name =>
        "McLaren Order Moment";

    public UlsSolverKind Kind =>
        UlsSolverKind.Heuristic;

    public static bool IsApplicable(
        UlsProblem problem) =>
        ClassicHeuristicGuard.HasStationaryRelevantCosts(problem);

    /// <summary>
    /// Computes the Order Moment Target for the supplied stationary-cost
    /// problem.
    /// </summary>
    public static double GetOrderMomentTarget(
        UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        ClassicHeuristicGuard.ThrowIfNotStationary(
            problem,
            "McLaren Order Moment");

        if (problem.TotalDemand == 0.0)
        {
            return 0.0;
        }

        int horizon = problem.Horizon;
        double holdingCost =
            horizon > 1
                ? problem.HoldingCosts[0]
                : 0.0;

        if (holdingCost == 0.0)
        {
            return double.PositiveInfinity;
        }

        double averageDemand =
            problem.TotalDemand /
            horizon;

        double denominator =
            holdingCost *
            averageDemand;

        if (denominator == 0.0)
        {
            return double.PositiveInfinity;
        }

        double ratio =
            2.0 *
            problem.SetupCosts[0] /
            denominator;

        if (double.IsPositiveInfinity(ratio))
        {
            return double.PositiveInfinity;
        }

        if (!double.IsFinite(ratio) ||
            ratio < 0.0)
        {
            throw new ArithmeticException(
                "Non-finite EOQ-derived ratio while computing the Order Moment Target.");
        }

        double timeBetweenOrders =
            Math.Sqrt(ratio);

        double truncatedTimeBetweenOrders =
            Math.Floor(timeBetweenOrders);

        double integerMoment =
            truncatedTimeBetweenOrders *
            (truncatedTimeBetweenOrders - 1.0) /
            2.0;

        double fractionalMoment =
            (timeBetweenOrders -
             truncatedTimeBetweenOrders) *
            truncatedTimeBetweenOrders;

        double target =
            averageDemand *
            (integerMoment +
             fractionalMoment);

        if (!double.IsFinite(target))
        {
            return double.PositiveInfinity;
        }

        return target;
    }

    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        ClassicHeuristicGuard.ThrowIfNotStationary(
            problem,
            Name);

        int horizon = problem.Horizon;
        int[] buffer =
            ArrayPool<int>.Shared.Rent(horizon);

        try
        {
            Span<int> cycleEnds =
                buffer.AsSpan(0, horizon);

            cycleEnds.Fill(-1);

            ReadOnlySpan<double> demands =
                problem.Demands;

            int start =
                ClassicHeuristicGuard.FindNextPositiveDemand(
                    demands,
                    0);

            if (start >= horizon)
            {
                return HeuristicSolutionBuilder.Build(
                    problem,
                    cycleEnds,
                    Name,
                    cancellationToken);
            }

            double holdingCost =
                horizon > 1
                    ? problem.HoldingCosts[0]
                    : 0.0;

            if (holdingCost == 0.0)
            {
                cycleEnds[start] =
                    horizon - 1;

                return HeuristicSolutionBuilder.Build(
                    problem,
                    cycleEnds,
                    Name,
                    cancellationToken);
            }

            double setupCost =
                problem.SetupCosts[0];

            double target =
                GetOrderMomentTarget(problem);

            while (start < horizon)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double partPeriods = 0.0;
                int selectedEnd = start;
                bool closed = false;

                for (int candidate = start + 1;
                     candidate < horizon;
                     candidate++)
                {
                    if ((candidate & 255) == 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    double demand =
                        demands[candidate];

                    if (demand == 0.0)
                    {
                        selectedEnd = candidate;
                        continue;
                    }

                    double candidatePartPeriods =
                        partPeriods +
                        (candidate - start) *
                        demand;

                    if (!double.IsFinite(candidatePartPeriods))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow while accumulating MOM part-periods.");
                    }

                    if (StrictlyBelow(
                            candidatePartPeriods,
                            target))
                    {
                        partPeriods =
                            candidatePartPeriods;

                        selectedEnd =
                            candidate;

                        continue;
                    }

                    double marginalHolding =
                        holdingCost *
                        (candidate - start) *
                        demand;

                    if (!double.IsFinite(marginalHolding))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow while evaluating the MOM marginal test.");
                    }

                    if (LessOrEqual(
                            marginalHolding,
                            setupCost))
                    {
                        selectedEnd =
                            candidate;
                    }
                    else
                    {
                        selectedEnd =
                            candidate - 1;
                    }

                    cycleEnds[start] =
                        selectedEnd;

                    start =
                        ClassicHeuristicGuard.FindNextPositiveDemand(
                            demands,
                            selectedEnd + 1);

                    closed = true;
                    break;
                }

                if (!closed)
                {
                    cycleEnds[start] =
                        horizon - 1;

                    break;
                }
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

    private static bool StrictlyBelow(
        double value,
        double target)
    {
        if (double.IsPositiveInfinity(target))
        {
            return true;
        }

        double tolerance =
            1.0e-12 *
            Math.Max(
                1.0,
                Math.Max(
                    Math.Abs(value),
                    Math.Abs(target)));

        return value <
               target - tolerance;
    }

    private static bool LessOrEqual(
        double left,
        double right)
    {
        double tolerance =
            1.0e-12 *
            Math.Max(
                1.0,
                Math.Max(
                    Math.Abs(left),
                    Math.Abs(right)));

        return left <=
               right + tolerance;
    }
}
