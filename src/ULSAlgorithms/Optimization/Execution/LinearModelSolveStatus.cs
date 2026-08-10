namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// Describes the termination state of a solver-backed portable linear model.
/// </summary>
public enum LinearModelSolveStatus
{
    /// <summary>No reliable status was obtained.</summary>
    Unknown = 0,

    /// <summary>The model was solved to proven optimality.</summary>
    Optimal = 1,

    /// <summary>A feasible solution was returned without an optimality proof.</summary>
    Feasible = 2,

    /// <summary>The model was proven infeasible.</summary>
    Infeasible = 3,

    /// <summary>The model was proven unbounded.</summary>
    Unbounded = 4,

    /// <summary>The solver could not distinguish infeasibility from unboundedness.</summary>
    InfeasibleOrUnbounded = 5,

    /// <summary>The computation was cancelled by the caller.</summary>
    Cancelled = 6,

    /// <summary>No usable optimization solver was available.</summary>
    SolverUnavailable = 7,

    /// <summary>The solver invocation, translation, or independent validation failed.</summary>
    Failed = 8
}
