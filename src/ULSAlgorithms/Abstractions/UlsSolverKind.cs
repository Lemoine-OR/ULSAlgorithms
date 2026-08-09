namespace ULSAlgorithms.Abstractions;

/// <summary>
/// Identifies the broad family of a ULS solution strategy.
/// </summary>
public enum UlsSolverKind
{
    /// <summary>
    /// An exact algorithm that proves optimality when it returns an optimal solution.
    /// </summary>
    Exact = 0,

    /// <summary>
    /// A heuristic algorithm that seeks a feasible solution without an optimality proof.
    /// </summary>
    Heuristic = 1
}
