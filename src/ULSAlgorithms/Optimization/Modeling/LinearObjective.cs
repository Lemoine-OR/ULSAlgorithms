namespace ULSAlgorithms.Optimization.Modeling;

/// <summary>
/// Describes the minimization objective of a portable linear model.
/// </summary>
public sealed class LinearObjective
{
    private readonly LinearTerm[] _terms;

    /// <summary>Initializes a minimization objective.</summary>
    public LinearObjective(
        IEnumerable<LinearTerm> terms,
        double constant = 0.0)
    {
        ArgumentNullException.ThrowIfNull(terms);

        if (!double.IsFinite(constant))
        {
            throw new ArgumentOutOfRangeException(
                nameof(constant));
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
                "The objective cannot contain duplicate variable identifiers.",
                nameof(terms));
        }

        Constant = constant;
    }

    /// <summary>Gets nonzero objective terms.</summary>
    public IReadOnlyList<LinearTerm> Terms => _terms;

    /// <summary>Gets the objective constant.</summary>
    public double Constant { get; }
}
