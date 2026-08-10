using ULSAlgorithms.CuttingPlanes;
using Xunit;

namespace ULSAlgorithms.Tests.CuttingPlanes;

public sealed class CutGenerationReportTests
{
    [Fact]
    public void LsDefinition_DefensivelyCopiesAndSortsS()
    {
        int[] source = [3, 1, 2];

        var definition = new LsCutDefinition(
            4,
            source,
            [new CutCoefficient("y[0]", 1.0)],
            LinearConstraintSense.GreaterOrEqual,
            1.0);

        source[0] = 0;

        Assert.Equal([1, 2, 3], definition.S);
    }

    [Fact]
    public void Report_TracksGeneratedAddedDuplicateAndRejectedCuts()
    {
        CutRecord added = CreateCut(
            sequence: 0,
            iteration: 0,
            CutDisposition.Added,
            2.5);

        CutRecord duplicate = CreateCut(
            sequence: 1,
            iteration: 0,
            CutDisposition.Duplicate,
            1.4);

        CutRecord small = CreateCut(
            sequence: 2,
            iteration: 1,
            CutDisposition.BelowTolerance,
            0.00001);

        CutRecord rejected = CreateCut(
            sequence: 3,
            iteration: 1,
            CutDisposition.SolverRejected,
            0.8);

        var report = new CutGenerationReport(
        [
            new CutIterationReport(
                0,
                [added, duplicate],
                TimeSpan.FromMilliseconds(3)),
            new CutIterationReport(
                1,
                [small, rejected],
                TimeSpan.FromMilliseconds(4))
        ]);

        Assert.Equal(2, report.IterationCount);
        Assert.Equal(4, report.CutsGenerated);
        Assert.Equal(1, report.CutsAdded);
        Assert.Single(report.AddedCuts);
        Assert.Equal(3, report.GeneratedButNotAddedCuts.Count);
        Assert.Equal(1, report.Duplicates);
        Assert.Equal(1, report.BelowTolerance);
        Assert.Equal(1, report.SolverRejected);
        Assert.Equal(2.5, report.MaximumViolation);
        Assert.Equal(TimeSpan.FromMilliseconds(7), report.TotalSeparationTime);
    }

    [Fact]
    public void CutRecord_ExposesGeneratedConstraintAndAdditionStatus()
    {
        CutRecord cut = CreateCut(
            sequence: 7,
            iteration: 3,
            CutDisposition.Added,
            1.75);

        Assert.Equal(CutFamily.Ls, cut.Family);
        Assert.Equal(CutSeparationMethod.WagnerWhitin, cut.SeparationMethod);
        Assert.True(cut.WasAdded);
        Assert.Equal("LS_003_0007", cut.SolverConstraintName);
        Assert.Equal(4, cut.Definition.L);
        Assert.Equal([1, 3], cut.Definition.S);
        Assert.NotEmpty(cut.Definition.Coefficients);
        Assert.Contains("y[0]", cut.Definition.ToString(), StringComparison.Ordinal);
        Assert.Contains(">=", cut.Definition.ToString(), StringComparison.Ordinal);
    }

    private static CutRecord CreateCut(
        int sequence,
        int iteration,
        CutDisposition disposition,
        double violation)
    {
        var definition = new LsCutDefinition(
            4,
            [1, 3],
            [
                new CutCoefficient("y[0]", 1.0),
                new CutCoefficient("x[1]", 0.25)
            ],
            LinearConstraintSense.GreaterOrEqual,
            1.0);

        return new CutRecord(
            sequence,
            iteration,
            CutSeparationMethod.WagnerWhitin,
            definition,
            violation,
            efficacy: 0.5,
            disposition,
            solverConstraintName: $"LS_{iteration:000}_{sequence:0000}",
            dispositionReason:
                disposition == CutDisposition.Added
                    ? "Violated cut accepted."
                    : disposition.ToString());
    }
}
