using ULSAlgorithms.Optimization;

namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Combines solver-selection provenance, cutting-plane traceability and root
/// convergence statistics.
/// </summary>
public sealed class CuttingPlaneExecutionReport
{
    /// <summary>
    /// Initializes a report without convergence statistics.
    /// Kept for source compatibility with v0.20.0 callers.
    /// </summary>
    public CuttingPlaneExecutionReport(
        SolverExecutionInfo solver,
        CutGenerationReport cuts)
        : this(
            solver,
            cuts,
            convergence: null)
    {
    }

    /// <summary>Initializes a complete solver-backed cutting-plane report.</summary>
    public CuttingPlaneExecutionReport(
        SolverExecutionInfo solver,
        CutGenerationReport cuts,
        CuttingPlaneConvergenceReport? convergence)
    {
        Solver =
            solver ??
            throw new ArgumentNullException(
                nameof(solver));

        Cuts =
            cuts ??
            throw new ArgumentNullException(
                nameof(cuts));

        Convergence =
            convergence;
    }

    /// <summary>Gets the exact solver/adapter selected for the solve.</summary>
    public SolverExecutionInfo Solver { get; }

    /// <summary>Gets every generated and added/rejected cut.</summary>
    public CutGenerationReport Cuts { get; }

    /// <summary>Gets root LP convergence statistics when available.</summary>
    public CuttingPlaneConvergenceReport? Convergence { get; }
}
