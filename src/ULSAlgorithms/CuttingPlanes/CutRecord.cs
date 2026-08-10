namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Trace record for one generated cutting-plane constraint.
/// </summary>
public sealed class CutRecord
{
    /// <summary>Initializes one cut trace record.</summary>
    public CutRecord(
        int sequenceNumber,
        int iteration,
        CutSeparationMethod separationMethod,
        LsCutDefinition definition,
        double violation,
        double efficacy,
        CutDisposition disposition,
        string solverConstraintName = "",
        string dispositionReason = "")
    {
        if (sequenceNumber < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequenceNumber));
        }

        if (iteration < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(iteration));
        }

        if (separationMethod == CutSeparationMethod.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(separationMethod));
        }

        ArgumentNullException.ThrowIfNull(definition);

        if (!double.IsFinite(violation))
        {
            throw new ArgumentOutOfRangeException(nameof(violation));
        }

        if (!double.IsFinite(efficacy))
        {
            throw new ArgumentOutOfRangeException(nameof(efficacy));
        }

        SequenceNumber = sequenceNumber;
        Iteration = iteration;
        Family = CutFamily.Ls;
        SeparationMethod = separationMethod;
        Definition = definition;
        Violation = violation;
        Efficacy = efficacy;
        Disposition = disposition;
        SolverConstraintName = solverConstraintName ?? string.Empty;
        DispositionReason = dispositionReason ?? string.Empty;
    }

    /// <summary>Gets the stable sequence number within the solve.</summary>
    public int SequenceNumber { get; }

    /// <summary>Gets the cutting-plane iteration that generated this cut.</summary>
    public int Iteration { get; }

    /// <summary>Gets the cut family.</summary>
    public CutFamily Family { get; }

    /// <summary>Gets the separation method.</summary>
    public CutSeparationMethod SeparationMethod { get; }

    /// <summary>Gets the complete solver-independent (l,S) definition.</summary>
    public LsCutDefinition Definition { get; }

    /// <summary>Gets the violation measured when the cut was generated.</summary>
    public double Violation { get; }

    /// <summary>Gets the cut efficacy metric used by the separator.</summary>
    public double Efficacy { get; }

    /// <summary>Gets the final disposition of this generated cut.</summary>
    public CutDisposition Disposition { get; }

    /// <summary>Gets whether the constraint was actually added to the solver model.</summary>
    public bool WasAdded => Disposition == CutDisposition.Added;

    /// <summary>Gets the row/constraint name used by the selected solver adapter.</summary>
    public string SolverConstraintName { get; }

    /// <summary>Gets a human-readable explanation of the disposition.</summary>
    public string DispositionReason { get; }
}
