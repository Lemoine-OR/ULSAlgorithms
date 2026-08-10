namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Complete cutting-plane traceability report for one solver-backed solve.
/// </summary>
public sealed class CutGenerationReport
{
    private readonly CutIterationReport[] _iterations;
    private readonly CutRecord[] _cuts;

    /// <summary>Initializes a complete cut-generation report.</summary>
    public CutGenerationReport(
        IEnumerable<CutIterationReport> iterations)
    {
        ArgumentNullException.ThrowIfNull(iterations);

        _iterations = iterations
            .OrderBy(
                static iteration =>
                    iteration.Iteration)
            .ToArray();

        if (_iterations
                .Select(
                    static item =>
                        item.Iteration)
                .Distinct()
                .Count() !=
            _iterations.Length)
        {
            throw new ArgumentException(
                "Iteration identifiers must be unique.",
                nameof(iterations));
        }

        _cuts = _iterations
            .SelectMany(
                static iteration =>
                    iteration.Cuts)
            .OrderBy(
                static cut =>
                    cut.SequenceNumber)
            .ToArray();

        if (_cuts
                .Select(
                    static cut =>
                        cut.SequenceNumber)
                .Distinct()
                .Count() !=
            _cuts.Length)
        {
            throw new ArgumentException(
                "Cut sequence numbers must be unique within a solve.",
                nameof(iterations));
        }
    }

    public IReadOnlyList<CutIterationReport> Iterations => _iterations;
    public IReadOnlyList<CutRecord> Cuts => _cuts;
    public int IterationCount => _iterations.Length;
    public int CutsGenerated => _cuts.Length;

    public IReadOnlyList<CutRecord> AddedCuts =>
        _cuts.Where(
            static cut =>
                cut.WasAdded)
        .ToArray();

    public IReadOnlyList<CutRecord> GeneratedButNotAddedCuts =>
        _cuts.Where(
            static cut =>
                !cut.WasAdded)
        .ToArray();

    public int CutsAdded =>
        _cuts.Count(
            static cut =>
                cut.WasAdded);

    public int Duplicates =>
        _cuts.Count(
            static cut =>
                cut.Disposition ==
                CutDisposition.Duplicate);

    public int BelowTolerance =>
        _cuts.Count(
            static cut =>
                cut.Disposition ==
                CutDisposition.BelowTolerance);

    public int NotSelected =>
        _cuts.Count(
            static cut =>
                cut.Disposition ==
                CutDisposition.NotSelected);

    public int SolverRejected =>
        _cuts.Count(
            static cut =>
                cut.Disposition ==
                CutDisposition.SolverRejected);

    public double MaximumViolation =>
        _cuts.Length == 0
            ? 0.0
            : _cuts.Max(
                static cut =>
                    cut.Violation);

    public TimeSpan TotalSeparationTime =>
        TimeSpan.FromTicks(
            _iterations.Sum(
                static iteration =>
                    iteration.SeparationTime.Ticks));
}
