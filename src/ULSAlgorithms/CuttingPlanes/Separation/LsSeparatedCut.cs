using ULSAlgorithms.CuttingPlanes;

namespace ULSAlgorithms.CuttingPlanes.Separation;

/// <summary>
/// One candidate (l,S) inequality produced by a separation procedure.
/// </summary>
public sealed class LsSeparatedCut
{
    /// <summary>Initializes a separated cut.</summary>
    public LsSeparatedCut(
        LsCutDefinition definition,
        double violation,
        double efficacy)
    {
        Definition =
            definition ??
            throw new ArgumentNullException(
                nameof(definition));

        if (!double.IsFinite(violation))
        {
            throw new ArgumentOutOfRangeException(
                nameof(violation));
        }

        if (!double.IsFinite(efficacy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(efficacy));
        }

        Violation = violation;
        Efficacy = efficacy;
    }

    /// <summary>Gets the solver-independent inequality definition.</summary>
    public LsCutDefinition Definition { get; }

    /// <summary>
    /// Gets RHS - LHS for the canonical greater-than-or-equal form.
    /// Positive values are violated.
    /// </summary>
    public double Violation { get; }

    /// <summary>Gets violation divided by the Euclidean coefficient norm.</summary>
    public double Efficacy { get; }
}
