using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Heuristics;

public sealed class LiteratureHeuristicsV023ValidationTests
{
    [Fact]
    public void NewHeuristics_AreFeasibleAndNeverBeatExactOptimum()
    {
        CancellationToken token =
            TestContext.Current.CancellationToken;

        var random =
            new Random(20260810);

        IUlsSolver[] heuristics =
        [
            new HoChangSolisNetLeastPeriodCostSolver(),
            new HoChangSolisImprovedNetLeastPeriodCostSolver(),
            new McLarenOrderMomentSolver(),
            new KarniMaximumPartPeriodGainSolver()
        ];

        var exact =
            new WagnerWhitinSolver();

        const int instanceCount =
            1_000;

        for (int instance = 0;
             instance < instanceCount;
             instance++)
        {
            token.ThrowIfCancellationRequested();

            int horizon =
                random.Next(1, 81);

            var demands =
                new double[horizon];

            var setupCosts =
                new double[horizon];

            var productionCosts =
                new double[horizon];

            var holdingCosts =
                new double[horizon];

            double setupCost =
                random.Next(0, 301);

            double productionCost =
                random.Next(0, 21);

            double holdingCost =
                random.Next(0, 16);

            for (int period = 0;
                 period < horizon;
                 period++)
            {
                demands[period] =
                    random.NextDouble() < 0.25
                        ? 0.0
                        : random.Next(1, 101);

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

            UlsSolveResult optimumResult =
                exact.Solve(
                    problem,
                    token);

            Assert.Equal(
                UlsSolveStatus.Optimal,
                optimumResult.Status);

            double optimum =
                optimumResult.ObjectiveValue!.Value;

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

                double tolerance =
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
    public void NewHeuristics_RejectNonStationaryRelevantCosts()
    {
        var problem =
            new UlsProblem(
                [5.0, 6.0, 7.0],
                [10.0, 11.0, 10.0],
                [2.0, 2.0, 2.0],
                [1.0, 1.0, 0.0]);

        IUlsSolver[] heuristics =
        [
            new HoChangSolisNetLeastPeriodCostSolver(),
            new HoChangSolisImprovedNetLeastPeriodCostSolver(),
            new McLarenOrderMomentSolver(),
            new KarniMaximumPartPeriodGainSolver()
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
