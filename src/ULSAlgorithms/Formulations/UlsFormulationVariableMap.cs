using System.Collections.ObjectModel;

namespace ULSAlgorithms.Formulations;

/// <summary>
/// Maps semantic ULS decisions to solver-independent variable identifiers.
/// </summary>
public sealed class UlsFormulationVariableMap
{
    private readonly IReadOnlyDictionary<int, int> _production;
    private readonly IReadOnlyDictionary<int, int> _setup;
    private readonly IReadOnlyDictionary<int, int> _inventory;
    private readonly IReadOnlyDictionary<(int First, int Second), int>
        _disaggregated;
    private readonly IReadOnlyDictionary<(int From, int To), int>
        _arcs;

    /// <summary>Initializes a formulation variable map.</summary>
    public UlsFormulationVariableMap(
        IDictionary<int, int>? production = null,
        IDictionary<int, int>? setup = null,
        IDictionary<int, int>? inventory = null,
        IDictionary<(int First, int Second), int>? disaggregated = null,
        IDictionary<(int From, int To), int>? arcs = null)
    {
        _production =
            new ReadOnlyDictionary<int, int>(
                production is null
                    ? new Dictionary<int, int>()
                    : new Dictionary<int, int>(production));

        _setup =
            new ReadOnlyDictionary<int, int>(
                setup is null
                    ? new Dictionary<int, int>()
                    : new Dictionary<int, int>(setup));

        _inventory =
            new ReadOnlyDictionary<int, int>(
                inventory is null
                    ? new Dictionary<int, int>()
                    : new Dictionary<int, int>(inventory));

        _disaggregated =
            new ReadOnlyDictionary<(int First, int Second), int>(
                disaggregated is null
                    ? new Dictionary<(int First, int Second), int>()
                    : new Dictionary<(int First, int Second), int>(
                        disaggregated));

        _arcs =
            new ReadOnlyDictionary<(int From, int To), int>(
                arcs is null
                    ? new Dictionary<(int From, int To), int>()
                    : new Dictionary<(int From, int To), int>(arcs));
    }

    /// <summary>Gets production-variable ids by period.</summary>
    public IReadOnlyDictionary<int, int> Production => _production;

    /// <summary>Gets setup-variable ids by period.</summary>
    public IReadOnlyDictionary<int, int> Setup => _setup;

    /// <summary>Gets inventory-variable ids by period.</summary>
    public IReadOnlyDictionary<int, int> Inventory => _inventory;

    /// <summary>
    /// Gets disaggregated-variable ids. For the facility-location formulation,
    /// the key is (production period, demand period).
    /// </summary>
    public IReadOnlyDictionary<(int First, int Second), int>
        Disaggregated => _disaggregated;

    /// <summary>
    /// Gets shortest-path arc-variable ids keyed by (from node, to node).
    /// </summary>
    public IReadOnlyDictionary<(int From, int To), int> Arcs => _arcs;
}
