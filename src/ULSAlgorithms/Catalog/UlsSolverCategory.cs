namespace ULSAlgorithms.Catalog;

/// <summary>
/// Identifies the operational category of a public ULS strategy.
/// </summary>
public enum UlsSolverCategory
{
    /// <summary>
    /// Direct exact algorithm that does not require an external mathematical
    /// programming engine.
    /// </summary>
    DirectExact = 0,

    /// <summary>
    /// Exact mathematical formulation solved through the portable external
    /// optimization layer.
    /// </summary>
    OptimizationFormulation = 1,

    /// <summary>
    /// Exact cutting-plane strategy solved through the portable external
    /// optimization layer.
    /// </summary>
    CuttingPlane = 2,

    /// <summary>
    /// Constructive or improvement heuristic without an optimality proof.
    /// </summary>
    Heuristic = 3
}
