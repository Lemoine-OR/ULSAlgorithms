namespace ULSAlgorithms.Optimization.Modeling;

/// <summary>
/// Stores one coefficient of a portable linear expression.
/// </summary>
public readonly record struct LinearTerm
{
    /// <summary>Initializes a linear term.</summary>
    public LinearTerm(
        int variableId,
        double coefficient)
    {
        if (variableId < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(variableId));
        }

        if (!double.IsFinite(coefficient))
        {
            throw new ArgumentOutOfRangeException(
                nameof(coefficient),
                "A coefficient must be finite.");
        }

        VariableId = variableId;
        Coefficient = coefficient;
    }

    /// <summary>Gets the referenced variable identifier.</summary>
    public int VariableId { get; }

    /// <summary>Gets the coefficient.</summary>
    public double Coefficient { get; }
}
