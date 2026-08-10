using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using Xunit;

namespace ULSAlgorithms.Tests.Heuristics;

public sealed class ChiuModifiedLeastUnitCostSolverTests
{
    [Fact]
    public void ModifiedLuc_MergesCostBeneficialFinalLot()
    {
        var problem =
            CreateFivePeriodExample();

        var luc =
            new LeastUnitCostSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        var modified =
            new ChiuModifiedLeastUnitCostSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            [50.0, 0.0, 65.0, 0.0, 15.0],
            luc.Solution!.ProductionQuantities.ToArray());

        Assert.Equal(
            [50.0, 0.0, 80.0, 0.0, 0.0],
            modified.Solution!.ProductionQuantities.ToArray());

        Assert.True(
            modified.ObjectiveValue <
            luc.ObjectiveValue);
    }

    [Fact]
    public void ModifiedLuc_DoesNotMergeWhenExtraHoldingExceedsSetupSaving()
    {
        var problem =
            new UlsProblem(
                [10.0, 10.0, 1.0],
                [5.0, 5.0, 5.0],
                [0.0, 0.0, 0.0],
                [10.0, 10.0, 0.0]);

        var modified =
            new ChiuModifiedLeastUnitCostSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            [10.0, 10.0, 1.0],
            modified.Solution!.ProductionQuantities.ToArray());
    }

    private static UlsProblem CreateFivePeriodExample()
    {
        return new UlsProblem(
            [20.0, 30.0, 25.0, 40.0, 15.0],
            [200.0, 200.0, 200.0, 200.0, 200.0],
            [0.0, 0.0, 0.0, 0.0, 0.0],
            [4.0, 4.0, 4.0, 4.0, 0.0]);
    }
}
