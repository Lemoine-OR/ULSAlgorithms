using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using Xunit;

namespace ULSAlgorithms.Tests.Heuristics;

public sealed class ChiuTingModifiedPartPeriodBalancingSolverTests
{
    [Fact]
    public void ModifiedPpb_MergesCostBeneficialFinalLot()
    {
        var problem =
            new UlsProblem(
                [20.0, 30.0, 25.0, 40.0, 15.0],
                [200.0, 200.0, 200.0, 200.0, 200.0],
                [0.0, 0.0, 0.0, 0.0, 0.0],
                [4.0, 4.0, 4.0, 4.0, 0.0]);

        var ppb =
            new PartPeriodBalancingSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        var modified =
            new ChiuTingModifiedPartPeriodBalancingSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            [50.0, 0.0, 65.0, 0.0, 15.0],
            ppb.Solution!.ProductionQuantities.ToArray());

        Assert.Equal(
            [50.0, 0.0, 80.0, 0.0, 0.0],
            modified.Solution!.ProductionQuantities.ToArray());

        Assert.True(
            modified.ObjectiveValue <
            ppb.ObjectiveValue);
    }

    [Fact]
    public void ModifiedPpb_UsesCommonHeuristicContract()
    {
        var problem =
            new UlsProblem(
                [5.0, 5.0],
                [20.0, 20.0],
                [1.0, 1.0],
                [1.0, 0.0]);

        var result =
            new ChiuTingModifiedPartPeriodBalancingSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            ULSAlgorithms.Abstractions.UlsSolverKind.Heuristic,
            new ChiuTingModifiedPartPeriodBalancingSolver().Kind);

        Assert.Equal(
            ULSAlgorithms.Results.UlsSolveStatus.Feasible,
            result.Status);
    }
}
