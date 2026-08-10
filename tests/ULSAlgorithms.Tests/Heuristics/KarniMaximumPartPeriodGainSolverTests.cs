using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using Xunit;

namespace ULSAlgorithms.Tests.Heuristics;

public sealed class KarniMaximumPartPeriodGainSolverTests
{
    [Fact]
    public void Mpg_ReproducesPublishedTwelvePeriodReconstructionExample()
    {
        var problem =
            new UlsProblem(
                [
                    10.0, 62.0, 12.0, 130.0,
                    154.0, 129.0, 88.0, 52.0,
                    124.0, 160.0, 238.0, 41.0
                ],
                [
                    54.0, 54.0, 54.0, 54.0,
                    54.0, 54.0, 54.0, 54.0,
                    54.0, 54.0, 54.0, 54.0
                ],
                [
                    0.0, 0.0, 0.0, 0.0,
                    0.0, 0.0, 0.0, 0.0,
                    0.0, 0.0, 0.0, 0.0
                ],
                [
                    0.4, 0.4, 0.4, 0.4,
                    0.4, 0.4, 0.4, 0.4,
                    0.4, 0.4, 0.4, 0.0
                ]);

        var result =
            new KarniMaximumPartPeriodGainSolver()
                .Solve(
                    problem,
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                84.0, 0.0, 0.0, 130.0,
                283.0, 0.0, 140.0, 0.0,
                124.0, 160.0, 279.0, 0.0
            ],
            result.Solution!.ProductionQuantities.ToArray());

        Assert.Equal(
            501.2,
            result.ObjectiveValue!.Value,
            precision: 10);
    }

    [Fact]
    public void Mpg_UsesGlobalMergeRatherThanForwardScan()
    {
        var problem =
            new UlsProblem(
                [10.0, 10.0, 10.0, 10.0],
                [20.0, 20.0, 20.0, 20.0],
                [0.0, 0.0, 0.0, 0.0],
                [1.0, 1.0, 1.0, 0.0]);

        var result =
            new KarniMaximumPartPeriodGainSolver()
                .Solve(
                    problem,
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            [20.0, 0.0, 20.0, 0.0],
            result.Solution!.ProductionQuantities.ToArray());
    }
}
