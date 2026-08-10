namespace ULSAlgorithms.Optimization.Modeling;

/// <summary>
/// Describes one variable in a portable linear mathematical model.
/// </summary>
public sealed class LinearVariable
{
    /// <summary>Initializes a variable.</summary>
    public LinearVariable(
        int id,
        string name,
        LinearVariableType type,
        double lowerBound,
        double upperBound)
    {
        if (id < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A variable name is required.",
                nameof(name));
        }

        if (!double.IsFinite(lowerBound) ||
            (!double.IsFinite(upperBound) &&
             !double.IsPositiveInfinity(upperBound)))
        {
            throw new ArgumentOutOfRangeException(
                nameof(lowerBound),
                "Variable bounds must be finite, except for +infinity.");
        }

        if (lowerBound > upperBound)
        {
            throw new ArgumentException(
                "The lower bound cannot exceed the upper bound.");
        }

        if (type == LinearVariableType.Binary &&
            (lowerBound < 0.0 || upperBound > 1.0))
        {
            throw new ArgumentException(
                "Binary-variable bounds must be contained in [0,1].");
        }

        Id = id;
        Name = name.Trim();
        Type = type;
        LowerBound = lowerBound;
        UpperBound = upperBound;
    }

    /// <summary>Gets the stable zero-based variable identifier.</summary>
    public int Id { get; }

    /// <summary>Gets the solver-independent variable name.</summary>
    public string Name { get; }

    /// <summary>Gets the variable domain.</summary>
    public LinearVariableType Type { get; }

    /// <summary>Gets the lower bound.</summary>
    public double LowerBound { get; }

    /// <summary>Gets the upper bound.</summary>
    public double UpperBound { get; }
}
