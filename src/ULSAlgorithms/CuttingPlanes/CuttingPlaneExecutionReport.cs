using ULSAlgorithms.Optimization;

namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Combines solver-selection provenance and cutting-plane traceability.
/// </summary>
public sealed class CuttingPlaneExecutionReport
{
    /// <summary>Initializes a solver-backed cutting-plane execution report.</summary>
    public CuttingPlaneExecutionReport(
        SolverExecutionInfo solver,
        CutGenerationReport cuts)
    {
        Solver = solver ?? throw new ArgumentNullException(nameof(solver));
        Cuts = cuts ?? throw new ArgumentNullException(nameof(cuts));
    }

    /// <summary>Gets the exact solver/adapter selected for the solve.</summary>
    public SolverExecutionInfo Solver { get; }

    /// <summary>Gets every generated and added/rejected cut.</summary>
    public CutGenerationReport Cuts { get; }
}
