namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Complete cutting-plane traceability report for one solver-backed solve.
/// </summary>
public sealed class CutGenerationReport
{
    private readonly CutIterationReport[] _iterations;
    private readonly CutRecord[] _cuts;

    /// <summary>Initializes a complete cut-generation report.</summary>
    public CutGenerationReport(IEnumerable<CutIterationReport> iterations)
    {
        ArgumentNullException.ThrowIfNull(iterations);

        _iterations = iterations
            .OrderBy(static iteration => iteration.Iteration)
            .ToArray();

        if (_iterations.Select(static item => item.Iteration).Distinct().Count() !=
            _iterations.Length)
        {
            throw new ArgumentException(
                "Iteration identifiers must be unique.",
                nameof(iterations));
        }

        _cuts = _iterations
            .SelectMany(static iteration => iteration.Cuts)
            .OrderBy(static cut => cut.SequenceNumber)
            .ToArray();

        if (_cuts.Select(static cut => cut.SequenceNumber).Distinct().Count() !=
            _cuts.Length)
        {
            throw new ArgumentException(
                "Cut sequence numbers must be unique within a solve.",
                nameof(iterations));
        }
    }

    /// <summary>Gets iteration reports in ascending iteration order.</summary>
    public IReadOnlyList<CutIterationReport> Iterations => _iterations;

    /// <summary>Gets all cuts in sequence-number order.</summary>
    public IReadOnlyList<CutRecord> Cuts => _cuts;

    /// <summary>Gets the number of cutting-plane iterations.</summary>
    public int IterationCount => _iterations.Length;

    /// <summary>Gets the number of generated cuts.</summary>
    public int CutsGenerated => _cuts.Length;

    /// <summary>Gets every generated cut that was actually added to the model.</summary>
    public IReadOnlyList<CutRecord> AddedCuts =>
        _cuts.Where(static cut => cut.WasAdded).ToArray();

    /// <summary>Gets every generated cut that was not added to the model.</summary>
    public IReadOnlyList<CutRecord> GeneratedButNotAddedCuts =>
        _cuts.Where(static cut => !cut.WasAdded).ToArray();

    /// <summary>Gets the number of cuts actually added to the solver model.</summary>
    public int CutsAdded => _cuts.Count(static cut => cut.WasAdded);

    /// <summary>Gets the number of duplicate cuts.</summary>
    public int Duplicates =>
        _cuts.Count(static cut => cut.Disposition == CutDisposition.Duplicate);

    /// <summary>Gets the number rejected because violation was below tolerance.</summary>
    public int BelowTolerance =>
        _cuts.Count(static cut => cut.Disposition == CutDisposition.BelowTolerance);

    /// <summary>Gets the number rejected by the solver adapter.</summary>
    public int SolverRejected =>
        _cuts.Count(static cut => cut.Disposition == CutDisposition.SolverRejected);

    /// <summary>Gets the maximum violation observed over the complete solve.</summary>
    public double MaximumViolation =>
        _cuts.Length == 0
            ? 0.0
            : _cuts.Max(static cut => cut.Violation);

    /// <summary>Gets total recorded separation time.</summary>
    public TimeSpan TotalSeparationTime =>
        TimeSpan.FromTicks(
            _iterations.Sum(static iteration => iteration.SeparationTime.Ticks));
}
