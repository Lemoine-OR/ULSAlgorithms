namespace ULSAlgorithms.Optimization;

/// <summary>
/// Stores optimization-solver adapters and provides deterministic lookup by
/// adapter identifier and solver kind.
/// </summary>
public sealed class SolverAdapterRegistry
{
    private readonly Dictionary<string, IOptimizationSolverAdapter> _byId =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<SolverKind, List<IOptimizationSolverAdapter>> _byKind =
        new();

    /// <summary>Gets all currently registered adapters.</summary>
    public IReadOnlyCollection<IOptimizationSolverAdapter> Adapters =>
        _byId.Values.ToArray();

    /// <summary>Gets the number of registered adapters.</summary>
    public int Count => _byId.Count;

    /// <summary>Registers an adapter.</summary>
    public void Register(IOptimizationSolverAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(adapter);

        if (string.IsNullOrWhiteSpace(adapter.AdapterId))
        {
            throw new InvalidOperationException(
                "A solver adapter identifier is required.");
        }

        if (adapter.SolverKind is SolverKind.Unknown or SolverKind.Automatic)
        {
            throw new InvalidOperationException(
                "A solver adapter must target a concrete solver.");
        }

        string adapterId = adapter.AdapterId.Trim();

        if (_byId.ContainsKey(adapterId))
        {
            throw new InvalidOperationException(
                $"An adapter with identifier '{adapterId}' is already registered.");
        }

        _byId.Add(adapterId, adapter);

        if (!_byKind.TryGetValue(
                adapter.SolverKind,
                out List<IOptimizationSolverAdapter>? adapters))
        {
            adapters = [];
            _byKind.Add(adapter.SolverKind, adapters);
        }

        adapters.Add(adapter);
    }

    /// <summary>Gets registered adapters targeting one solver kind.</summary>
    public IReadOnlyList<IOptimizationSolverAdapter> FindBySolverKind(
        SolverKind solverKind)
    {
        return _byKind.TryGetValue(
            solverKind,
            out List<IOptimizationSolverAdapter>? adapters)
            ? adapters.ToArray()
            : [];
    }

    /// <summary>Removes all adapters.</summary>
    public void Clear()
    {
        _byId.Clear();
        _byKind.Clear();
    }
}
