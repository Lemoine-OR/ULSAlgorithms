using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using Xunit;

namespace ULSAlgorithms.Tests.Heuristics;

public sealed class HoChangSolisNetLeastPeriodCostSolverTests
{
    [Fact]
    public void NetLpc_IgnoresZeroDemandPeriodsInAverageDenominator()
    {
        var problem =
            new UlsProblem(
                [10.0, 0.0, 5.0],
                [12.0, 12.0, 12.0],
                [0.0, 0.0, 0.0],
                [1.0, 1.0, 0.0]);

        var result =
            new HoChangSolisNetLeastPeriodCostSolver()
                .Solve(
                    problem,
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            [15.0, 0.0, 0.0],
            result.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void NetLpc_ReturnsFeasibleStatus()
    {
        var problem =
            new UlsProblem(
                [8.0, 0.0, 7.0, 6.0],
                [20.0, 20.0, 20.0, 20.0],
                [2.0, 2.0, 2.0, 2.0],
                [1.0, 1.0, 1.0, 0.0]);

        var result =
            new HoChangSolisNetLeastPeriodCostSolver()
                .Solve(
                    problem,
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            ULSAlgorithms.Results.UlsSolveStatus.Feasible,
            result.Status);
    }
}
