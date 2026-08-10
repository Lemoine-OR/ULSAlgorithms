namespace ULSAlgorithms.Catalog;

/// <summary>
/// Identifies the constructor-level settings that can be supplied through the
/// configurable solver factory.
/// </summary>
[Flags]
public enum UlsSolverConfigurationCapabilities
{
    /// <summary>The strategy exposes no configurable factory settings.</summary>
    None = 0,

    /// <summary>
    /// The adaptive exact strategy can select its general exact fallback.
    /// </summary>
    AdaptiveGeneralFallback = 1 << 0,

    /// <summary>
    /// The strategy exposes shared-memory parallel execution controls.
    /// </summary>
    Parallelism = 1 << 1,

    /// <summary>
    /// The strategy accepts portable mathematical-optimization execution
    /// options, including explicit solver selection.
    /// </summary>
    OptimizationExecution = 1 << 2,

    /// <summary>
    /// The strategy accepts root (l,S) cutting-plane controls.
    /// </summary>
    CuttingPlane = 1 << 3
}
