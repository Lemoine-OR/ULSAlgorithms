namespace ULSAlgorithms.Optimization;

/// <summary>
/// Serializable-style immutable snapshot of the solver selected for one
/// solver-backed ULS execution.
/// </summary>
public sealed class SolverExecutionInfo
{
    private readonly string[] _selectionDiagnostics;

    /// <summary>Creates an execution snapshot from a successful selection.</summary>
    public SolverExecutionInfo(SolverSelectionResult selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        if (!selection.IsSelected ||
            selection.SelectedAdapter is null ||
            selection.Availability is null)
        {
            throw new ArgumentException(
                "A successful solver selection is required.",
                nameof(selection));
        }

        RequestedSolver = selection.RequestedSolver;
        SelectedSolver = selection.SelectedSolver;
        AdapterId = selection.SelectedAdapter.AdapterId;
        AdapterName = selection.SelectedAdapter.AdapterName;
        AdapterVersion = selection.SelectedAdapter.AdapterVersion;
        SolverName = selection.Availability.SolverName;
        SolverVersion = selection.Availability.SolverVersion;
        AvailabilityStatus = selection.Availability.Status;
        InstallationPath = selection.Availability.InstallationPath;
        LicenseInformation = selection.Availability.LicenseInformation;
        _selectionDiagnostics = selection.Diagnostics.ToArray();
    }

    /// <summary>Gets the requested solver.</summary>
    public SolverKind RequestedSolver { get; }

    /// <summary>Gets the selected concrete solver.</summary>
    public SolverKind SelectedSolver { get; }

    /// <summary>Gets the selected adapter identifier.</summary>
    public string AdapterId { get; }

    /// <summary>Gets the selected adapter name.</summary>
    public string AdapterName { get; }

    /// <summary>Gets the selected adapter version.</summary>
    public string AdapterVersion { get; }

    /// <summary>Gets the detected solver name.</summary>
    public string SolverName { get; }

    /// <summary>Gets the detected solver version.</summary>
    public string SolverVersion { get; }

    /// <summary>Gets the selected availability status.</summary>
    public SolverAvailabilityStatus AvailabilityStatus { get; }

    /// <summary>Gets the selected installation path.</summary>
    public string InstallationPath { get; }

    /// <summary>Gets detected license information.</summary>
    public string LicenseInformation { get; }

    /// <summary>Gets diagnostics explaining automatic or explicit selection.</summary>
    public IReadOnlyList<string> SelectionDiagnostics => _selectionDiagnostics;
}
