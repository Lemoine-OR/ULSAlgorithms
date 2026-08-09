using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.ChowdhuryBakiAzab;
using ULSAlgorithms.Exact.FedergruenTzur;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using ULSAlgorithms.Tests.Exact.WagnerWhitin;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.ChowdhuryBakiAzab;

public sealed class ChowdhuryBakiAzabSolverTests
{
    [Fact]
    public void Solver_ImplementsCommonStrategyContract()
    {
        IUlsSolver solver = new ChowdhuryBakiAzabSolver();

        Assert.Equal(UlsSolverKind.Exact, solver.Kind);
        Assert.Equal("Chowdhury-Baki-Azab O(T)", solver.Name);
    }

    [Fact]
    public void SinglePeriod_IsSolvedExactly()
    {
        var problem = new UlsProblem(
            [25.0],
            [17.0],
            [3.0],
            [99.0]);

        var result = new ChowdhuryBakiAzabSolver().Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.Equal(UlsSolveStatus.Optimal, result.Status);
        Assert.NotNull(result.Solution);
        Assert.Equal([25.0], result.Solution.ProductionQuantities.ToArray());
    }

    [Fact]
    public void ZeroDemand_IsRejectedConservatively()
    {
        var problem = new UlsProblem(
            [5.0, 0.0, 8.0],
            [10.0, 12.0, 11.0],
            [2.0, 2.0, 2.0],
            [1.0, 1.0, 0.0]);

        Assert.False(ChowdhuryBakiAzabSolver.IsApplicable(problem));

        Assert.Throws<NotSupportedException>(() =>
            new ChowdhuryBakiAzabSolver().Solve(
                problem,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public void RandomPublishedDomainInstances_MatchOracleAndLinearSolvers()
    {
        var token = TestContext.Current.CancellationToken;
        var random = new Random(20180110);

        var solver = new ChowdhuryBakiAzabSolver();
        var wagelmans = new WagnerWhitinSolver();
        var federgruenTzur =
            new FedergruenTzurNoSpeculativeMotiveSolver();

        const int instanceCount = 5_000;

        for (var instance = 0; instance < instanceCount; instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon = random.Next(1, 151);
            var demands = new double[horizon];
            var setupCosts = new double[horizon];
            var productionCosts = new double[horizon];
            var holdingCosts = new double[horizon];

            var productionCost = random.Next(0, 31);
            var holdingCost = random.Next(1, 16);

            for (var period = 0; period < horizon; period++)
            {
                demands[period] = random.Next(1, 51);
                setupCosts[period] = random.Next(0, 151);
                productionCosts[period] = productionCost;
                holdingCosts[period] = holdingCost;
            }

            var problem = new UlsProblem(
                demands,
                setupCosts,
                productionCosts,
                holdingCosts);

            Assert.True(ChowdhuryBakiAzabSolver.IsApplicable(problem));

            var expected =
                QuadraticWagnerWhitinOracle.GetOptimalCost(problem, token);

            var a = solver.Solve(problem, token).ObjectiveValue!.Value;
            var b = wagelmans.Solve(problem, token).ObjectiveValue!.Value;
            var c = federgruenTzur.Solve(problem, token).ObjectiveValue!.Value;

            AssertClose(expected, a, $"CBA instance {instance}");
            AssertClose(expected, b, $"Wagelmans instance {instance}");
            AssertClose(expected, c, $"FT instance {instance}");
        }
    }

    [Fact]
    public void Solve_HonorsCancellation()
    {
        var problem = new UlsProblem(
            [1.0, 2.0, 3.0],
            [10.0, 10.0, 10.0],
            [4.0, 4.0, 4.0],
            [1.0, 1.0, 1.0]);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            new ChowdhuryBakiAzabSolver().Solve(
                problem,
                cancellation.Token));
    }

    private static void AssertClose(
        double expected,
        double actual,
        string? context = null)
    {
        var scale = Math.Max(1.0, Math.Max(Math.Abs(expected), Math.Abs(actual)));
        var tolerance = 1e-10 * scale;

        Assert.True(
            Math.Abs(expected - actual) <= tolerance,
            $"Expected {expected:R}, actual {actual:R}, tolerance {tolerance:R}" +
            (context is null ? string.Empty : $", {context}."));
    }
}
