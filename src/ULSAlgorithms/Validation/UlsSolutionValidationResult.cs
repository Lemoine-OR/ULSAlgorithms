namespace ULSAlgorithms.Validation;

/// <summary>
/// Independent ULS-domain validation report for one production plan.
/// </summary>
public sealed class UlsSolutionValidationResult
{
    /// <summary>Initializes a ULS validation report.</summary>
    public UlsSolutionValidationResult(
        bool isFeasible,
        double maximumBalanceResidual,
        double finalInventoryResidual,
        int setupLinkViolations,
        double recomputedSetupCost,
        double recomputedProductionCost,
        double recomputedHoldingCost,
        double maximumCostDifference,
        IEnumerable<string> diagnostics)
    {
        IsFeasible = isFeasible;
        MaximumBalanceResidual = maximumBalanceResidual;
        FinalInventoryResidual = finalInventoryResidual;
        SetupLinkViolations = setupLinkViolations;
        RecomputedSetupCost = recomputedSetupCost;
        RecomputedProductionCost = recomputedProductionCost;
        RecomputedHoldingCost = recomputedHoldingCost;
        MaximumCostDifference = maximumCostDifference;
        Diagnostics = diagnostics.ToArray();
    }

    /// <summary>Gets whether the plan passed every ULS-domain check.</summary>
    public bool IsFeasible { get; }

    /// <summary>Gets the largest absolute inventory-balance residual.</summary>
    public double MaximumBalanceResidual { get; }

    /// <summary>Gets the absolute final inventory.</summary>
    public double FinalInventoryResidual { get; }

    /// <summary>Gets the number of periods with production but no setup.</summary>
    public int SetupLinkViolations { get; }

    /// <summary>Gets independently recomputed setup cost.</summary>
    public double RecomputedSetupCost { get; }

    /// <summary>Gets independently recomputed variable production cost.</summary>
    public double RecomputedProductionCost { get; }

    /// <summary>Gets independently recomputed holding cost.</summary>
    public double RecomputedHoldingCost { get; }

    /// <summary>
    /// Gets the maximum absolute difference between stored and independently
    /// recomputed cost components/total.
    /// </summary>
    public double MaximumCostDifference { get; }

    /// <summary>Gets validation diagnostics.</summary>
    public IReadOnlyList<string> Diagnostics { get; }

    /// <summary>Gets the independently recomputed total objective.</summary>
    public double RecomputedTotalCost =>
        RecomputedSetupCost +
        RecomputedProductionCost +
        RecomputedHoldingCost;
}
