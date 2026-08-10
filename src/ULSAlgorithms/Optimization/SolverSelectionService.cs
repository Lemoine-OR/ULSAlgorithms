namespace ULSAlgorithms.Optimization;

/// <summary>
/// Selects a usable optimization solver by capability, machine availability,
/// and deterministic priority.
/// </summary>
public sealed class SolverSelectionService
{
    /// <summary>
    /// Selects an optimization-solver adapter.
    /// </summary>
    public async ValueTask<SolverSelectionResult> SelectAsync(
        SolverKind requestedSolver,
        SolverAdapterRegistry registry,
        SolverSelectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (requestedSolver == SolverKind.Unknown)
        {
            return new SolverSelectionResult(
                requestedSolver,
                null,
                null,
                ["The requested solver cannot be Unknown."]);
        }

        options ??= new SolverSelectionOptions();
        options.EnsureValid();

        var diagnostics = new List<string>();
        IReadOnlyList<SolverKind> order =
            BuildSolverOrder(requestedSolver, options);

        foreach (SolverKind solverKind in order)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<IOptimizationSolverAdapter> adapters =
                registry.FindBySolverKind(solverKind);

            if (adapters.Count == 0)
            {
                diagnostics.Add(
                    $"No registered adapter targets solver '{solverKind}'.");

                if (requestedSolver != SolverKind.Automatic &&
                    options.RequireExactSolverKind)
                {
                    break;
                }

                continue;
            }

            foreach (IOptimizationSolverAdapter adapter in adapters)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!SupportsRequiredCapabilities(
                        adapter,
                        options.RequiredCapabilities))
                {
                    diagnostics.Add(
                        $"Adapter '{adapter.AdapterName}' does not support " +
                        "all required capabilities.");
                    continue;
                }

                SolverAvailabilityInfo availability =
                    await adapter.CheckAvailabilityAsync(cancellationToken);

                if (availability.SolverKind != adapter.SolverKind)
                {
                    diagnostics.Add(
                        $"Adapter '{adapter.AdapterName}' returned availability " +
                        $"for '{availability.SolverKind}' instead of " +
                        $"'{adapter.SolverKind}'.");
                    continue;
                }

                if (!IsAvailabilityAccepted(availability, options))
                {
                    diagnostics.Add(
                        $"Adapter '{adapter.AdapterName}' is not usable. " +
                        $"Availability: {availability.Status}.");

                    foreach (string diagnostic in availability.Diagnostics)
                    {
                        diagnostics.Add(
                            $"{adapter.AdapterName}: {diagnostic}");
                    }

                    continue;
                }

                diagnostics.Add(
                    $"Adapter '{adapter.AdapterName}' was selected for " +
                    $"'{solverKind}'.");

                return new SolverSelectionResult(
                    requestedSolver,
                    adapter,
                    availability,
                    diagnostics);
            }

            if (requestedSolver != SolverKind.Automatic &&
                options.RequireExactSolverKind)
            {
                break;
            }
        }

        diagnostics.Add("No suitable optimization solver could be selected.");

        return new SolverSelectionResult(
            requestedSolver,
            null,
            null,
            diagnostics);
    }

    private static IReadOnlyList<SolverKind> BuildSolverOrder(
        SolverKind requestedSolver,
        SolverSelectionOptions options)
    {
        if (requestedSolver == SolverKind.Automatic)
        {
            return options.SolverPriority.ToArray();
        }

        if (options.RequireExactSolverKind)
        {
            return [requestedSolver];
        }

        var order = new List<SolverKind> { requestedSolver };

        foreach (SolverKind solverKind in options.SolverPriority)
        {
            if (!order.Contains(solverKind))
            {
                order.Add(solverKind);
            }
        }

        return order;
    }

    private static bool SupportsRequiredCapabilities(
        IOptimizationSolverAdapter adapter,
        IEnumerable<SolverCapability> requiredCapabilities)
    {
        foreach (SolverCapability capability in requiredCapabilities)
        {
            if (!adapter.SupportsCapability(capability))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsAvailabilityAccepted(
        SolverAvailabilityInfo availability,
        SolverSelectionOptions options)
    {
        if (availability.Status == SolverAvailabilityStatus.Available)
        {
            return true;
        }

        return options.AllowLimitedAvailability &&
               availability.Status ==
                   SolverAvailabilityStatus.AvailableWithLimitations;
    }
}
