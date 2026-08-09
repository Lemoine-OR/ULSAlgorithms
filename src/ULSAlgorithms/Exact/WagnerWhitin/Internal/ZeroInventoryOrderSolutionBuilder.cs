using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.WagnerWhitin.Internal;

/// <summary>
/// Reconstructs a zero-inventory-order ULS solution from shortest-path predecessors.
/// </summary>
internal static class ZeroInventoryOrderSolutionBuilder
{
    public static UlsSolveResult Build(
        UlsProblem problem,
        ReadOnlySpan<int> predecessor,
        string solverName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var horizon = problem.Horizon;
        if (predecessor.Length != horizon + 1)
        {
            throw new ArgumentException(
                $"Predecessor vector must contain {horizon + 1} entries.",
                nameof(predecessor));
        }

        var production = new double[horizon];
        var inventory = new double[horizon];
        var setup = new bool[horizon];

        var demands = problem.Demands;

        var end = horizon;
        while (end > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var start = predecessor[end];
            if (start < 0 || start >= end)
            {
                throw new InvalidOperationException(
                    "The shortest-path predecessor chain is inconsistent.");
            }

            var quantity = 0.0;
            for (var period = start; period < end; period++)
            {
                quantity = AddFinite(quantity, demands[period], "reconstructed production quantity");
            }

            if (quantity > 0.0)
            {
                production[start] = quantity;
                setup[start] = true;
            }

            end = start;
        }

        var runningInventory = 0.0;
        var setupCost = 0.0;
        var productionCost = 0.0;
        var holdingCost = 0.0;

        var setupCosts = problem.SetupCosts;
        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        for (var period = 0; period < horizon; period++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (setup[period])
            {
                setupCost = AddFinite(
                    setupCost,
                    setupCosts[period],
                    "solution setup cost");
            }

            productionCost = AddFinite(
                productionCost,
                MultiplyFinite(
                    productionCosts[period],
                    production[period],
                    "solution production cost"),
                "solution production cost");

            runningInventory = AddFinite(
                runningInventory,
                production[period],
                "inventory balance");

            runningInventory -= demands[period];

            var tolerance = 1e-10 * Math.Max(
                1.0,
                Math.Max(Math.Abs(runningInventory), Math.Abs(demands[period])));

            if (runningInventory < -tolerance)
            {
                throw new InvalidOperationException(
                    $"Reconstructed solution has negative inventory at period {period}.");
            }

            if (runningInventory < 0.0)
            {
                runningInventory = 0.0;
            }

            inventory[period] = runningInventory;

            holdingCost = AddFinite(
                holdingCost,
                MultiplyFinite(
                    holdingCosts[period],
                    inventory[period],
                    "solution holding cost"),
                "solution holding cost");
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
            UlsSolveStatus.Optimal,
            solution);
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
