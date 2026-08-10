using ULSAlgorithms.Models;

namespace ULSAlgorithms.Heuristics.Internal;

/// <summary>
/// Applies the published final-lot merge test used by the modified LUC and
/// modified PPB heuristics.
/// </summary>
internal static class LastReplenishmentMergeImprover
{
    /// <summary>
    /// Eliminates the final replenishment lot when moving its complete demand
    /// to the preceding replenishment period strictly reduces setup plus
    /// holding cost.
    /// </summary>
    public static bool TryMergeLastLot(
        UlsProblem problem,
        Span<int> cycleEnds)
    {
        ArgumentNullException.ThrowIfNull(problem);

        if (cycleEnds.Length < problem.Horizon)
        {
            throw new ArgumentException(
                "The cycle-end buffer is shorter than the problem horizon.",
                nameof(cycleEnds));
        }

        var previousStart = -1;
        var lastStart = -1;

        for (var period = 0; period < problem.Horizon; period++)
        {
            if (cycleEnds[period] < period)
            {
                continue;
            }

            previousStart = lastStart;
            lastStart = period;
        }

        if (previousStart < 0 || lastStart < 0)
        {
            return false;
        }

        var lastEnd = cycleEnds[lastStart];

        if (lastEnd < lastStart ||
            lastEnd >= problem.Horizon)
        {
            throw new InvalidOperationException(
                $"Invalid final replenishment cycle [{lastStart}, {lastEnd}].");
        }

        var lastQuantity = 0.0;
        var demands = problem.Demands;

        for (var period = lastStart; period <= lastEnd; period++)
        {
            lastQuantity += demands[period];

            if (!double.IsFinite(lastQuantity))
            {
                throw new ArithmeticException(
                    "Numerical overflow while evaluating the final-lot merge.");
            }
        }

        if (lastQuantity <= 0.0)
        {
            return false;
        }

        var holdingCost =
            problem.Horizon > 1
                ? problem.HoldingCosts[0]
                : 0.0;

        var extraHoldingCost =
            holdingCost *
            (lastStart - previousStart) *
            lastQuantity;

        if (!double.IsFinite(extraHoldingCost))
        {
            throw new ArithmeticException(
                "Numerical overflow while evaluating final-lot holding cost.");
        }

        var setupSaving = problem.SetupCosts[0];

        var scale =
            Math.Max(
                1.0,
                Math.Max(
                    Math.Abs(extraHoldingCost),
                    Math.Abs(setupSaving)));

        var tolerance = 1.0e-12 * scale;

        if (extraHoldingCost + tolerance >= setupSaving)
        {
            return false;
        }

        cycleEnds[previousStart] = lastEnd;
        cycleEnds[lastStart] = -1;

        return true;
    }
}
