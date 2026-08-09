using ULSAlgorithms.Models;

namespace ULSAlgorithms.Heuristics.Internal;

/// <summary>
/// Shared applicability checks for classical stationary-cost lot-sizing
/// heuristics.
/// </summary>
internal static class ClassicHeuristicGuard
{
    public static bool HasStationaryRelevantCosts(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var horizon = problem.Horizon;
        var setupCosts = problem.SetupCosts;
        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        var setupCost = setupCosts[0];
        var productionCost = productionCosts[0];

        for (var period = 1; period < horizon; period++)
        {
            if (setupCosts[period] != setupCost ||
                productionCosts[period] != productionCost)
            {
                return false;
            }
        }

        if (horizon > 1)
        {
            var holdingCost = holdingCosts[0];

            for (var period = 1; period < horizon - 1; period++)
            {
                if (holdingCosts[period] != holdingCost)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static void ThrowIfNotStationary(
        UlsProblem problem,
        string solverName)
    {
        if (!HasStationaryRelevantCosts(problem))
        {
            throw new NotSupportedException(
                $"{solverName} requires constant setup costs, constant unit " +
                "production costs and constant economically relevant holding costs.");
        }
    }

    public static int FindNextPositiveDemand(
        ReadOnlySpan<double> demands,
        int start)
    {
        for (var period = start; period < demands.Length; period++)
        {
            if (demands[period] > 0.0)
            {
                return period;
            }
        }

        return demands.Length;
    }
}
