using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using Xunit;

namespace ULSAlgorithms.Tests.Heuristics;

public sealed class McLarenOrderMomentSolverTests
{
    [Fact]
    public void OrderMomentTarget_UsesEoqDerivedTimeBetweenOrders()
    {
        var problem =
            new UlsProblem(
                [10.0, 10.0, 10.0, 10.0],
                [20.0, 20.0, 20.0, 20.0],
                [0.0, 0.0, 0.0, 0.0],
                [1.0, 1.0, 1.0, 0.0]);

        double target =
            McLarenOrderMomentSolver
                .GetOrderMomentTarget(problem);

        Assert.Equal(
            10.0,
            target,
            precision: 10);
    }

    [Fact]
    public void OrderMoment_ClosesLotAfterTargetAndMarginalTest()
    {
        var problem =
            new UlsProblem(
                [10.0, 10.0, 10.0, 10.0],
                [20.0, 20.0, 20.0, 20.0],
                [0.0, 0.0, 0.0, 0.0],
                [1.0, 1.0, 1.0, 0.0]);

        var result =
            new McLarenOrderMomentSolver()
                .Solve(
                    problem,
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            [20.0, 0.0, 20.0, 0.0],
            result.Solution!.ProductionQuantities.ToArray());
    }
}
