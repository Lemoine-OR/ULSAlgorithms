namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// Stores concrete portable-model execution backends by solver kind.
/// </summary>
public sealed class LinearModelExecutorRegistry
{
    private readonly Dictionary<SolverKind, ILinearModelSolverExecutor>
        _executors = [];

    /// <summary>Gets the number of registered execution backends.</summary>
    public int Count => _executors.Count;

    /// <summary>Registers one concrete execution backend.</summary>
    public void Register(
        ILinearModelSolverExecutor executor)
    {
        ArgumentNullException.ThrowIfNull(executor);

        if (executor.SolverKind is
            SolverKind.Unknown or
            SolverKind.Automatic)
        {
            throw new InvalidOperationException(
                "An executor must target one concrete solver.");
        }

        if (!_executors.TryAdd(
                executor.SolverKind,
                executor))
        {
            throw new InvalidOperationException(
                $"An executor for '{executor.SolverKind}' is already registered.");
        }
    }

    /// <summary>Gets one registered execution backend.</summary>
    public ILinearModelSolverExecutor GetRequired(
        SolverKind solverKind)
    {
        return _executors.TryGetValue(
            solverKind,
            out ILinearModelSolverExecutor? executor)
            ? executor
            : throw new InvalidOperationException(
                $"No linear-model executor is registered for '{solverKind}'.");
    }
}
