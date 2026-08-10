namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Describes what happened to a generated cut after separation.
/// </summary>
public enum CutDisposition
{
    /// <summary>The cut has been generated but no disposition was recorded.</summary>
    Generated = 0,

    /// <summary>The cut was added to the mathematical model.</summary>
    Added = 1,

    /// <summary>An equivalent cut was already present.</summary>
    Duplicate = 2,

    /// <summary>The violation was below the configured insertion tolerance.</summary>
    BelowTolerance = 3,

    /// <summary>The cut was rejected by an implementation-level validity check.</summary>
    Invalid = 4,

    /// <summary>The solver adapter refused or failed to add the constraint.</summary>
    SolverRejected = 5,

    /// <summary>
    /// The cut was eligible and violated but was deliberately not selected by
    /// the configured cut-pool policy.
    /// </summary>
    NotSelected = 6
}
