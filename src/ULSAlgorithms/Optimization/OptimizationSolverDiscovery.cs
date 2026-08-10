namespace ULSAlgorithms.Optimization;

/// <summary>
/// High-level entry point used by solver-backed ULS algorithms to discover and
/// select a mathematical optimizer automatically.
/// </summary>
public static class OptimizationSolverDiscovery
{
    private static readonly IReadOnlyList<SolverKind> StandardOrder =
        Array.AsReadOnly<SolverKind>(
        [
            SolverKind.Cplex,
            SolverKind.Gurobi,
            SolverKind.Xpress,
            SolverKind.CoinOrCbc
        ]);

    /// <summary>
    /// Gets the standard automatic solver priority.
    /// </summary>
    public static IReadOnlyList<SolverKind> DefaultPriority =>
        StandardOrder;

    /// <summary>
    /// Probes every built-in adapter in standard priority order.
    /// </summary>
    public static async ValueTask<SolverDiscoveryReport>
        DiscoverAllAsync(
            CancellationToken cancellationToken = default)
    {
        SolverAdapterRegistry registry =
            DefaultSolverAdapterRegistry.Create();

        var availability =
            new List<SolverAvailabilityInfo>(
                StandardOrder.Count);

        foreach (SolverKind solverKind in StandardOrder)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IOptimizationSolverAdapter adapter =
                registry.FindBySolverKind(solverKind)
                    .Single();

            availability.Add(
                await adapter.CheckAvailabilityAsync(
                    cancellationToken));
        }

        return new SolverDiscoveryReport(
            availability);
    }

    /// <summary>
    /// Selects the requested solver using all built-in adapters.
    /// </summary>
    /// <remarks>
    /// Solver-backed algorithms should normally call this method with
    /// <see cref="SolverKind.Automatic"/> once at the beginning of a solve and
    /// keep the resulting <see cref="SolverSelectionResult"/> for the complete
    /// execution.
    /// </remarks>
    public static ValueTask<SolverSelectionResult>
        SelectAsync(
            SolverKind requestedSolver = SolverKind.Automatic,
            SolverSelectionOptions? options = null,
            CancellationToken cancellationToken = default)
    {
        SolverAdapterRegistry registry =
            DefaultSolverAdapterRegistry.Create();

        return new SolverSelectionService()
            .SelectAsync(
                requestedSolver,
                registry,
                options,
                cancellationToken);
    }
}
