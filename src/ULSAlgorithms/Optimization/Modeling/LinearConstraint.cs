namespace ULSAlgorithms.Optimization.Modeling;

/// <summary>
/// Describes one portable linear constraint.
/// </summary>
public sealed class LinearConstraint
{
    private readonly LinearTerm[] _terms;

    /// <summary>Initializes a linear constraint.</summary>
    public LinearConstraint(
        string name,
        IEnumerable<LinearTerm> terms,
        LinearConstraintSense sense,
        double rightHandSide)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A constraint name is required.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(terms);

        if (!double.IsFinite(rightHandSide))
        {
            throw new ArgumentOutOfRangeException(
                nameof(rightHandSide),
                "The right-hand side must be finite.");
        }

        _terms = terms
            .Where(
                static term =>
                    term.Coefficient != 0.0)
            .ToArray();

        if (_terms
            .GroupBy(
                static term =>
                    term.VariableId)
            .Any(
                static group =>
                    group.Count() > 1))
        {
            throw new ArgumentException(
                "A constraint cannot contain duplicate variable identifiers.",
                nameof(terms));
        }

        Name = name.Trim();
        Sense = sense;
        RightHandSide = rightHandSide;
    }

    /// <summary>Gets the constraint name.</summary>
    public string Name { get; }

    /// <summary>Gets the nonzero terms.</summary>
    public IReadOnlyList<LinearTerm> Terms => _terms;

    /// <summary>Gets the constraint sense.</summary>
    public LinearConstraintSense Sense { get; }

    /// <summary>Gets the right-hand side.</summary>
    public double RightHandSide { get; }
}
