using ULSAlgorithms.Exact.Wagelmans;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using ULSAlgorithms.Selection;
using Xunit;

namespace ULSAlgorithms.Tests.Selection;

public sealed class CachedAdaptiveDispatchTests
{
    [Fact]
    public void Selection_CachedApplicabilityMatchesWagnerWhitinCheck()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var random = new Random(20260810);
        var selector = new AdaptiveExactUlsSolver();

        const int instanceCount = 1_000;

        for (var instance = 0; instance < instanceCount; instance++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var horizon = random.Next(1, 65);
            var demands = new double[horizon];
            var setupCosts = new double[horizon];
            var productionCosts = new double[horizon];
            var holdingCosts = new double[horizon];

            for (var period = 0; period < horizon; period++)
            {
                demands[period] = random.Next(0, 101);
                setupCosts[period] = random.Next(0, 501);
                productionCosts[period] = random.NextDouble() * 100.0;
                holdingCosts[period] =
                    period == horizon - 1
                        ? 0.0
                        : random.NextDouble() * 10.0;
            }

            var problem = new UlsProblem(
                demands,
                setupCosts,
                productionCosts,
                holdingCosts);

            var expectedLinear =
                WagnerWhitinSolver.IsApplicable(problem);

            var selected = selector.SelectSolver(problem);

            Assert.Equal(
                expectedLinear,
                selected is WagnerWhitinSolver);
        }
    }

    [Fact]
    public void Selection_CachedApplicabilityPreservesFiniteOverflowGuard()
    {
        var problem = new UlsProblem(
            [1.0, 1.0],
            [10.0, 10.0],
            [double.MaxValue, 0.0],
            [double.MaxValue, 0.0]);

        var selector = new AdaptiveExactUlsSolver();

        Assert.False(WagnerWhitinSolver.IsApplicable(problem));
        Assert.IsType<WagelmansGeneralSolver>(
            selector.SelectSolver(problem));
    }

    [Fact]
    public void Solve_NsmCaseMatchesDirectLinearSolver()
    {
        var problem = new UlsProblem(
            [5.0, 0.0, 8.0, 3.0, 11.0, 7.0],
            [30.0, 25.0, 40.0, 35.0, 20.0, 45.0],
            [8.0, 9.0, 9.5, 10.0, 10.5, 11.0],
            [2.0, 1.0, 1.0, 1.0, 1.0, 0.0]);

        var adaptive = new AdaptiveExactUlsSolver();
        var direct = new WagnerWhitinSolver();
        var cancellationToken = TestContext.Current.CancellationToken;

        var adaptiveResult = adaptive.Solve(
            problem,
            cancellationToken);
        var directResult = direct.Solve(
            problem,
            cancellationToken);

        Assert.Equal(UlsSolveStatus.Optimal, adaptiveResult.Status);
        Assert.Equal(UlsSolveStatus.Optimal, directResult.Status);
        Assert.NotNull(adaptiveResult.Solution);
        Assert.NotNull(directResult.Solution);

        Assert.Equal(
            directResult.ObjectiveValue,
            adaptiveResult.ObjectiveValue);
        Assert.Equal(
            directResult.Solution.ProductionQuantities.ToArray(),
            adaptiveResult.Solution.ProductionQuantities.ToArray());
        Assert.Equal(
            directResult.Solution.EndingInventories.ToArray(),
            adaptiveResult.Solution.EndingInventories.ToArray());
        Assert.Equal(
            directResult.Solution.SetupDecisions.ToArray(),
            adaptiveResult.Solution.SetupDecisions.ToArray());
    }
}
