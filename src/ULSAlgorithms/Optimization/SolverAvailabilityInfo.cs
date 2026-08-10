namespace ULSAlgorithms.Optimization;

/// <summary>
/// Describes solver availability and the installation selected by an adapter.
/// </summary>
public sealed class SolverAvailabilityInfo
{
    private readonly string[] _diagnostics;
    private readonly string[] _limitations;

    /// <summary>
    /// Initializes an availability result.
    /// </summary>
    public SolverAvailabilityInfo(
        SolverKind solverKind,
        SolverAvailabilityStatus status,
        string solverName = "",
        string solverVersion = "",
        string installationPath = "",
        string managedAssemblyPath = "",
        string nativeLibraryPath = "",
        string licenseInformation = "",
        IEnumerable<string>? diagnostics = null,
        IEnumerable<string>? limitations = null)
    {
        if (solverKind is SolverKind.Unknown or SolverKind.Automatic)
        {
            throw new ArgumentOutOfRangeException(
                nameof(solverKind),
                "Availability information must target a concrete solver.");
        }

        SolverKind = solverKind;
        Status = status;
        SolverName = solverName ?? string.Empty;
        SolverVersion = solverVersion ?? string.Empty;
        InstallationPath = installationPath ?? string.Empty;
        ManagedAssemblyPath = managedAssemblyPath ?? string.Empty;
        NativeLibraryPath = nativeLibraryPath ?? string.Empty;
        LicenseInformation = licenseInformation ?? string.Empty;
        _diagnostics = CopyNonBlank(diagnostics);
        _limitations = CopyNonBlank(limitations);
    }

    /// <summary>Gets the concrete solver kind.</summary>
    public SolverKind SolverKind { get; }

    /// <summary>Gets the detected availability status.</summary>
    public SolverAvailabilityStatus Status { get; }

    /// <summary>Gets the solver display name.</summary>
    public string SolverName { get; }

    /// <summary>Gets the detected solver version.</summary>
    public string SolverVersion { get; }

    /// <summary>Gets the detected installation root.</summary>
    public string InstallationPath { get; }

    /// <summary>Gets the managed assembly path, when applicable.</summary>
    public string ManagedAssemblyPath { get; }

    /// <summary>Gets the native library or executable path, when applicable.</summary>
    public string NativeLibraryPath { get; }

    /// <summary>Gets detected licensing information.</summary>
    public string LicenseInformation { get; }

    /// <summary>Gets diagnostics produced during availability detection.</summary>
    public IReadOnlyList<string> Diagnostics => _diagnostics;

    /// <summary>Gets detected functional limitations.</summary>
    public IReadOnlyList<string> Limitations => _limitations;

    /// <summary>Gets whether the solver can be selected for use.</summary>
    public bool IsUsable =>
        Status is
            SolverAvailabilityStatus.Available or
            SolverAvailabilityStatus.AvailableWithLimitations;

    private static string[] CopyNonBlank(IEnumerable<string>? values)
    {
        if (values is null)
        {
            return [];
        }

        return values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .ToArray();
    }
}
