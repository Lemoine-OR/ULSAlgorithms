using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Validation;

/// <summary>
/// Independently verifies a ULS production plan against the original
/// <see cref="UlsProblem"/>.
/// </summary>
public static class UlsSolutionValidator
{
    /// <summary>
    /// Validates inventory balance, nonnegativity, setup linking, final
    /// inventory and all objective components.
    /// </summary>
    public static UlsSolutionValidationResult Validate(
        UlsProblem problem,
        UlsSolution solution,
        double tolerance = 1.0e-7)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(solution);

        if (!double.IsFinite(tolerance) ||
            tolerance <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tolerance));
        }

        if (solution.Horizon != problem.Horizon)
        {
            return new UlsSolutionValidationResult(
                false,
                double.PositiveInfinity,
                double.PositiveInfinity,
                0,
                double.NaN,
                double.NaN,
                double.NaN,
                double.PositiveInfinity,
                [
                    $"Solution horizon {solution.Horizon} differs from " +
                    $"problem horizon {problem.Horizon}."
                ]);
        }

        ReadOnlySpan<double> production =
            solution.ProductionQuantities;

        ReadOnlySpan<double> inventory =
            solution.EndingInventories;

        ReadOnlySpan<bool> setup =
            solution.SetupDecisions;

        double maximumBalanceResidual = 0.0;
        double maximumScaledBalanceResidual = 0.0;
        int setupLinkViolations = 0;

        double previousInventory = 0.0;

        double setupCost = 0.0;
        double productionCost = 0.0;
        double holdingCost = 0.0;

        var diagnostics =
            new List<string>();

        for (int period = 0;
             period < problem.Horizon;
             period++)
        {
            double balanceResidual =
                previousInventory +
                production[period] -
                problem.Demands[period] -
                inventory[period];

            double absoluteBalanceResidual =
                Math.Abs(
                    balanceResidual);

            maximumBalanceResidual =
                Math.Max(
                    maximumBalanceResidual,
                    absoluteBalanceResidual);

            double scale =
                Math.Max(
                    1.0,
                    Math.Max(
                        Math.Abs(previousInventory),
                        Math.Max(
                            Math.Abs(production[period]),
                            Math.Max(
                                Math.Abs(problem.Demands[period]),
                                Math.Abs(inventory[period])))));

            maximumScaledBalanceResidual =
                Math.Max(
                    maximumScaledBalanceResidual,
                    absoluteBalanceResidual / scale);

            if (production[period] >
                    tolerance *
                    Math.Max(
                        1.0,
                        Math.Abs(production[period])) &&
                !setup[period])
            {
                setupLinkViolations++;
            }

            if (setup[period])
            {
                setupCost +=
                    problem.SetupCosts[period];
            }

            productionCost +=
                production[period] *
                problem.UnitProductionCosts[period];

            holdingCost +=
                inventory[period] *
                problem.HoldingCosts[period];

            previousInventory =
                inventory[period];
        }

        double finalInventoryResidual =
            Math.Abs(
                inventory[^1]);

        double inventoryTolerance =
            tolerance *
            Math.Max(
                1.0,
                problem.TotalDemand);

        double setupDifference =
            Math.Abs(
                solution.SetupCost -
                setupCost);

        double productionDifference =
            Math.Abs(
                solution.ProductionCost -
                productionCost);

        double holdingDifference =
            Math.Abs(
                solution.HoldingCost -
                holdingCost);

        double totalDifference =
            Math.Abs(
                solution.TotalCost -
                (setupCost +
                 productionCost +
                 holdingCost));

        double maximumCostDifference =
            Math.Max(
                Math.Max(
                    setupDifference,
                    productionDifference),
                Math.Max(
                    holdingDifference,
                    totalDifference));

        double costScale =
            Math.Max(
                1.0,
                Math.Max(
                    Math.Abs(solution.TotalCost),
                    Math.Abs(
                        setupCost +
                        productionCost +
                        holdingCost)));

        bool feasible =
            maximumScaledBalanceResidual <= tolerance &&
            finalInventoryResidual <= inventoryTolerance &&
            setupLinkViolations == 0 &&
            maximumCostDifference <=
                tolerance * costScale;

        if (maximumScaledBalanceResidual > tolerance)
        {
            diagnostics.Add(
                $"Maximum scaled inventory-balance residual is " +
                $"{maximumScaledBalanceResidual:R}.");
        }

        if (finalInventoryResidual > inventoryTolerance)
        {
            diagnostics.Add(
                $"Final inventory residual is " +
                $"{finalInventoryResidual:R}.");
        }

        if (setupLinkViolations > 0)
        {
            diagnostics.Add(
                $"{setupLinkViolations} period(s) contain positive " +
                "production without an active setup.");
        }

        if (maximumCostDifference >
            tolerance * costScale)
        {
            diagnostics.Add(
                $"Maximum objective-component discrepancy is " +
                $"{maximumCostDifference:R}.");
        }

        return new UlsSolutionValidationResult(
            feasible,
            maximumBalanceResidual,
            finalInventoryResidual,
            setupLinkViolations,
            setupCost,
            productionCost,
            holdingCost,
            maximumCostDifference,
            diagnostics);
    }
}
