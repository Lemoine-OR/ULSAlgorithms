namespace ULSAlgorithms.Optimization.Adapters.Cplex;

/// <summary>
/// Describes one compatible IBM ILOG CPLEX installation.
/// </summary>
public sealed record CplexInstallationInfo(
    string VersionFamily,
    string RootDirectory,
    string RuntimeDirectory,
    string ConcertAssemblyPath,
    string CplexAssemblyPath,
    string DiscoverySource);
