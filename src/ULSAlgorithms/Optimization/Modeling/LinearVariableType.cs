namespace ULSAlgorithms.Optimization.Modeling;

/// <summary>
/// Identifies the domain of a variable in a portable linear mathematical model.
/// </summary>
public enum LinearVariableType
{
    /// <summary>A continuous real-valued variable.</summary>
    Continuous = 0,

    /// <summary>A binary variable restricted to zero or one.</summary>
    Binary = 1,

    /// <summary>An integer-valued variable.</summary>
    Integer = 2
}
