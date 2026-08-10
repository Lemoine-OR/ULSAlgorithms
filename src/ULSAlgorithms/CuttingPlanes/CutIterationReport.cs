namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Traceability report for one cutting-plane iteration.
/// </summary>
public sealed class CutIterationReport
{
    private readonly CutRecord[] _cuts;

    /// <summary>Initializes an iteration report.</summary>
    public CutIterationReport(
        int iteration,
        IEnumerable<CutRecord> cuts,
        TimeSpan separationTime)
    {
        if (iteration < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(iteration));
        }

        ArgumentNullException.ThrowIfNull(cuts);

        if (separationTime < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(separationTime));
        }

        _cuts = cuts.ToArray();

        if (_cuts.Any(cut => cut.Iteration != iteration))
        {
            throw new ArgumentException(
                "Every cut in an iteration report must have the same iteration.",
                nameof(cuts));
        }

        Iteration = iteration;
        SeparationTime = separationTime;
        MaximumViolation = _cuts.Length == 0
            ? 0.0
            : _cuts.Max(static cut => cut.Violation);
    }

    /// <summary>Gets the iteration index.</summary>
    public int Iteration { get; }

    /// <summary>Gets all generated cuts, including rejected and duplicate cuts.</summary>
    public IReadOnlyList<CutRecord> Cuts => _cuts;

    /// <summary>Gets the number of generated cuts.</summary>
    public int GeneratedCount => _cuts.Length;

    /// <summary>Gets the number of cuts actually added to the model.</summary>
    public int AddedCount => _cuts.Count(static cut => cut.WasAdded);

    /// <summary>Gets the number of generated cuts not added to the model.</summary>
    public int NotAddedCount => GeneratedCount - AddedCount;

    /// <summary>Gets the maximum measured violation in this iteration.</summary>
    public double MaximumViolation { get; }

    /// <summary>Gets time spent in separation for this iteration.</summary>
    public TimeSpan SeparationTime { get; }
}
