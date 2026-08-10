namespace ULSAlgorithms.CuttingPlanes;

/// <summary>Identifies the sense of a generated linear inequality.</summary>
public enum LinearConstraintSense
{
    /// <summary>Less than or equal.</summary>
    LessOrEqual = 0,

    /// <summary>Equal.</summary>
    Equal = 1,

    /// <summary>Greater than or equal.</summary>
    GreaterOrEqual = 2
}
