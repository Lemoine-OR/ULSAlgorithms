using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Heuristics;

public sealed class LiteratureHeuristicsPackIIIValidationTests
{
    [Fact]
    public void NewHeuristics_AreFeasibleAndNeverBeatExactOptimum()
    {
        var token =
            TestContext.Current.CancellationToken;

        var random =
            new Random(20230810);

        IUlsSolver[] heuristics =
        [
            new PartPeriodSimplifiedSolver(),
            new SegerstedtReformulatedSilverMealSolver(),
            new ChiuModifiedLeastUnitCostSolver(),
            new ChiuTingModifiedPartPeriodBalancingSolver()
        ];

        var exact =
            new WagnerWhitinSolver();

        const int instanceCount = 1_000;

        for (var instance = 0;
             instance < instanceCount;
             instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon =
                random.Next(1, 81);

            var demands =
                new double[horizon];

            var setupCosts =
                new double[horizon];

            var productionCosts =
                new double[horizon];

            var holdingCosts =
                new double[horizon];

            var setupCost =
                random.Next(0, 201);

            var productionCost =
                random.Next(0, 21);

            var holdingCost =
                random.Next(0, 16);

            for (var period = 0;
                 period < horizon;
                 period++)
            {
                demands[period] =
                    random.NextDouble() < 0.30
                        ? 0.0
                        : random.Next(1, 51);

                setupCosts[period] =
                    setupCost;

                productionCosts[period] =
                    productionCost;

                holdingCosts[period] =
                    holdingCost;
            }

            var problem =
                new UlsProblem(
                    demands,
                    setupCosts,
                    productionCosts,
                    holdingCosts);

            var optimum =
                exact
                    .Solve(
                        problem,
                        token)
                    .ObjectiveValue!
                    .Value;

            foreach (IUlsSolver heuristic in heuristics)
            {
                UlsSolveResult result =
                    heuristic.Solve(
                        problem,
                        token);

                Assert.Equal(
                    UlsSolveStatus.Feasible,
                    result.Status);

                Assert.NotNull(
                    result.Solution);

                var tolerance =
                    1.0e-9 *
                    Math.Max(
                        1.0,
                        optimum);

                Assert.True(
                    result.ObjectiveValue!.Value +
                    tolerance >=
                    optimum,
                    $"{heuristic.Name}, instance {instance}: " +
                    $"heuristic={result.ObjectiveValue.Value:R}, " +
                    $"optimum={optimum:R}.");
            }
        }
    }

    [Fact]
    public void NewStationaryHeuristics_RejectTimeVaryingSetupCosts()
    {
        var problem =
            new UlsProblem(
                [5.0, 6.0, 7.0],
                [10.0, 11.0, 10.0],
                [2.0, 2.0, 2.0],
                [1.0, 1.0, 0.0]);

        IUlsSolver[] heuristics =
        [
            new PartPeriodSimplifiedSolver(),
            new SegerstedtReformulatedSilverMealSolver(),
            new ChiuModifiedLeastUnitCostSolver(),
            new ChiuTingModifiedPartPeriodBalancingSolver()
        ];

        foreach (IUlsSolver heuristic in heuristics)
        {
            Assert.Throws<NotSupportedException>(
                () =>
                    heuristic.Solve(
                        problem,
                        TestContext.Current.CancellationToken));
        }
    }
}
