using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.WagnerWhitin;

public sealed class WagnerWhitinClassicalAndEvansTests
{
    [Fact]
    public void PublishedWagelmansExample_AllThreeSolversAgree()
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

        var classical = new WagnerWhitinClassicalSolver().Solve(problem, token);
        var evans = new WagnerWhitinEvansSolver().Solve(problem, token);
        var linear = new WagnerWhitinSolver().Solve(problem, token);

        Assert.Equal(UlsSolveStatus.Optimal, classical.Status);
        Assert.Equal(UlsSolveStatus.Optimal, evans.Status);
        Assert.Equal(UlsSolveStatus.Optimal, linear.Status);

        AssertClose(864.0, classical.ObjectiveValue!.Value);
        AssertClose(classical.ObjectiveValue.Value, evans.ObjectiveValue!.Value);
        AssertClose(classical.ObjectiveValue.Value, linear.ObjectiveValue!.Value);
    }

    [Fact]
    public void GeneralCosts_ClassicalAndEvansAgreeWithIndependentOracle()
    {
        var problem = new UlsProblem(
            [6.0, 0.0, 8.0, 5.0, 9.0],
            [18.0, 31.0, 12.0, 22.0, 15.0],
            [9.0, 1.0, 12.0, 2.0, 11.0],
            [3.0, 4.0, 2.0, 5.0, 0.0]);

        var token = TestContext.Current.CancellationToken;
        var expected = QuadraticWagnerWhitinOracle.GetOptimalCost(problem, token);

        var classical = new WagnerWhitinClassicalSolver().Solve(problem, token);
        var evans = new WagnerWhitinEvansSolver().Solve(problem, token);

        AssertClose(expected, classical.ObjectiveValue!.Value);
        AssertClose(expected, evans.ObjectiveValue!.Value);
    }

    [Fact]
    public void AllZeroDemand_BothSolversReturnZeroCostWithoutSetups()
    {
        var problem = new UlsProblem(
            [0.0, 0.0, 0.0, 0.0],
            [10.0, 20.0, 30.0, 40.0],
            [4.0, 3.0, 2.0, 1.0],
            [2.0, 2.0, 2.0, 0.0]);

        var token = TestContext.Current.CancellationToken;

        foreach (var result in new[]
        {
            new WagnerWhitinClassicalSolver().Solve(problem, token),
            new WagnerWhitinEvansSolver().Solve(problem, token)
        })
        {
            Assert.Equal(UlsSolveStatus.Optimal, result.Status);
            Assert.NotNull(result.Solution);
            AssertClose(0.0, result.Solution.TotalCost);
            Assert.DoesNotContain(true, result.Solution.SetupDecisions.ToArray());
        }
    }

    [Fact]
    public void RandomGeneralInstances_ClassicalAndEvansMatchOracle()
    {
        var token = TestContext.Current.CancellationToken;
        var random = new Random(19850201);
        var classical = new WagnerWhitinClassicalSolver();
        var evans = new WagnerWhitinEvansSolver();

        const int instanceCount = 1_000;

        for (var instance = 0; instance < instanceCount; instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon = random.Next(1, 31);
            var demands = new double[horizon];
            var setupCosts = new double[horizon];
            var productionCosts = new double[horizon];
            var holdingCosts = new double[horizon];

            for (var period = 0; period < horizon; period++)
            {
                demands[period] = random.Next(0, 31);
                setupCosts[period] = random.Next(0, 101);
                productionCosts[period] = random.Next(0, 31);
                holdingCosts[period] = random.Next(0, 11);
            }

            var problem = new UlsProblem(
                demands,
                setupCosts,
                productionCosts,
                holdingCosts);

            var expected = QuadraticWagnerWhitinOracle.GetOptimalCost(problem, token);
            var classicalResult = classical.Solve(problem, token);
            var evansResult = evans.Solve(problem, token);

            AssertClose(
                expected,
                classicalResult.ObjectiveValue!.Value,
                $"classical instance {instance}");

            AssertClose(
                expected,
                evansResult.ObjectiveValue!.Value,
                $"Evans instance {instance}");
        }
    }

    [Fact]
    public void RandomWagnerWhitinInstances_AllThreeImplementationsAgree()
    {
        var token = TestContext.Current.CancellationToken;
        var random = new Random(19920301);

        var classical = new WagnerWhitinClassicalSolver();
        var evans = new WagnerWhitinEvansSolver();
        var linear = new WagnerWhitinSolver();

        const int instanceCount = 500;

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
                holdingCosts[period] = random.Next(0, 11);
            }

            productionCosts[0] = random.Next(0, 21);

            for (var period = 1; period < horizon; period++)
            {
                var upperBound =
                    (int)(productionCosts[period - 1] + holdingCosts[period - 1]);

                productionCosts[period] = random.Next(0, upperBound + 1);
            }

            var problem = new UlsProblem(
                demands,
                setupCosts,
                productionCosts,
                holdingCosts);

            var a = classical.Solve(problem, token).ObjectiveValue!.Value;
            var b = evans.Solve(problem, token).ObjectiveValue!.Value;
            var c = linear.Solve(problem, token).ObjectiveValue!.Value;

            AssertClose(a, b, $"Evans instance {instance}");
            AssertClose(a, c, $"linear instance {instance}");
        }
    }

    [Fact]
    public void Solvers_HonorCancellation()
    {
        var problem = new UlsProblem(
            [1.0, 2.0, 3.0],
            [10.0, 10.0, 10.0],
            [2.0, 2.0, 2.0],
            [1.0, 1.0, 0.0]);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            new WagnerWhitinClassicalSolver().Solve(problem, cancellation.Token));

        Assert.Throws<OperationCanceledException>(() =>
            new WagnerWhitinEvansSolver().Solve(problem, cancellation.Token));
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
