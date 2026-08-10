using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// High-level solver-independent execution service for portable linear models.
/// </summary>
/// <remarks>
/// Automatic selection follows the repository-wide priority:
/// CPLEX, Gurobi, Xpress, then COIN-OR CBC.
/// </remarks>
public sealed class LinearModelSolver
{
    private readonly SolverAdapterRegistry _adapterRegistry;
    private readonly LinearModelExecutorRegistry _executorRegistry;
    private readonly SolverSelectionService _selectionService;

    /// <summary>
    /// Initializes the service with the built-in discovery and execution
    /// backends.
    /// </summary>
    public LinearModelSolver()
        : this(
            DefaultSolverAdapterRegistry.Create(),
            DefaultLinearModelExecutorRegistry.Create(),
            new SolverSelectionService())
    {
    }

    /// <summary>
    /// Initializes the service with explicit registries. This constructor is
    /// useful for deterministic tests and custom integrations.
    /// </summary>
    public LinearModelSolver(
        SolverAdapterRegistry adapterRegistry,
        LinearModelExecutorRegistry executorRegistry,
        SolverSelectionService selectionService)
    {
        _adapterRegistry =
            adapterRegistry ??
            throw new ArgumentNullException(nameof(adapterRegistry));

        _executorRegistry =
            executorRegistry ??
            throw new ArgumentNullException(nameof(executorRegistry));

        _selectionService =
            selectionService ??
            throw new ArgumentNullException(nameof(selectionService));
    }

    /// <summary>
    /// Selects a usable solver and executes the portable model.
    /// </summary>
    public async ValueTask<LinearModelSolveResult> SolveAsync(
        LinearModel model,
        LinearModelSolveOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        options ??=
            new LinearModelSolveOptions();

        options.EnsureValid();
        cancellationToken.ThrowIfCancellationRequested();

        var selectionOptions =
            new SolverSelectionOptions
            {
                RequireExactSolverKind =
                    options.Solver != SolverKind.Automatic &&
                    !options.AllowFallbackWhenExplicit
            };

        selectionOptions.RequiredCapabilities.Add(
            model.IsMixedInteger
                ? SolverCapability.MixedIntegerLinearProgramming
                : SolverCapability.LinearProgramming);

        SolverSelectionResult selection =
            await _selectionService.SelectAsync(
                options.Solver,
                _adapterRegistry,
                selectionOptions,
                cancellationToken);

        if (!selection.IsSelected)
        {
            return new LinearModelSolveResult(
                model.Name,
                LinearModelSolveStatus.SolverUnavailable,
                solver: null,
                variableValues: null,
                validation: null,
                solveDuration: TimeSpan.Zero,
                nativeStatus: string.Empty,
                diagnostics: selection.Diagnostics);
        }

        ILinearModelSolverExecutor executor =
            _executorRegistry.GetRequired(
                selection.SelectedSolver);

        return await executor.SolveAsync(
            model,
            selection,
            options,
            cancellationToken);
    }
}
