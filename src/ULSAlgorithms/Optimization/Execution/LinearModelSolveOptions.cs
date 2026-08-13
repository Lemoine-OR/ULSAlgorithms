namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// Configures one solver-backed execution of a portable linear model.
/// </summary>
public sealed class LinearModelSolveOptions
{
    /// <summary>
    /// Gets or sets the requested solver. The default is automatic selection.
    /// </summary>
    public SolverKind Solver { get; set; } =
        SolverKind.Automatic;

    /// <summary>
    /// Gets or sets whether an explicitly requested solver may fall back to
    /// another solver when unavailable.
    /// </summary>
    public bool AllowFallbackWhenExplicit { get; set; }

    /// <summary>
    /// Gets or sets the feasibility tolerance used by the independent solution
    /// checker.
    /// </summary>
    public double FeasibilityTolerance { get; set; } =
        1.0e-7;

    /// <summary>
    /// Gets or sets the tolerance used to identify numerical zero before
    /// validation and objective reconstruction.
    /// </summary>
    public double ZeroTolerance { get; set; } =
        LinearVariableValueNormalizer.DefaultZeroTolerance;

    /// <summary>
    /// Gets or sets the integrality tolerance used both for normalization and
    /// the independent solution checker.
    /// </summary>
    public double IntegralityTolerance { get; set; } =
        LinearVariableValueNormalizer.DefaultIntegralityTolerance;

    /// <summary>
    /// Gets or sets the tolerance used to clean continuous values that are
    /// numerically indistinguishable from an integer.
    /// </summary>
    public double NearIntegerTolerance { get; set; } =
        LinearVariableValueNormalizer.DefaultNearIntegerTolerance;

    /// <summary>
    /// Gets or sets whether a solver candidate rejected only after numerical
    /// normalization may be recovered by fixing all integer decisions to their
    /// normalized values and re-optimizing the remaining continuous model.
    /// </summary>
    /// <remarks>
    /// Polishing never upgrades a tolerance-only MIP result to proven optimal.
    /// The original solver-reported optimality status is preserved.
    /// </remarks>
    public bool EnableFixedIntegerPolishing { get; set; } = true;
    /// <summary>
    /// Gets or sets an optional path receiving the exact LP model submitted to
    /// the selected solver.
    /// </summary>
    public string ExportModelPath { get; set; } =
        string.Empty;

    /// <summary>
    /// Gets or sets whether temporary model/solution/log artifacts are retained.
    /// </summary>
    public bool KeepTemporaryFiles { get; set; }

    /// <summary>
    /// Gets or sets an optional parent directory for temporary solver artifacts.
    /// </summary>
    public string TemporaryRootPath { get; set; } =
        string.Empty;

    /// <summary>Validates this option set.</summary>
    public void EnsureValid()
    {
        if (Solver == SolverKind.Unknown)
        {
            throw new InvalidOperationException(
                "The requested solver cannot be Unknown.");
        }

        if (!double.IsFinite(FeasibilityTolerance) ||
            FeasibilityTolerance <= 0.0)
        {
            throw new InvalidOperationException(
                "FeasibilityTolerance must be finite and strictly positive.");
        }

        ValidateNonNegativeTolerance(
            ZeroTolerance,
            nameof(ZeroTolerance));

        if (!double.IsFinite(IntegralityTolerance) ||
            IntegralityTolerance <= 0.0)
        {
            throw new InvalidOperationException(
                "IntegralityTolerance must be finite and strictly positive.");
        }

        ValidateNonNegativeTolerance(
            NearIntegerTolerance,
            nameof(NearIntegerTolerance));
    }

    private static void ValidateNonNegativeTolerance(
        double tolerance,
        string name)
    {
        if (!double.IsFinite(tolerance) ||
            tolerance < 0.0)
        {
            throw new InvalidOperationException(
                $"{name} must be finite and non-negative.");
        }
    }
}

