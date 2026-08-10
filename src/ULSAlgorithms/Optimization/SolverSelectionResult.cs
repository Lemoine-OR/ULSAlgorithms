namespace ULSAlgorithms.Optimization;

/// <summary>
/// Describes the outcome of optimization-solver selection.
/// </summary>
public sealed class SolverSelectionResult
{
    private readonly string[] _diagnostics;

    internal SolverSelectionResult(
        SolverKind requestedSolver,
        IOptimizationSolverAdapter? selectedAdapter,
        SolverAvailabilityInfo? availability,
        IEnumerable<string> diagnostics)
    {
        RequestedSolver = requestedSolver;
        SelectedAdapter = selectedAdapter;
        Availability = availability;
        _diagnostics = diagnostics.ToArray();
    }

    /// <summary>Gets the solver requested by the caller.</summary>
    public SolverKind RequestedSolver { get; }

    /// <summary>Gets the selected adapter, or null when selection failed.</summary>
    public IOptimizationSolverAdapter? SelectedAdapter { get; }

    /// <summary>Gets the selected solver kind, or Unknown when none was selected.</summary>
    public SolverKind SelectedSolver =>
        SelectedAdapter?.SolverKind ?? SolverKind.Unknown;

    /// <summary>Gets machine-availability information for the selected solver.</summary>
    public SolverAvailabilityInfo? Availability { get; }

    /// <summary>Gets deterministic selection diagnostics.</summary>
    public IReadOnlyList<string> Diagnostics => _diagnostics;

    /// <summary>Gets whether a usable adapter was selected.</summary>
    public bool IsSelected =>
        SelectedAdapter is not null &&
        Availability is not null &&
        Availability.IsUsable;
}
