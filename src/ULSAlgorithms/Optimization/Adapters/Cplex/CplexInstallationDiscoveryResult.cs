namespace ULSAlgorithms.Optimization.Adapters.Cplex;

/// <summary>
/// Result of searching the current machine for IBM ILOG CPLEX.
/// </summary>
public sealed class CplexInstallationDiscoveryResult
{
    private readonly string[] _diagnostics;

    /// <summary>Initializes a discovery result.</summary>
    public CplexInstallationDiscoveryResult(
        CplexInstallationInfo? installation,
        IEnumerable<string> diagnostics)
    {
        Installation = installation;
        _diagnostics = diagnostics.ToArray();
    }

    /// <summary>Gets the selected compatible installation.</summary>
    public CplexInstallationInfo? Installation { get; }

    /// <summary>Gets discovery diagnostics.</summary>
    public IReadOnlyList<string> Diagnostics => _diagnostics;

    /// <summary>Gets whether a compatible installation was found.</summary>
    public bool IsFound => Installation is not null;
}
