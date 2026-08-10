using ULSAlgorithms.Models;

namespace ULSAlgorithms.Formulations.Internal;

internal static class UlsFormulationMath
{
    internal static double[] BuildSuffixDemand(
        UlsProblem problem)
    {
        var suffix =
            new double[problem.Horizon + 1];

        ReadOnlySpan<double> demand =
            problem.Demands;

        for (int period = problem.Horizon - 1;
             period >= 0;
             period--)
        {
            suffix[period] =
                AddFinite(
                    demand[period],
                    suffix[period + 1],
                    "suffix demand");
        }

        return suffix;
    }

    internal static double[] BuildCumulativeDemand(
        UlsProblem problem)
    {
        var cumulative =
            new double[problem.Horizon];

        double running = 0.0;

        for (int period = 0;
             period < problem.Horizon;
             period++)
        {
            running =
                AddFinite(
                    running,
                    problem.Demands[period],
                    "cumulative demand");

            cumulative[period] =
                running;
        }

        return cumulative;
    }

    internal static double DeliveredUnitCost(
        UlsProblem problem,
        int productionPeriod,
        int demandPeriod)
    {
        if (productionPeriod < 0 ||
            demandPeriod < productionPeriod ||
            demandPeriod >= problem.Horizon)
        {
            throw new ArgumentOutOfRangeException();
        }

        double cost =
            problem.UnitProductionCosts[productionPeriod];

        for (int period = productionPeriod;
             period < demandPeriod;
             period++)
        {
            cost =
                AddFinite(
                    cost,
                    problem.HoldingCosts[period],
                    "delivered unit cost");
        }

        return cost;
    }

    internal static double RegenerationArcCost(
        UlsProblem problem,
        int start,
        int endInclusive)
    {
        double quantity = 0.0;
        double variableCost = 0.0;

        for (int demandPeriod = start;
             demandPeriod <= endInclusive;
             demandPeriod++)
        {
            double demand =
                problem.Demands[demandPeriod];

            quantity =
                AddFinite(
                    quantity,
                    demand,
                    "regeneration quantity");

            variableCost =
                AddFinite(
                    variableCost,
                    MultiplyFinite(
                        demand,
                        DeliveredUnitCost(
                            problem,
                            start,
                            demandPeriod),
                        "regeneration variable cost"),
                    "regeneration variable cost");
        }

        return quantity == 0.0
            ? 0.0
            : AddFinite(
                problem.SetupCosts[start],
                variableCost,
                "regeneration arc cost");
    }

    internal static bool IsNoSpeculativeMotive(
        UlsProblem problem)
    {
        for (int period = 0;
             period < problem.Horizon - 1;
             period++)
        {
            double deliveredNext =
                problem.UnitProductionCosts[period] +
                problem.HoldingCosts[period];

            if (!double.IsFinite(deliveredNext) ||
                deliveredNext <
                    problem.UnitProductionCosts[period + 1])
            {
                return false;
            }
        }

        return true;
    }

    internal static double AddFinite(
        double left,
        double right,
        string operation)
    {
        double value =
            left + right;

        if (!double.IsFinite(value))
        {
            throw new ArithmeticException(
                $"Numerical overflow while computing {operation}.");
        }

        return value;
    }

    internal static double MultiplyFinite(
        double left,
        double right,
        string operation)
    {
        double value =
            left * right;

        if (!double.IsFinite(value))
        {
            throw new ArithmeticException(
                $"Numerical overflow while computing {operation}.");
        }

        return value;
    }
}
