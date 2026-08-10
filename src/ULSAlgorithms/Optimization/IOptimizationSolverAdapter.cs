namespace ULSAlgorithms.Optimization;

/// <summary>
/// Defines the common metadata and availability contract implemented by every
/// optimization-solver adapter used by solver-backed ULS algorithms.
/// </summary>
/// <remarks>
/// Concrete adapters are responsible for the real native-library / executable /
/// license smoke test for their solver. Automatic selection never infers
/// usability from a folder name alone.
/// </remarks>
public interface IOptimizationSolverAdapter
{
    /// <summary>Gets the stable adapter identifier.</summary>
    string AdapterId { get; }

    /// <summary>Gets the adapter display name.</summary>
    string AdapterName { get; }

    /// <summary>Gets the adapter implementation version.</summary>
    string AdapterVersion { get; }

    /// <summary>Gets the concrete solver targeted by this adapter.</summary>
    SolverKind SolverKind { get; }

    /// <summary>Gets the capabilities implemented by this adapter.</summary>
    IReadOnlyCollection<SolverCapability> Capabilities { get; }

    /// <summary>Tests whether the adapter exposes a capability.</summary>
    bool SupportsCapability(SolverCapability capability);

    /// <summary>
    /// Performs the adapter-specific machine discovery / load / license check.
    /// </summary>
    ValueTask<SolverAvailabilityInfo> CheckAvailabilityAsync(
        CancellationToken cancellationToken = default);
}
