namespace ULSAlgorithms.Optimization;

/// <summary>
/// Configures automatic or explicit optimization-solver selection.
/// </summary>
public sealed class SolverSelectionOptions
{
    private readonly List<SolverKind> _solverPriority =
    [
        SolverKind.Cplex,
        SolverKind.Gurobi,
        SolverKind.Xpress,
        SolverKind.CoinOrCbc
    ];

    private readonly List<SolverCapability> _requiredCapabilities = [];

    /// <summary>
    /// Gets the priority used for <see cref="SolverKind.Automatic"/>.
    /// Earlier entries have higher priority.
    /// </summary>
    public List<SolverKind> SolverPriority => _solverPriority;

    /// <summary>Gets capabilities that the selected adapter must support.</summary>
    public List<SolverCapability> RequiredCapabilities => _requiredCapabilities;

    /// <summary>
    /// Gets or sets whether an adapter reported as available with limitations
    /// can still be selected.
    /// </summary>
    public bool AllowLimitedAvailability { get; set; } = true;

    /// <summary>
    /// Gets or sets whether an explicitly requested solver forbids fallback.
    /// </summary>
    public bool RequireExactSolverKind { get; set; }

    /// <summary>Validates this option set.</summary>
    public void EnsureValid()
    {
        if (_solverPriority.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one concrete solver must be present in the priority.");
        }

        var seenKinds = new HashSet<SolverKind>();

        foreach (SolverKind solverKind in _solverPriority)
        {
            if (solverKind is SolverKind.Unknown or SolverKind.Automatic)
            {
                throw new InvalidOperationException(
                    "The priority must contain only concrete solver kinds.");
            }

            if (!seenKinds.Add(solverKind))
            {
                throw new InvalidOperationException(
                    $"Solver '{solverKind}' appears more than once in the priority.");
            }
        }

        var seenCapabilities = new HashSet<SolverCapability>();

        foreach (SolverCapability capability in _requiredCapabilities)
        {
            if (capability == SolverCapability.Unknown)
            {
                throw new InvalidOperationException(
                    "Required capabilities cannot contain Unknown.");
            }

            if (!seenCapabilities.Add(capability))
            {
                throw new InvalidOperationException(
                    $"Capability '{capability}' appears more than once.");
            }
        }
    }
}
