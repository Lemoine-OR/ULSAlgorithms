namespace ULSAlgorithms.Optimization;

/// <summary>
/// Complete availability snapshot for all built-in solver adapters.
/// </summary>
public sealed class SolverDiscoveryReport
{
    private readonly SolverAvailabilityInfo[] _solvers;

    /// <summary>Initializes a discovery report.</summary>
    public SolverDiscoveryReport(
        IEnumerable<SolverAvailabilityInfo> solvers)
    {
        ArgumentNullException.ThrowIfNull(solvers);
        _solvers = solvers.ToArray();
    }

    /// <summary>
    /// Gets solver availability in the standard CPLEX → Gurobi → Xpress → CBC
    /// order.
    /// </summary>
    public IReadOnlyList<SolverAvailabilityInfo> Solvers =>
        _solvers;

    /// <summary>Gets only currently usable solvers.</summary>
    public IReadOnlyList<SolverAvailabilityInfo> UsableSolvers =>
        _solvers
            .Where(
                static solver =>
                    solver.IsUsable)
            .ToArray();

    /// <summary>Gets whether at least one solver is usable.</summary>
    public bool HasUsableSolver =>
        _solvers.Any(
            static solver =>
                solver.IsUsable);
}
