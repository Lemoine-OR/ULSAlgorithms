using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using Xunit;

namespace ULSAlgorithms.Tests.Heuristics;

public sealed class HoChangSolisImprovedNetLeastPeriodCostSolverTests
{
    [Fact]
    public void ImprovedTieBreak_StopsWhenBothNetAveragesEqualSetupCost()
    {
        var problem =
            new UlsProblem(
                [10.0, 10.0, 100.0],
                [10.0, 10.0, 10.0],
                [0.0, 0.0, 0.0],
                [1.0, 1.0, 0.0]);

        var ordinary =
            new HoChangSolisNetLeastPeriodCostSolver()
                .Solve(
                    problem,
                    TestContext.Current.CancellationToken);

        var improved =
            new HoChangSolisImprovedNetLeastPeriodCostSolver()
                .Solve(
                    problem,
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            [20.0, 0.0, 100.0],
            ordinary.Solution!.ProductionQuantities.ToArray());

        Assert.Equal(
            [10.0, 10.0, 100.0],
            improved.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void ImprovedNetLpc_UsesSameZeroDemandNetAverageRule()
    {
        var problem =
            new UlsProblem(
                [10.0, 0.0, 5.0],
                [12.0, 12.0, 12.0],
                [0.0, 0.0, 0.0],
                [1.0, 1.0, 0.0]);

        var result =
            new HoChangSolisImprovedNetLeastPeriodCostSolver()
                .Solve(
                    problem,
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            [15.0, 0.0, 0.0],
            result.Solution!.ProductionQuantities.ToArray());
    }
}
