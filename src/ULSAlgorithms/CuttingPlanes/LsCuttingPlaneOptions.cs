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
    /// Gets or sets the positive violation required before a cut is added.
    /// </summary>
    public double ViolationTolerance { get; set; } =
        1.0e-7;

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
    }
}
