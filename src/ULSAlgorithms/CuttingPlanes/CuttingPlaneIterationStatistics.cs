namespace ULSAlgorithms.CuttingPlanes;

/// <summary>
/// Numerical convergence statistics for one root cutting-plane iteration.
/// </summary>
public sealed class CuttingPlaneIterationStatistics
{
    /// <summary>Initializes iteration statistics.</summary>
    public CuttingPlaneIterationStatistics(
        int iteration,
        double lpObjective,
        TimeSpan lpSolveTime,
        TimeSpan separationTime,
        int generatedCandidates,
        int eligibleCandidates,
        int selectedCuts,
        int cutsAdded,
        int cumulativeCutsAdded,
        double maximumViolation,
        double meanPositiveViolation,
        double maximumEfficacy)
    {
        if (iteration < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(iteration));
        }

        if (!double.IsFinite(lpObjective))
        {
            throw new ArgumentOutOfRangeException(nameof(lpObjective));
        }

        if (lpSolveTime < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lpSolveTime));
        }

        if (separationTime < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(separationTime));
        }

        if (generatedCandidates < 0 ||
            eligibleCandidates < 0 ||
            selectedCuts < 0 ||
            cutsAdded < 0 ||
            cumulativeCutsAdded < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(generatedCandidates));
        }

        if (!double.IsFinite(maximumViolation) ||
            !double.IsFinite(meanPositiveViolation) ||
            !double.IsFinite(maximumEfficacy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumViolation));
        }

        Iteration = iteration;
        LpObjective = lpObjective;
        LpSolveTime = lpSolveTime;
        SeparationTime = separationTime;
        GeneratedCandidates = generatedCandidates;
        EligibleCandidates = eligibleCandidates;
        SelectedCuts = selectedCuts;
        CutsAdded = cutsAdded;
        CumulativeCutsAdded = cumulativeCutsAdded;
        MaximumViolation = maximumViolation;
        MeanPositiveViolation = meanPositiveViolation;
        MaximumEfficacy = maximumEfficacy;
    }

    public int Iteration { get; }
    public double LpObjective { get; }
    public TimeSpan LpSolveTime { get; }
    public TimeSpan SeparationTime { get; }
    public int GeneratedCandidates { get; }
    public int EligibleCandidates { get; }
    public int SelectedCuts { get; }
    public int CutsAdded { get; }
    public int CumulativeCutsAdded { get; }
    public double MaximumViolation { get; }
    public double MeanPositiveViolation { get; }
    public double MaximumEfficacy { get; }
}
