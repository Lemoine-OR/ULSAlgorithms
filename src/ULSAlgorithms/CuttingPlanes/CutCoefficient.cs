namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Stores one nonzero coefficient of a generated linear inequality.
/// </summary>
public readonly record struct CutCoefficient
{
    /// <summary>Initializes a coefficient.</summary>
    public CutCoefficient(string variableName, double coefficient)
    {
        if (string.IsNullOrWhiteSpace(variableName))
        {
            throw new ArgumentException(
                "A variable name is required.",
                nameof(variableName));
        }

        if (!double.IsFinite(coefficient))
        {
            throw new ArgumentOutOfRangeException(
                nameof(coefficient),
                "A cut coefficient must be finite.");
        }

        VariableName = variableName.Trim();
        Coefficient = coefficient;
    }

    /// <summary>Gets the solver-independent variable identifier.</summary>
    public string VariableName { get; }

    /// <summary>Gets the coefficient value.</summary>
    public double Coefficient { get; }
}
