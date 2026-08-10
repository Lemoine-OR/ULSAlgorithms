using ULSAlgorithms.CuttingPlanes;
using Xunit;

namespace ULSAlgorithms.Tests.CuttingPlanes;

public sealed class CuttingPlaneConvergenceReportTests
{
    [Fact]
    public void Report_ComputesRootGapClosedFraction()
    {
        var report =
            new CuttingPlaneConvergenceReport(
                [
                    Create(
                        0,
                        lpObjective: 80.0,
                        cumulativeCuts: 0),
                    Create(
                        1,
                        lpObjective: 95.0,
                        cumulativeCuts: 4)
                ],
                finalMipObjective: 100.0);

        Assert.Equal(
            15.0,
            report.RootBoundImprovement);

        Assert.Equal(
            0.75,
            report.RootGapClosedFraction);

        Assert.Equal(
            4,
            report.Iterations[^1].CumulativeCutsAdded);
    }

    private static CuttingPlaneIterationStatistics Create(
        int iteration,
        double lpObjective,
        int cumulativeCuts)
    {
        return new CuttingPlaneIterationStatistics(
            iteration,
            lpObjective,
            TimeSpan.FromMilliseconds(1),
            TimeSpan.FromMilliseconds(2),
            generatedCandidates: 10,
            eligibleCandidates: 5,
            selectedCuts: 4,
            cutsAdded: 4,
            cumulativeCutsAdded: cumulativeCuts,
            maximumViolation: 2.0,
            meanPositiveViolation: 1.0,
            maximumEfficacy: 0.5);
    }
}
