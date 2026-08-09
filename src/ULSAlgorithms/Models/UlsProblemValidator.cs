namespace ULSAlgorithms.Models;

/// <summary>
/// Validates the numerical data of a classical finite-horizon ULS problem.
/// </summary>
public static class UlsProblemValidator
{
    /// <summary>
    /// Validates all period-dependent ULS input vectors.
    /// </summary>
    /// <param name="demands">Demand in each period.</param>
    /// <param name="setupCosts">Fixed setup cost in each period.</param>
    /// <param name="unitProductionCosts">Unit production cost in each period.</param>
    /// <param name="holdingCosts">
    /// Unit cost of holding one unit of end-of-period inventory in each period.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the horizon is empty, vector lengths differ, or a value is
    /// negative, NaN, or infinite.
    /// </exception>
    public static void Validate(
        ReadOnlySpan<double> demands,
        ReadOnlySpan<double> setupCosts,
        ReadOnlySpan<double> unitProductionCosts,
        ReadOnlySpan<double> holdingCosts)
    {
        var horizon = demands.Length;

        if (horizon == 0)
        {
            throw new ArgumentException(
                "A ULS problem must contain at least one period.",
                nameof(demands));
        }

        ValidateLength(setupCosts, horizon, nameof(setupCosts));
        ValidateLength(unitProductionCosts, horizon, nameof(unitProductionCosts));
        ValidateLength(holdingCosts, horizon, nameof(holdingCosts));

        ValidateNonNegativeFiniteVector(demands, nameof(demands));
        ValidateNonNegativeFiniteVector(setupCosts, nameof(setupCosts));
        ValidateNonNegativeFiniteVector(unitProductionCosts, nameof(unitProductionCosts));
        ValidateNonNegativeFiniteVector(holdingCosts, nameof(holdingCosts));
    }

    private static void ValidateLength(
        ReadOnlySpan<double> values,
        int expectedLength,
        string parameterName)
    {
        if (values.Length != expectedLength)
        {
            throw new ArgumentException(
                $"Vector '{parameterName}' must contain exactly {expectedLength} values, " +
                $"but contains {values.Length}.",
                parameterName);
        }
    }

    private static void ValidateNonNegativeFiniteVector(
        ReadOnlySpan<double> values,
        string parameterName)
    {
        for (var period = 0; period < values.Length; period++)
        {
            var value = values[period];

            if (!double.IsFinite(value) || value < 0.0)
            {
                throw new ArgumentException(
                    $"Vector '{parameterName}' contains an invalid value at period {period}: " +
                    $"{value}. ULS input values must be finite and non-negative.",
                    parameterName);
            }
        }
    }
}
