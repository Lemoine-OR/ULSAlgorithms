namespace ULSAlgorithms.Optimization;

/// <summary>
/// Identifies an optional capability exposed by an optimization-solver adapter.
/// </summary>
public enum SolverCapability
{
    /// <summary>No capability has been specified.</summary>
    Unknown = 0,

    /// <summary>Continuous linear programming.</summary>
    LinearProgramming = 1,

    /// <summary>Mixed-integer linear programming.</summary>
    MixedIntegerLinearProgramming = 2,

    /// <summary>Quadratic programming.</summary>
    QuadraticProgramming = 3,

    /// <summary>Mixed-integer quadratic programming.</summary>
    MixedIntegerQuadraticProgramming = 4,

    /// <summary>Progress callbacks.</summary>
    ProgressCallbacks = 5,

    /// <summary>Incumbent callbacks.</summary>
    IncumbentCallbacks = 6,

    /// <summary>User-defined cutting-plane callbacks.</summary>
    UserCutCallbacks = 7,

    /// <summary>Lazy-constraint callbacks.</summary>
    LazyConstraintCallbacks = 8,

    /// <summary>Heuristic-solution callbacks.</summary>
    HeuristicCallbacks = 9,

    /// <summary>Branching callbacks.</summary>
    BranchCallbacks = 10,

    /// <summary>Search-tree node callbacks.</summary>
    NodeCallbacks = 11,

    /// <summary>Cooperative interruption.</summary>
    Interruption = 12,

    /// <summary>Warm starts or MIP starts.</summary>
    WarmStart = 13,

    /// <summary>Multiple-solution retrieval.</summary>
    SolutionPool = 14,

    /// <summary>LP-format export.</summary>
    LpExport = 15,

    /// <summary>MPS-format export.</summary>
    MpsExport = 16,

    /// <summary>LP-format import.</summary>
    LpImport = 17,

    /// <summary>MPS-format import.</summary>
    MpsImport = 18,

    /// <summary>Infeasibility analysis such as IIS extraction.</summary>
    InfeasibilityAnalysis = 19,

    /// <summary>Conflict refinement.</summary>
    ConflictRefinement = 20,

    /// <summary>Deterministic parallel optimization.</summary>
    DeterministicParallelism = 21,

    /// <summary>Solver-native log capture.</summary>
    LogCapture = 22,

    /// <summary>Best-bound and optimality-gap reporting.</summary>
    OptimalityGapReporting = 23,

    /// <summary>Search-node and iteration statistics.</summary>
    SearchStatistics = 24
}
