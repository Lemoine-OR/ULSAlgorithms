namespace ULSAlgorithms.Optimization.Modeling;

/// <summary>
/// Immutable solver-independent linear or mixed-integer linear model.
/// </summary>
public sealed class LinearModel
{
    private readonly LinearVariable[] _variables;
    private readonly LinearConstraint[] _constraints;

    /// <summary>Initializes a portable model.</summary>
    public LinearModel(
        string name,
        IEnumerable<LinearVariable> variables,
        IEnumerable<LinearConstraint> constraints,
        LinearObjective objective)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A model name is required.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(variables);
        ArgumentNullException.ThrowIfNull(constraints);
        ArgumentNullException.ThrowIfNull(objective);

        _variables = variables.ToArray();
        _constraints = constraints.ToArray();

        if (_variables
            .Select(
                static variable =>
                    variable.Id)
            .Distinct()
            .Count() != _variables.Length)
        {
            throw new ArgumentException(
                "Variable identifiers must be unique.",
                nameof(variables));
        }

        if (_variables
            .Select(
                static variable =>
                    variable.Name)
            .Distinct(
                StringComparer.Ordinal)
            .Count() != _variables.Length)
        {
            throw new ArgumentException(
                "Variable names must be unique.",
                nameof(variables));
        }

        var ids =
            _variables
                .Select(
                    static variable =>
                        variable.Id)
                .ToHashSet();

        foreach (LinearConstraint constraint in _constraints)
        {
            foreach (LinearTerm term in constraint.Terms)
            {
                if (!ids.Contains(term.VariableId))
                {
                    throw new ArgumentException(
                        $"Constraint '{constraint.Name}' references unknown " +
                        $"variable id {term.VariableId}.",
                        nameof(constraints));
                }
            }
        }

        foreach (LinearTerm term in objective.Terms)
        {
            if (!ids.Contains(term.VariableId))
            {
                throw new ArgumentException(
                    $"The objective references unknown variable id " +
                    $"{term.VariableId}.",
                    nameof(objective));
            }
        }

        Name = name.Trim();
        Objective = objective;
    }

    /// <summary>Gets the model name.</summary>
    public string Name { get; }

    /// <summary>Gets all variables.</summary>
    public IReadOnlyList<LinearVariable> Variables => _variables;

    /// <summary>Gets all constraints.</summary>
    public IReadOnlyList<LinearConstraint> Constraints => _constraints;

    /// <summary>Gets the minimization objective.</summary>
    public LinearObjective Objective { get; }

    /// <summary>Gets the number of variables.</summary>
    public int VariableCount => _variables.Length;

    /// <summary>Gets the number of constraints.</summary>
    public int ConstraintCount => _constraints.Length;

    /// <summary>Gets whether the model contains integer or binary variables.</summary>
    public bool IsMixedInteger =>
        _variables.Any(
            static variable =>
                variable.Type != LinearVariableType.Continuous);

    /// <summary>Finds a variable by id.</summary>
    public LinearVariable GetVariable(
        int id)
    {
        return _variables.First(
            variable =>
                variable.Id == id);
    }
}
