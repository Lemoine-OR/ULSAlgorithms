using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// Normalizes raw mathematical-variable values returned by optimization
/// solvers before independent validation, objective reconstruction and ULS
/// solution mapping.
/// </summary>
/// <remarks>
/// <para>
/// Solver values are floating-point values and can contain harmless numerical
/// residuals such as -5E-9 or 180.00000000000006.
/// </para>
/// <para>
/// This class deliberately follows the same numerical-cleanup policy and
/// default tolerances as LotSizingDataModel's
/// MathematicalVariableValueNormalizer.
/// </para>
/// <para>
/// Materially fractional or materially negative values are never silently
/// repaired.
/// </para>
/// </remarks>
public sealed class LinearVariableValueNormalizer
{
    /// <summary>
    /// Default absolute tolerance used to identify numerical zero.
    /// </summary>
    public const double DefaultZeroTolerance =
        1.0e-8;
    /// <summary>
    /// Historical public default absolute tolerance used for integer-domain
    /// variables.
    /// </summary>
    /// <remarks>
    /// This value is retained for 1.x public-API compatibility.
    /// Solver-backed execution uses
    /// <see cref="SolverIntegralityTolerance"/> by default.
    /// </remarks>
    public const double DefaultIntegralityTolerance =
        1.0e-7;

    /// <summary>
    /// Default integrality tolerance used when normalizing values returned by
    /// external mathematical-programming solvers.
    /// </summary>
    /// <remarks>
    /// The wider tolerance accommodates harmless solver residuals while the
    /// normalized solution remains subject to independent model-feasibility
    /// validation.
    /// </remarks>
    public const double SolverIntegralityTolerance =
        1.0e-5;

    /// <summary>
    /// Default absolute tolerance used to clean continuous values that are
    /// numerically indistinguishable from an integer.
    /// </summary>
    public const double DefaultNearIntegerTolerance =
        1.0e-8;

    /// <summary>Initializes the normalizer with repository defaults.</summary>
    public LinearVariableValueNormalizer()
        : this(
            DefaultZeroTolerance,
            SolverIntegralityTolerance,
            DefaultNearIntegerTolerance)
    {
    }

    /// <summary>Initializes the normalizer with explicit tolerances.</summary>
    public LinearVariableValueNormalizer(
        double zeroTolerance,
        double integralityTolerance,
        double nearIntegerTolerance)
    {
        ValidateTolerance(
            zeroTolerance,
            nameof(zeroTolerance));

        ValidateTolerance(
            integralityTolerance,
            nameof(integralityTolerance));

        ValidateTolerance(
            nearIntegerTolerance,
            nameof(nearIntegerTolerance));

        ZeroTolerance =
            zeroTolerance;

        IntegralityTolerance =
            integralityTolerance;

        NearIntegerTolerance =
            nearIntegerTolerance;
    }

    /// <summary>Gets the zero tolerance.</summary>
    public double ZeroTolerance { get; }

    /// <summary>Gets the integrality tolerance.</summary>
    public double IntegralityTolerance { get; }

    /// <summary>Gets the near-integer tolerance for continuous values.</summary>
    public double NearIntegerTolerance { get; }

    /// <summary>
    /// Normalizes one raw solver value according to the variable domain.
    /// </summary>
    public double Normalize(
        LinearVariable variable,
        double rawValue)
    {
        ArgumentNullException.ThrowIfNull(
            variable);

        if (!double.IsFinite(rawValue))
        {
            throw new ArgumentOutOfRangeException(
                nameof(rawValue),
                rawValue,
                "A solver variable value must be finite.");
        }

        if (Math.Abs(rawValue) <=
            ZeroTolerance)
        {
            return 0.0;
        }

        return variable.Type switch
        {
            LinearVariableType.Binary =>
                NormalizeBinary(
                    rawValue),

            LinearVariableType.Integer =>
                NormalizeInteger(
                    rawValue),

            LinearVariableType.Continuous =>
                NormalizeContinuous(
                    rawValue),

            _ =>
                NormalizeContinuous(
                    rawValue)
        };
    }

    private double NormalizeBinary(
        double rawValue)
    {
        // A binary value is solver-reported floating-point data.  Normalize
        // symmetrically around both admissible integer values using the
        // configured integrality tolerance.  The normalized solution is then
        // independently checked against every model constraint.
        if (Math.Abs(rawValue) <=
            IntegralityTolerance)
        {
            return 0.0;
        }

        if (Math.Abs(
                rawValue - 1.0) <=
            IntegralityTolerance)
        {
            return 1.0;
        }

        throw new InvalidOperationException(
            $"Binary solver value '{rawValue:G17}' is not within " +
            $"the configured integrality tolerance " +
            $"'{IntegralityTolerance:G17}' of 0 or 1.");
    }
    private double NormalizeInteger(
        double rawValue)
    {
        double nearestInteger =
            Math.Round(
                rawValue,
                MidpointRounding.AwayFromZero);

        return Math.Abs(
                   rawValue -
                   nearestInteger) <=
               IntegralityTolerance
            ? nearestInteger
            : rawValue;
    }

    private double NormalizeContinuous(
        double rawValue)
    {
        double nearestInteger =
            Math.Round(
                rawValue,
                MidpointRounding.AwayFromZero);

        return Math.Abs(
                   rawValue -
                   nearestInteger) <=
               NearIntegerTolerance
            ? nearestInteger
            : rawValue;
    }

    private static void ValidateTolerance(
        double tolerance,
        string parameterName)
    {
        if (!double.IsFinite(tolerance) ||
            tolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                tolerance,
                "A numerical tolerance must be finite and non-negative.");
        }
    }
}



