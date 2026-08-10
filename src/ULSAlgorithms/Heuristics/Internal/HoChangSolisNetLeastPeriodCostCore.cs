using ULSAlgorithms.Models;

namespace ULSAlgorithms.Heuristics.Internal;

/// <summary>
/// Shared incremental implementation of the Ho-Chang-Solis net average period
/// cost recursion.
/// </summary>
internal static class HoChangSolisNetLeastPeriodCostCore
{
    public static void BuildCycleEnds(
        UlsProblem problem,
        Span<int> cycleEnds,
        bool useImprovedTieBreak,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(problem);

        int horizon = problem.Horizon;

        if (cycleEnds.Length < horizon)
        {
            throw new ArgumentException(
                "The cycle-end buffer is shorter than the problem horizon.",
                nameof(cycleEnds));
        }

        cycleEnds[..horizon].Fill(-1);

        ReadOnlySpan<double> demands = problem.Demands;
        double setupCost = problem.SetupCosts[0];
        double holdingCost =
            horizon > 1
                ? problem.HoldingCosts[0]
                : 0.0;

        int start =
            ClassicHeuristicGuard.FindNextPositiveDemand(
                demands,
                0);

        while (start < horizon)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double accumulatedHolding = 0.0;
            int nonZeroDemandPeriods = 1;
            double previousNetAverage = setupCost;
            bool closed = false;

            for (int candidate = start + 1;
                 candidate < horizon;
                 candidate++)
            {
                if ((candidate & 255) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                double demand = demands[candidate];

                // Ho, Chang and Solis explicitly skip the stopping test for
                // zero-demand periods.
                if (demand == 0.0)
                {
                    continue;
                }

                double additionalHolding =
                    holdingCost *
                    (candidate - start) *
                    demand;

                double candidateHolding =
                    AddFinite(
                        accumulatedHolding,
                        additionalHolding,
                        "Ho-Chang-Solis holding accumulation");

                int candidateNonZeroPeriods =
                    nonZeroDemandPeriods + 1;

                double candidateNetAverage =
                    (setupCost + candidateHolding) /
                    candidateNonZeroPeriods;

                if (!double.IsFinite(candidateNetAverage))
                {
                    throw new ArithmeticException(
                        "Non-finite net average period cost.");
                }

                bool increasing =
                    StrictlyGreater(
                        candidateNetAverage,
                        previousNetAverage);

                bool improvedTie =
                    useImprovedTieBreak &&
                    NearlyEqual(
                        candidateNetAverage,
                        previousNetAverage) &&
                    NearlyEqual(
                        candidateNetAverage,
                        setupCost);

                if (increasing || improvedTie)
                {
                    cycleEnds[start] =
                        candidate - 1;

                    start = candidate;
                    closed = true;
                    break;
                }

                accumulatedHolding =
                    candidateHolding;

                nonZeroDemandPeriods =
                    candidateNonZeroPeriods;

                previousNetAverage =
                    candidateNetAverage;
            }

            if (!closed)
            {
                cycleEnds[start] =
                    horizon - 1;

                break;
            }
        }
    }

    private static bool StrictlyGreater(
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

        return left >
               right + tolerance;
    }

    private static bool NearlyEqual(
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

        return Math.Abs(left - right) <=
               tolerance;
    }

    private static double AddFinite(
        double left,
        double right,
        string operation)
    {
        double value = left + right;

        if (!double.IsFinite(value))
        {
            throw new ArithmeticException(
                $"Numerical overflow while computing {operation}.");
        }

        return value;
    }
}
