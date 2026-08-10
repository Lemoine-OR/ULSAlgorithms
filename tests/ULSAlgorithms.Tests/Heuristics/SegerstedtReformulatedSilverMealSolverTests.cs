using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using Xunit;

namespace ULSAlgorithms.Tests.Heuristics;

public sealed class SegerstedtReformulatedSilverMealSolverTests
{
    [Fact]
    public void ReformulatedSilverMeal_RemovesZeroDemandDistortion()
    {
        var problem =
            new UlsProblem(
                [10.0, 0.0, 5.0],
                [12.0, 12.0, 12.0],
                [0.0, 0.0, 0.0],
                [1.0, 1.0, 0.0]);

        var classical =
            new SilverMealSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        var reformulated =
            new SegerstedtReformulatedSilverMealSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            [10.0, 0.0, 5.0],
            classical.Solution!.ProductionQuantities.ToArray());

        Assert.Equal(
            [15.0, 0.0, 0.0],
            reformulated.Solution!.ProductionQuantities.ToArray());

        Assert.True(
            reformulated.ObjectiveValue <
            classical.ObjectiveValue);
    }

    [Fact]
    public void ReformulatedSilverMeal_EqualsClassicalWithoutZeroDemandInSimpleCase()
    {
        var problem =
            new UlsProblem(
                [20.0, 30.0, 25.0, 40.0, 15.0],
                [200.0, 200.0, 200.0, 200.0, 200.0],
                [0.0, 0.0, 0.0, 0.0, 0.0],
                [4.0, 4.0, 4.0, 4.0, 0.0]);

        var classical =
            new SilverMealSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        var reformulated =
            new SegerstedtReformulatedSilverMealSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            classical.Solution!.ProductionQuantities.ToArray(),
            reformulated.Solution!.ProductionQuantities.ToArray());
    }
}
