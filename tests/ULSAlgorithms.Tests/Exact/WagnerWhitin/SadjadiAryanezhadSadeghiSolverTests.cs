using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.WagnerWhitin;

public sealed class SadjadiAryanezhadSadeghiSolverTests
{
    [Fact]
    public void Solver_ImplementsCommonStrategyContract()
    {
        IUlsSolver solver = new SadjadiAryanezhadSadeghiSolver();

        Assert.Equal(UlsSolverKind.Exact, solver.Kind);
        Assert.Equal(
            "Sadjadi-Aryanezhad-Sadeghi improved Wagner-Whitin",
            solver.Name);
    }

    [Fact]
    public void PublishedExample_DppIs135_AndMatchesOracle()
    {
        double[] demands =
        [
            10.0, 62.0, 12.0, 130.0, 154.0, 129.0,
            88.0, 52.0, 124.0, 160.0, 238.0, 41.0
        ];

        var problem = new UlsProblem(
            demands,
            Enumerable.Repeat(54.0, 12).ToArray(),
            new double[12],
            Enumerable.Repeat(0.4, 12).ToArray());

        Assert.True(SadjadiAryanezhadSadeghiSolver.IsApplicable(problem));
        AssertClose(
            135.0,
            SadjadiAryanezhadSadeghiSolver
                .GetDerivedPartPeriodThreshold(problem));

        var token = TestContext.Current.CancellationToken;
        var expected =
            QuadraticWagnerWhitinOracle.GetOptimalCost(problem, token);

        var result =
            new SadjadiAryanezhadSadeghiSolver().Solve(problem, token);

        Assert.Equal(UlsSolveStatus.Optimal, result.Status);
        AssertClose(501.2, expected);
        AssertClose(expected, result.ObjectiveValue!.Value);
    }

    [Fact]
    public void RandomFixedCostInstances_MatchHeadyZhuAndOracle()
    {
        var token = TestContext.Current.CancellationToken;
        var random = new Random(20090301);

        var sadjadi = new SadjadiAryanezhadSadeghiSolver();
        var headyZhu = new HeadyZhuEconomicPartPeriodSolver();

        const int instanceCount = 3_000;

        for (var instance = 0; instance < instanceCount; instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon = random.Next(1, 101);
            var demands = new double[horizon];
            var setupCosts = new double[horizon];
            var productionCosts = new double[horizon];
            var holdingCosts = new double[horizon];

            var setupCost = random.Next(0, 201);
            var productionCost = random.Next(0, 31);
            var holdingCost = random.Next(0, 16);

            for (var period = 0; period < horizon; period++)
            {
                demands[period] =
                    random.NextDouble() < 0.20
                        ? 0.0
                        : random.Next(1, 51);

                setupCosts[period] = setupCost;
                productionCosts[period] = productionCost;
                holdingCosts[period] = holdingCost;
            }

            var problem = new UlsProblem(
                demands,
                setupCosts,
                productionCosts,
                holdingCosts);

            var expected =
                QuadraticWagnerWhitinOracle.GetOptimalCost(problem, token);

            var a = sadjadi.Solve(problem, token).ObjectiveValue!.Value;
            var b = headyZhu.Solve(problem, token).ObjectiveValue!.Value;

            AssertClose(expected, a, $"Sadjadi instance {instance}");
            AssertClose(expected, b, $"Heady-Zhu instance {instance}");
        }
    }

    [Fact]
    public void VaryingSetupCosts_AreRejected()
    {
        var problem = new UlsProblem(
            [5.0, 6.0],
            [10.0, 11.0],
            [2.0, 2.0],
            [1.0, 1.0]);

        Assert.False(SadjadiAryanezhadSadeghiSolver.IsApplicable(problem));
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
            new SadjadiAryanezhadSadeghiSolver().Solve(
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
