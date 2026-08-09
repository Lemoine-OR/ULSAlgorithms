using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.Wagelmans;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using ULSAlgorithms.Tests.Exact.WagnerWhitin;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.Wagelmans;

public sealed class WagelmansGeneralSolverTests
{
    [Fact]
    public void Solver_ImplementsCommonStrategyContract()
    {
        IUlsSolver solver = new WagelmansGeneralSolver();

        Assert.Equal(UlsSolverKind.Exact, solver.Kind);
        Assert.Equal("Wagelmans general O(n log n)", solver.Name);
    }

    [Fact]
    public void Solve_HandlesStronglyNonmonotoneGeneralCosts()
    {
        var problem = new UlsProblem(
            [6.0, 0.0, 8.0, 5.0, 9.0, 3.0],
            [18.0, 31.0, 12.0, 22.0, 15.0, 19.0],
            [20.0, 1.0, 18.0, 2.0, 16.0, 3.0],
            [1.0, 4.0, 1.0, 5.0, 2.0, 0.0]);

        var token = TestContext.Current.CancellationToken;

        var expected = QuadraticWagnerWhitinOracle.GetOptimalCost(
            problem,
            token);

        var result = new WagelmansGeneralSolver().Solve(
            problem,
            token);

        Assert.Equal(UlsSolveStatus.Optimal, result.Status);
        Assert.NotNull(result.Solution);
        AssertClose(expected, result.Solution.TotalCost);
    }

    [Fact]
    public void Solve_AllZeroDemand_ReturnsZeroCostWithoutSetups()
    {
        var problem = new UlsProblem(
            [0.0, 0.0, 0.0, 0.0],
            [10.0, 20.0, 30.0, 40.0],
            [15.0, 1.0, 12.0, 3.0],
            [2.0, 4.0, 1.0, 0.0]);

        var result = new WagelmansGeneralSolver().Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.NotNull(result.Solution);
        AssertClose(0.0, result.Solution.TotalCost);
        Assert.DoesNotContain(true, result.Solution.SetupDecisions.ToArray());
    }

    [Fact]
    public void Solve_ZeroDemandPeriodMayStillBeOptimalProductionPeriod()
    {
        var problem = new UlsProblem(
            [0.0, 10.0],
            [1.0, 100.0],
            [1.0, 20.0],
            [1.0, 0.0]);

        var result = new WagelmansGeneralSolver().Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.NotNull(result.Solution);
        AssertClose(21.0, result.Solution.TotalCost);
        Assert.Equal([true, false], result.Solution.SetupDecisions.ToArray());
        Assert.Equal([10.0, 0.0], result.Solution.ProductionQuantities.ToArray());
    }

    [Fact]
    public void PublishedWagnerWhitinExample_AgreesWithAllExistingExactImplementations()
    {
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

        var token = TestContext.Current.CancellationToken;

        var general = new WagelmansGeneralSolver().Solve(problem, token);
        var classical = new WagnerWhitinClassicalSolver().Solve(problem, token);
        var evans = new WagnerWhitinEvansSolver().Solve(problem, token);
        var linear = new WagnerWhitinSolver().Solve(problem, token);

        AssertClose(864.0, general.ObjectiveValue!.Value);
        AssertClose(general.ObjectiveValue.Value, classical.ObjectiveValue!.Value);
        AssertClose(general.ObjectiveValue.Value, evans.ObjectiveValue!.Value);
        AssertClose(general.ObjectiveValue.Value, linear.ObjectiveValue!.Value);
    }

    [Fact]
    public void RandomGeneralInstances_MatchQuadraticOracleClassicalAndEvans()
    {
        var token = TestContext.Current.CancellationToken;
        var random = new Random(19920201);

        var general = new WagelmansGeneralSolver();
        var classical = new WagnerWhitinClassicalSolver();
        var evans = new WagnerWhitinEvansSolver();

        const int instanceCount = 1_000;

        for (var instance = 0; instance < instanceCount; instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon = random.Next(1, 41);
            var demands = new double[horizon];
            var setupCosts = new double[horizon];
            var productionCosts = new double[horizon];
            var holdingCosts = new double[horizon];

            for (var period = 0; period < horizon; period++)
            {
                demands[period] = random.Next(0, 31);
                setupCosts[period] = random.Next(0, 101);
                productionCosts[period] = random.Next(0, 41);
                holdingCosts[period] = random.Next(0, 11);
            }

            var problem = new UlsProblem(
                demands,
                setupCosts,
                productionCosts,
                holdingCosts);

            var expected = QuadraticWagnerWhitinOracle.GetOptimalCost(
                problem,
                token);

            var a = general.Solve(problem, token).ObjectiveValue!.Value;
            var b = classical.Solve(problem, token).ObjectiveValue!.Value;
            var c = evans.Solve(problem, token).ObjectiveValue!.Value;

            AssertClose(expected, a, $"general instance {instance}");
            AssertClose(expected, b, $"classical instance {instance}");
            AssertClose(expected, c, $"Evans instance {instance}");
        }
    }

    [Fact]
    public void RandomWagnerWhitinInstances_MatchLinearSpecialization()
    {
        var token = TestContext.Current.CancellationToken;
        var random = new Random(20260809);

        var general = new WagelmansGeneralSolver();
        var linear = new WagnerWhitinSolver();

        const int instanceCount = 500;

        for (var instance = 0; instance < instanceCount; instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon = random.Next(1, 101);
            var demands = new double[horizon];
            var setupCosts = new double[horizon];
            var productionCosts = new double[horizon];
            var holdingCosts = new double[horizon];

            for (var period = 0; period < horizon; period++)
            {
                demands[period] = random.Next(0, 31);
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

            var a = general.Solve(problem, token).ObjectiveValue!.Value;
            var b = linear.Solve(problem, token).ObjectiveValue!.Value;

            AssertClose(a, b, $"instance {instance}");
        }
    }

    [Fact]
    public void Solve_HonorsCancellation()
    {
        var problem = new UlsProblem(
            [1.0, 2.0, 3.0],
            [10.0, 10.0, 10.0],
            [8.0, 1.0, 9.0],
            [1.0, 1.0, 0.0]);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            new WagelmansGeneralSolver().Solve(
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
