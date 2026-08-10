namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Summarizes root-bound evolution and separation effort for one exact
/// cut-and-solve execution.
/// </summary>
public sealed class CuttingPlaneConvergenceReport
{
    private readonly CuttingPlaneIterationStatistics[] _iterations;

    /// <summary>Initializes a convergence report.</summary>
    public CuttingPlaneConvergenceReport(
        IEnumerable<CuttingPlaneIterationStatistics> iterations,
        double? finalMipObjective)
    {
        ArgumentNullException.ThrowIfNull(iterations);

        _iterations =
            iterations
                .OrderBy(
                    static item =>
                        item.Iteration)
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

        if (finalMipObjective.HasValue &&
            !double.IsFinite(finalMipObjective.Value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(finalMipObjective));
        }

        FinalMipObjective = finalMipObjective;
    }

    /// <summary>Gets root iterations in ascending order.</summary>
    public IReadOnlyList<CuttingPlaneIterationStatistics> Iterations =>
        _iterations;

    /// <summary>Gets the objective of the first root LP.</summary>
    public double? InitialLpObjective =>
        _iterations.Length == 0
            ? null
            : _iterations[0].LpObjective;

    /// <summary>Gets the objective of the last solved strengthened root LP.</summary>
    public double? FinalRootLpObjective =>
        _iterations.Length == 0
            ? null
            : _iterations[^1].LpObjective;

    /// <summary>Gets the final exact MILP objective, when available.</summary>
    public double? FinalMipObjective { get; }

    /// <summary>Gets absolute improvement of the root lower bound.</summary>
    public double? RootBoundImprovement =>
        InitialLpObjective.HasValue &&
        FinalRootLpObjective.HasValue
            ? FinalRootLpObjective.Value -
              InitialLpObjective.Value
            : null;

    /// <summary>
    /// Gets the fraction of the initial LP-to-MIP gap closed by root cutting
    /// planes. Returns null when the denominator is numerically zero.
    /// </summary>
    public double? RootGapClosedFraction
    {
        get
        {
            if (!InitialLpObjective.HasValue ||
                !FinalRootLpObjective.HasValue ||
                !FinalMipObjective.HasValue)
            {
                return null;
            }

            double denominator =
                FinalMipObjective.Value -
                InitialLpObjective.Value;

            double scale =
                Math.Max(
                    1.0,
                    Math.Abs(
                        FinalMipObjective.Value));

            if (Math.Abs(denominator) <=
                1.0e-12 * scale)
            {
                return null;
            }

            return
                (FinalRootLpObjective.Value -
                 InitialLpObjective.Value) /
                denominator;
        }
    }

    /// <summary>Gets total root LP solver time.</summary>
    public TimeSpan TotalLpSolveTime =>
        TimeSpan.FromTicks(
            _iterations.Sum(
                static item =>
                    item.LpSolveTime.Ticks));

    /// <summary>Gets total separator time.</summary>
    public TimeSpan TotalSeparationTime =>
        TimeSpan.FromTicks(
            _iterations.Sum(
                static item =>
                    item.SeparationTime.Ticks));

    /// <summary>Gets total candidate count across root iterations.</summary>
    public int TotalGeneratedCandidates =>
        _iterations.Sum(
            static item =>
                item.GeneratedCandidates);

    /// <summary>Gets total selected cuts across root iterations.</summary>
    public int TotalSelectedCuts =>
        _iterations.Sum(
            static item =>
                item.SelectedCuts);
}
