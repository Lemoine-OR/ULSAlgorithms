using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics.Internal;

/// <summary>
/// Builds and validates a zero-backlogging heuristic solution from a set of
/// replenishment cycles.
/// </summary>
internal static class HeuristicSolutionBuilder
{
    public static UlsSolveResult Build(
        UlsProblem problem,
        ReadOnlySpan<int> cycleEnds,
        string solverName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var horizon = problem.Horizon;

        if (cycleEnds.Length < horizon)
        {
            throw new ArgumentException(
                "The cycle-end vector is shorter than the problem horizon.",
                nameof(cycleEnds));
        }

        var production = new double[horizon];
        var inventory = new double[horizon];
        var setup = new bool[horizon];

        var demands = problem.Demands;

        for (var start = 0; start < horizon; start++)
        {
            if ((start & 255) == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            var end = cycleEnds[start];

            if (end < 0)
            {
                continue;
            }

            if (end < start || end >= horizon)
            {
                throw new InvalidOperationException(
                    $"Invalid replenishment cycle [{start}, {end}].");
            }

            var quantity = 0.0;

            for (var period = start; period <= end; period++)
            {
                quantity += demands[period];

                if (!double.IsFinite(quantity))
                {
                    throw new ArithmeticException(
                        "Numerical overflow while accumulating a heuristic lot.");
                }
            }

            if (quantity == 0.0)
            {
                continue;
            }

            if (production[start] != 0.0)
            {
                throw new InvalidOperationException(
                    $"More than one replenishment cycle starts in period {start}.");
            }

            production[start] = quantity;
            setup[start] = true;
        }

        var setupCost = 0.0;
        var productionCost = 0.0;
        var holdingCost = 0.0;
        var stock = 0.0;

        var setupCosts = problem.SetupCosts;
        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        var tolerance =
            1e-10 * Math.Max(1.0, problem.TotalDemand);

        for (var period = 0; period < horizon; period++)
        {
            if ((period & 255) == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            stock += production[period] - demands[period];

            if (stock < -tolerance)
            {
                throw new InvalidOperationException(
                    $"The heuristic plan backlogs demand in period {period}.");
            }

            if (Math.Abs(stock) <= tolerance)
            {
                stock = 0.0;
            }

            inventory[period] = stock;

            if (setup[period])
            {
                setupCost = AddFinite(
                    setupCost,
                    setupCosts[period],
                    "heuristic setup cost");
            }

            productionCost = AddFinite(
                productionCost,
                MultiplyFinite(
                    production[period],
                    productionCosts[period],
                    "heuristic production cost"),
                "heuristic production cost");

            holdingCost = AddFinite(
                holdingCost,
                MultiplyFinite(
                    inventory[period],
                    holdingCosts[period],
                    "heuristic holding cost"),
                "heuristic holding cost");
        }

        if (Math.Abs(stock) > tolerance)
        {
            throw new InvalidOperationException(
                "The heuristic plan does not end with zero inventory.");
        }

        var solution = UlsSolution.FromOwnedBuffers(
            production,
            inventory,
            setup,
            setupCost,
            productionCost,
            holdingCost);

        return new UlsSolveResult(
            solverName,
            UlsSolveStatus.Feasible,
            solution,
            "Heuristic solution; no optimality proof is claimed.");
    }

    private static double AddFinite(
        double left,
        double right,
        string operation)
    {
        var value = left + right;

        if (!double.IsFinite(value))
        {
            throw new ArithmeticException(
                $"Numerical overflow while computing {operation}.");
        }

        return value;
    }

    private static double MultiplyFinite(
        double left,
        double right,
        string operation)
    {
        var value = left * right;

        if (!double.IsFinite(value))
        {
            throw new ArithmeticException(
                $"Numerical overflow while computing {operation}.");
        }

        return value;
    }
}
