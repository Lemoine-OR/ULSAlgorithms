using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Heuristics;

public sealed class PartPeriodSimplifiedSolverTests
{
    [Fact]
    public void Pps_StopsBeforeEppOvershootWhilePpbChoosesNearestSide()
    {
        var problem =
            new UlsProblem(
                [1.0, 6.0, 3.0],
                [10.0, 10.0, 10.0],
                [0.0, 0.0, 0.0],
                [1.0, 1.0, 1.0]);

        UlsSolveResult pps =
            new PartPeriodSimplifiedSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        UlsSolveResult ppb =
            new PartPeriodBalancingSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            [7.0, 0.0, 3.0],
            pps.Solution!.ProductionQuantities.ToArray());

        Assert.Equal(
            [10.0, 0.0, 0.0],
            ppb.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void Pps_ReturnsFeasibleHeuristicStatus()
    {
        var problem =
            new UlsProblem(
                [4.0, 5.0, 6.0],
                [20.0, 20.0, 20.0],
                [2.0, 2.0, 2.0],
                [1.0, 1.0, 0.0]);

        UlsSolveResult result =
            new PartPeriodSimplifiedSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            UlsSolveStatus.Feasible,
            result.Status);

        Assert.NotNull(result.Solution);
    }
}
