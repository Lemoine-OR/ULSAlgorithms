namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Solver-independent definition of one ULS (l,S) inequality.
/// </summary>
/// <remarks>
/// Period indices are zero-based, consistent with the rest of the ULSAlgorithms
/// public API.
/// </remarks>
public sealed class LsCutDefinition
{
    private readonly int[] _s;
    private readonly CutCoefficient[] _coefficients;

    /// <summary>Initializes an (l,S) cut definition.</summary>
    public LsCutDefinition(
        int l,
        IEnumerable<int> s,
        IEnumerable<CutCoefficient> coefficients,
        LinearConstraintSense sense,
        double rightHandSide)
    {
        if (l < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(l));
        }

        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(coefficients);

        _s = s.ToArray();

        if (_s.Any(period => period < 0 || period > l))
        {
            throw new ArgumentOutOfRangeException(
                nameof(s),
                "Every period in S must be between 0 and l.");
        }

        if (_s.Distinct().Count() != _s.Length)
        {
            throw new ArgumentException(
                "The set S cannot contain duplicate periods.",
                nameof(s));
        }

        Array.Sort(_s);

        _coefficients = coefficients
            .Where(static coefficient => coefficient.Coefficient != 0.0)
            .ToArray();

        if (_coefficients.Length == 0)
        {
            throw new ArgumentException(
                "A cut must contain at least one nonzero coefficient.",
                nameof(coefficients));
        }

        if (!double.IsFinite(rightHandSide))
        {
            throw new ArgumentOutOfRangeException(
                nameof(rightHandSide),
                "The right-hand side must be finite.");
        }

        L = l;
        Sense = sense;
        RightHandSide = rightHandSide;
    }

    /// <summary>Gets l in the (l,S) notation.</summary>
    public int L { get; }

    /// <summary>Gets a sorted defensive snapshot of S.</summary>
    public IReadOnlyList<int> S => _s;

    /// <summary>Gets nonzero coefficients in solver-independent form.</summary>
    public IReadOnlyList<CutCoefficient> Coefficients => _coefficients;

    /// <summary>Gets the inequality sense.</summary>
    public LinearConstraintSense Sense { get; }

    /// <summary>Gets the right-hand side.</summary>
    public double RightHandSide { get; }

    /// <summary>
    /// Returns a deterministic human-readable representation of the generated
    /// linear inequality.
    /// </summary>
    public override string ToString()
    {
        string leftHandSide = string.Join(
            " + ",
            _coefficients.Select(
                static term =>
                    $"{term.Coefficient.ToString(System.Globalization.CultureInfo.InvariantCulture)}*" +
                    term.VariableName));

        string relation = Sense switch
        {
            LinearConstraintSense.LessOrEqual => "<=",
            LinearConstraintSense.Equal => "=",
            LinearConstraintSense.GreaterOrEqual => ">=",
            _ => throw new InvalidOperationException(
                $"Unsupported constraint sense '{Sense}'.")
        };

        return
            $"{leftHandSide} {relation} " +
            RightHandSide.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
    }

}
