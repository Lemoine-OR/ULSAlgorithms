namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Configures root LP (l,S) separation before the final exact MILP solve.
/// </summary>
public sealed class LsCuttingPlaneOptions
{
    /// <summary>
    /// Gets or sets the maximum number of root separation iterations.
    /// </summary>
    public int MaximumIterations { get; set; } =
        100;

    /// <summary>
    /// Gets or sets the positive violation required before a cut is eligible.
    /// </summary>
    public double ViolationTolerance { get; set; } =
        1.0e-7;

    /// <summary>
    /// Gets or sets the minimum efficacy required before a cut is eligible.
    /// The default zero preserves v0.20.0 behavior.
    /// </summary>
    public double MinimumEfficacy { get; set; }

    /// <summary>
    /// Gets or sets the cut-pool selection policy.
    /// </summary>
    public CutSelectionPolicy SelectionPolicy { get; set; } =
        CutSelectionPolicy.AllViolated;

    /// <summary>
    /// Gets or sets the maximum number of cuts selected per iteration when the
    /// policy is TopByViolation or TopByEfficacy.
    /// </summary>
    public int MaximumCutsPerIteration { get; set; } =
        25;

    /// <summary>Validates this option set.</summary>
    public void EnsureValid()
    {
        if (MaximumIterations <= 0)
        {
            throw new InvalidOperationException(
                "MaximumIterations must be strictly positive.");
        }

        if (!double.IsFinite(ViolationTolerance) ||
            ViolationTolerance < 0.0)
        {
            throw new InvalidOperationException(
                "ViolationTolerance must be finite and non-negative.");
        }

        if (!double.IsFinite(MinimumEfficacy) ||
            MinimumEfficacy < 0.0)
        {
            throw new InvalidOperationException(
                "MinimumEfficacy must be finite and non-negative.");
        }

        if (!Enum.IsDefined(SelectionPolicy))
        {
            throw new InvalidOperationException(
                "SelectionPolicy is not a supported cut-selection policy.");
        }

        if (MaximumCutsPerIteration <= 0)
        {
            throw new InvalidOperationException(
                "MaximumCutsPerIteration must be strictly positive.");
        }
    }
}
