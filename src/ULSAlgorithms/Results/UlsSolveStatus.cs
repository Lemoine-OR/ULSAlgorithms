namespace ULSAlgorithms.Results;

/// <summary>
/// Describes the mathematical status of a ULS solve.
/// </summary>
public enum UlsSolveStatus
{
    /// <summary>
    /// The solver has not produced a mathematical conclusion.
    /// </summary>
    NotSolved = 0,

    /// <summary>
    /// A globally optimal solution has been found.
    /// </summary>
    Optimal = 1,

    /// <summary>
    /// A feasible solution has been found without an optimality proof.
    /// </summary>
    Feasible = 2,

    /// <summary>
    /// The problem has been proven infeasible.
    /// </summary>
    Infeasible = 3,

    /// <summary>
    /// The solver stopped because of a time or resource limit.
    /// A feasible incumbent may still be attached to the result.
    /// </summary>
    LimitReached = 4,

    /// <summary>
    /// The solver failed before producing a valid mathematical conclusion.
    /// </summary>
    Failed = 5
}
