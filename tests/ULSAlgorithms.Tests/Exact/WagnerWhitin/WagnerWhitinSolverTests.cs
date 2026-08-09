using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.WagnerWhitin;

public sealed class WagnerWhitinSolverTests
{
    [Fact]
    public void Solver_ImplementsCommonStrategyContract()
    {
        IUlsSolver solver = new WagnerWhitinSolver();

        Assert.Equal(UlsSolverKind.Exact, solver.Kind);
        Assert.Equal(
            "Wagner-Whitin (Wagelmans linear-time)",
            solver.Name);
    }

    [Fact]
    public void Solve_ReproducesWagelmansPublishedWagnerWhitinExample()
    {
        // Wagelmans, van Hoesel and Kolen (1992), Table I.
        // Their transformed marginal costs are 12, 11, ..., 1.
        // Setting p[t] = 0 and h[t] = 1 reproduces those transformed costs.
        double[] demands =
        [
            69.0, 29.0, 36.0, 61.0, 61.0, 26.0,
            34.0, 67.0, 45.0, 67.0, 79.0, 56.0
        ];

        double[] setupCosts =
        [
            85.0, 102.0, 102.0, 101.0, 98.0, 114.0,
            105.0, 86.0, 119.0, 110.0, 98.0, 114.0
        ];

        var problem = new UlsProblem(
            demands,
            setupCosts,
            new double[12],
            Enumerable.Repeat(1.0, 12).ToArray());

        var solver = new WagnerWhitinSolver();
        var result = solver.Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.Equal(UlsSolveStatus.Optimal, result.Status);
        Assert.NotNull(result.Solution);

        var solution = result.Solution;

        AssertClose(864.0, solution.TotalCost);
        AssertClose(579.0, solution.SetupCost);
        AssertClose(0.0, solution.ProductionCost);
        AssertClose(285.0, solution.HoldingCost);

        Assert.Equal(
            [
                true, false, true, false, true, false,
                false, true, false, true, true, false
            ],
            solution.SetupDecisions.ToArray());

        Assert.Equal(
            [98.0, 0.0, 97.0, 0.0, 121.0, 0.0,
             0.0, 112.0, 0.0, 67.0, 135.0, 0.0],
            solution.ProductionQuantities.ToArray());
    }

    [Fact]
    public void Solve_SupportsBroaderNoSpeculativeMotiveCosts()
    {
        var problem = new UlsProblem(
            [4.0, 8.0, 3.0, 9.0],
            [20.0, 15.0, 18.0, 10.0],
            [8.0, 9.0, 9.5, 10.0],
            [2.0, 1.0, 1.0, 0.0]);

        Assert.True(WagnerWhitinSolver.IsApplicable(problem));

        var solver = new WagnerWhitinSolver();
        var result = solver.Solve(
            problem,
            TestContext.Current.CancellationToken);

        var expected = QuadraticWagnerWhitinOracle.GetOptimalCost(
            problem,
            TestContext.Current.CancellationToken);

        Assert.Equal(UlsSolveStatus.Optimal, result.Status);
        AssertClose(expected, result.ObjectiveValue!.Value);
    }

    [Fact]
    public void Solve_HandlesZeroDemandProductionPeriod()
    {
        var problem = new UlsProblem(
            [0.0, 2.0],
            [3.0, 20.0],
            [1.0, 0.0],
            [2.0, 0.0]);

        var solver = new WagnerWhitinSolver();
        var result = solver.Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.NotNull(result.Solution);
        AssertClose(9.0, result.Solution.TotalCost);
        Assert.Equal([true, false], result.Solution.SetupDecisions.ToArray());
        Assert.Equal([2.0, 0.0], result.Solution.ProductionQuantities.ToArray());
        Assert.Equal([2.0, 0.0], result.Solution.EndingInventories.ToArray());
    }

    [Fact]
    public void Solve_AllZeroDemand_ReturnsZeroCostWithoutSetups()
    {
        var problem = new UlsProblem(
            [0.0, 0.0, 0.0],
            [100.0, 100.0, 100.0],
            [4.0, 4.0, 4.0],
            [1.0, 1.0, 0.0]);

        var solver = new WagnerWhitinSolver();
        var result = solver.Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.NotNull(result.Solution);
        AssertClose(0.0, result.Solution.TotalCost);
        Assert.Equal(
            [false, false, false],
            result.Solution.SetupDecisions.ToArray());
    }

    [Fact]
    public void Solve_RejectsSpeculativeMotiveInstance()
    {
        var problem = new UlsProblem(
            [5.0, 5.0],
            [10.0, 10.0],
            [1.0, 5.0],
            [1.0, 0.0]);

        Assert.False(WagnerWhitinSolver.IsApplicable(problem));

        var solver = new WagnerWhitinSolver();

        Assert.Throws<NotSupportedException>(() =>
            solver.Solve(
                problem,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Solve_HonorsCancellation()
    {
        var problem = new UlsProblem(
            [1.0, 1.0, 1.0],
            [1.0, 1.0, 1.0],
            [1.0, 1.0, 1.0],
            [1.0, 1.0, 0.0]);

        var solver = new WagnerWhitinSolver();

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            solver.Solve(problem, cancellation.Token));
    }

    [Fact]
    public void RandomInstances_MatchIndependentQuadraticOracle()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var random = new Random(20260809);
        var solver = new WagnerWhitinSolver();

        const int instanceCount = 1_000;

        for (var instance = 0; instance < instanceCount; instance++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var horizon = random.Next(1, 41);
            var demands = new double[horizon];
            var setupCosts = new double[horizon];
            var productionCosts = new double[horizon];
            var holdingCosts = new double[horizon];

            for (var period = 0; period < horizon; period++)
            {
                demands[period] = random.Next(0, 21);
                setupCosts[period] = random.Next(0, 101);
                holdingCosts[period] = random.Next(0, 11);
            }

            productionCosts[0] = random.Next(0, 21);

            for (var period = 1; period < horizon; period++)
            {
                var upperBound =
                    (int)(productionCosts[period - 1] +
                          holdingCosts[period - 1]);

                productionCosts[period] = random.Next(0, upperBound + 1);
            }

            var problem = new UlsProblem(
                demands,
                setupCosts,
                productionCosts,
                holdingCosts);

            var expected = QuadraticWagnerWhitinOracle.GetOptimalCost(
                problem,
                cancellationToken);

            var actual = solver.Solve(
                problem,
                cancellationToken);

            Assert.Equal(UlsSolveStatus.Optimal, actual.Status);
            Assert.NotNull(actual.Solution);
            AssertClose(
                expected,
                actual.Solution.TotalCost,
                $"instance {instance}, horizon {horizon}");
        }
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
