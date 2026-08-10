namespace ULSAlgorithms.Optimization.Modeling;

/// <summary>
/// Identifies the sense of a portable linear constraint.
/// </summary>
public enum LinearConstraintSense
{
    /// <summary>Less than or equal.</summary>
    LessOrEqual = 0,

    /// <summary>Equality.</summary>
    Equal = 1,

    /// <summary>Greater than or equal.</summary>
    GreaterOrEqual = 2
}
