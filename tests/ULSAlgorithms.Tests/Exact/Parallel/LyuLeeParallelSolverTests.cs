using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.Parallel;
using ULSAlgorithms.Exact.Wagelmans;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using ULSAlgorithms.Tests.Exact.WagnerWhitin;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.Parallel;

public sealed class LyuLeeParallelSolverTests
{
    [Fact]
    public void Solver_ImplementsCommonStrategyContract()
    {
        IUlsSolver solver = new LyuLeeParallelSolver();

        Assert.Equal(UlsSolverKind.Exact, solver.Kind);
        Assert.Equal("Lyu-Lee parallel dynamic lot-sizing", solver.Name);
    }

    [Fact]
    public void OneWorkerAndMultipleWorkers_ReturnSameResult()
    {
        var problem = new UlsProblem(
            [6.0, 0.0, 8.0, 5.0, 9.0, 3.0],
            [18.0, 31.0, 12.0, 22.0, 15.0, 19.0],
            [20.0, 1.0, 18.0, 2.0, 16.0, 3.0],
            [1.0, 4.0, 1.0, 5.0, 2.0, 0.0]);

        var token = TestContext.Current.CancellationToken;

        var sequential =
            new LyuLeeParallelSolver(1, 1).Solve(problem, token);

        var parallel =
            new LyuLeeParallelSolver(4, 1).Solve(problem, token);

        AssertClose(
            sequential.ObjectiveValue!.Value,
            parallel.ObjectiveValue!.Value);
    }

    [Fact]
    public void RandomGeneralInstances_MatchOracleAndWagelmansGeneral()
    {
        var token = TestContext.Current.CancellationToken;
        var random = new Random(20011101);

        var parallel = new LyuLeeParallelSolver(4, 16);
        var wagelmans = new WagelmansGeneralSolver();

        const int instanceCount = 2_000;

        for (var instance = 0; instance < instanceCount; instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon = random.Next(1, 61);
            var demands = new double[horizon];
            var setupCosts = new double[horizon];
            var productionCosts = new double[horizon];
            var holdingCosts = new double[horizon];

            for (var period = 0; period < horizon; period++)
            {
                demands[period] =
                    random.NextDouble() < 0.20
                        ? 0.0
                        : random.Next(1, 31);

                setupCosts[period] = random.Next(0, 101);
                productionCosts[period] = random.Next(0, 41);
                holdingCosts[period] = random.Next(0, 11);
            }

            var problem = new UlsProblem(
                demands,
                setupCosts,
                productionCosts,
                holdingCosts);

            var expected =
                QuadraticWagnerWhitinOracle.GetOptimalCost(problem, token);

            var a = parallel.Solve(problem, token).ObjectiveValue!.Value;
            var b = wagelmans.Solve(problem, token).ObjectiveValue!.Value;

            AssertClose(expected, a, $"Lyu-Lee instance {instance}");
            AssertClose(expected, b, $"Wagelmans instance {instance}");
        }
    }

    [Fact]
    public void InvalidParallelism_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LyuLeeParallelSolver(0));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LyuLeeParallelSolver(-2));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LyuLeeParallelSolver(1, 0));
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
            new LyuLeeParallelSolver(2, 1).Solve(
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
