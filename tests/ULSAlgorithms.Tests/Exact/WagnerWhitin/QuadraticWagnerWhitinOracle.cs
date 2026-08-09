using ULSAlgorithms.Models;

namespace ULSAlgorithms.Tests.Exact.WagnerWhitin;

/// <summary>
/// Independent O(n^2) forward dynamic-programming oracle used only by tests.
/// </summary>
internal static class QuadraticWagnerWhitinOracle
{
    public static double GetOptimalCost(
        UlsProblem problem,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var horizon = problem.Horizon;
        var demands = problem.Demands;
        var setupCosts = problem.SetupCosts;
        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        var value = new double[horizon + 1];
        Array.Fill(value, double.PositiveInfinity);
        value[0] = 0.0;

        for (var productionPeriod = 0;
             productionPeriod < horizon;
             productionPeriod++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (demands[productionPeriod] == 0.0)
            {
                value[productionPeriod + 1] = Math.Min(
                    value[productionPeriod + 1],
                    value[productionPeriod]);
            }

            if (!double.IsFinite(value[productionPeriod]))
            {
                continue;
            }

            var batchCost = setupCosts[productionPeriod];
            var deliveredUnitCost = productionCosts[productionPeriod];
            var cumulativeQuantity = 0.0;

            for (var demandPeriod = productionPeriod;
                 demandPeriod < horizon;
                 demandPeriod++)
            {
                cumulativeQuantity += demands[demandPeriod];
                batchCost += demands[demandPeriod] * deliveredUnitCost;

                if (cumulativeQuantity > 0.0)
                {
                    value[demandPeriod + 1] = Math.Min(
                        value[demandPeriod + 1],
                        value[productionPeriod] + batchCost);
                }

                deliveredUnitCost += holdingCosts[demandPeriod];
            }
        }

        return value[horizon];
    }
}
