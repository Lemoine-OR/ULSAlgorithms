using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.FedergruenTzur;
using ULSAlgorithms.Exact.Wagelmans;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using ULSAlgorithms.Tests.Exact.WagnerWhitin;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.FedergruenTzur;

public sealed class FedergruenTzurLinearSolversTests
{
    [Fact]
    public void BothSolvers_ImplementCommonStrategyContract()
    {
        IUlsSolver noSpeculative =
            new FedergruenTzurNoSpeculativeMotiveSolver();

        IUlsSolver nondecreasingSetup =
            new FedergruenTzurNondecreasingSetupSolver();

        Assert.Equal(UlsSolverKind.Exact, noSpeculative.Kind);
        Assert.Equal(UlsSolverKind.Exact, nondecreasingSetup.Kind);
    }

    [Fact]
    public void NoSpeculativeSolver_RejectsSpeculativeInstance()
    {
        var problem = new UlsProblem(
            [5.0, 5.0],
            [10.0, 10.0],
            [1.0, 5.0],
            [1.0, 0.0]);

        Assert.False(
            FedergruenTzurNoSpeculativeMotiveSolver.IsApplicable(problem));

        Assert.Throws<NotSupportedException>(() =>
            new FedergruenTzurNoSpeculativeMotiveSolver().Solve(
                problem,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public void NondecreasingSetupSolver_RejectsDecreasingSetupCost()
    {
        var problem = new UlsProblem(
            [5.0, 5.0],
            [10.0, 9.0],
            [1.0, 1.0],
            [1.0, 0.0]);

        Assert.False(
            FedergruenTzurNondecreasingSetupSolver.IsApplicable(problem));

        Assert.Throws<NotSupportedException>(() =>
            new FedergruenTzurNondecreasingSetupSolver().Solve(
                problem,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public void PublishedWagelmansExample_NoSpeculativeVariantReturns864()
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

        var result =
            new FedergruenTzurNoSpeculativeMotiveSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(UlsSolveStatus.Optimal, result.Status);
        AssertClose(864.0, result.ObjectiveValue!.Value);
    }

    [Fact]
    public void NondecreasingSetupVariant_AllowsStrongSpeculation()
    {
        var problem = new UlsProblem(
            [4.0, 7.0, 3.0, 8.0],
            [10.0, 10.0, 14.0, 20.0],
            [25.0, 1.0, 30.0, 2.0],
            [1.0, 5.0, 1.0, 0.0]);

        var token = TestContext.Current.CancellationToken;

        var expected =
            QuadraticWagnerWhitinOracle.GetOptimalCost(problem, token);

        var result =
            new FedergruenTzurNondecreasingSetupSolver().Solve(
                problem,
                token);

        AssertClose(expected, result.ObjectiveValue!.Value);
    }

    [Fact]
    public void RandomNoSpeculativeInstances_AllCompatibleExactSolversAgree()
    {
        var token = TestContext.Current.CancellationToken;
        var random = new Random(19910401);

        var federgruenLinear =
            new FedergruenTzurNoSpeculativeMotiveSolver();

        var federgruenGeneral =
            new FedergruenTzurSolver();

        var wagelmansLinear =
            new WagnerWhitinSolver();

        var wagelmansGeneral =
            new WagelmansGeneralSolver();

        const int instanceCount = 2_000;

        for (var instance = 0;
             instance < instanceCount;
             instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon = random.Next(1, 101);

            var demands = new double[horizon];
            var setupCosts = new double[horizon];
            var productionCosts = new double[horizon];
            var holdingCosts = new double[horizon];

            for (var period = 0;
                 period < horizon;
                 period++)
            {
                demands[period] = random.Next(0, 31);
                setupCosts[period] = random.Next(0, 101);
                holdingCosts[period] = random.Next(0, 11);
            }

            productionCosts[0] = random.Next(0, 21);

            for (var period = 1;
                 period < horizon;
                 period++)
            {
                var upperBound =
                    (int)(
                        productionCosts[period - 1] +
                        holdingCosts[period - 1]);

                productionCosts[period] =
                    random.Next(0, upperBound + 1);
            }

            var problem = new UlsProblem(
                demands,
                setupCosts,
                productionCosts,
                holdingCosts);

            var expected =
                QuadraticWagnerWhitinOracle.GetOptimalCost(
                    problem,
                    token);

            var a =
                federgruenLinear
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var b =
                federgruenGeneral
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var c =
                wagelmansLinear
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var d =
                wagelmansGeneral
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            AssertClose(expected, a, $"FT-linear instance {instance}");
            AssertClose(expected, b, $"FT-general instance {instance}");
            AssertClose(expected, c, $"Wagelmans-linear instance {instance}");
            AssertClose(expected, d, $"Wagelmans-general instance {instance}");
        }
    }

    [Fact]
    public void RandomNondecreasingSetupInstances_MatchGeneralSolversAndOracle()
    {
        var token = TestContext.Current.CancellationToken;
        var random = new Random(19910301);

        var federgruenLinear =
            new FedergruenTzurNondecreasingSetupSolver();

        var federgruenGeneral =
            new FedergruenTzurSolver();

        var wagelmansGeneral =
            new WagelmansGeneralSolver();

        var evans =
            new WagnerWhitinEvansSolver();

        const int instanceCount = 2_000;

        for (var instance = 0;
             instance < instanceCount;
             instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon = random.Next(1, 61);

            var demands = new double[horizon];
            var setupCosts = new double[horizon];
            var productionCosts = new double[horizon];
            var holdingCosts = new double[horizon];

            var setup = random.Next(0, 11);

            for (var period = 0;
                 period < horizon;
                 period++)
            {
                demands[period] = random.Next(0, 31);
                setup += random.Next(0, 6);
                setupCosts[period] = setup;

                productionCosts[period] = random.Next(0, 51);
                holdingCosts[period] = random.Next(0, 11);
            }

            var problem = new UlsProblem(
                demands,
                setupCosts,
                productionCosts,
                holdingCosts);

            var expected =
                QuadraticWagnerWhitinOracle.GetOptimalCost(
                    problem,
                    token);

            var a =
                federgruenLinear
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var b =
                federgruenGeneral
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var c =
                wagelmansGeneral
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var d =
                evans
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            AssertClose(expected, a, $"FT-linear instance {instance}");
            AssertClose(expected, b, $"FT-general instance {instance}");
            AssertClose(expected, c, $"Wagelmans instance {instance}");
            AssertClose(expected, d, $"Evans instance {instance}");
        }
    }

    [Fact]
    public void BothSolvers_HonorCancellation()
    {
        var noSpecProblem = new UlsProblem(
            [1.0, 2.0, 3.0],
            [10.0, 11.0, 12.0],
            [3.0, 3.0, 3.0],
            [1.0, 1.0, 0.0]);

        var nondecreasingSetupProblem = new UlsProblem(
            [1.0, 2.0, 3.0],
            [10.0, 11.0, 12.0],
            [8.0, 1.0, 9.0],
            [1.0, 1.0, 0.0]);

        using var cancellation =
            new CancellationTokenSource();

        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            new FedergruenTzurNoSpeculativeMotiveSolver().Solve(
                noSpecProblem,
                cancellation.Token));

        Assert.Throws<OperationCanceledException>(() =>
            new FedergruenTzurNondecreasingSetupSolver().Solve(
                nondecreasingSetupProblem,
                cancellation.Token));
    }

    private static void AssertClose(
        double expected,
        double actual,
        string? context = null)
    {
        var scale =
            Math.Max(
                1.0,
                Math.Max(
                    Math.Abs(expected),
                    Math.Abs(actual)));

        var tolerance = 1e-10 * scale;

        Assert.True(
            Math.Abs(expected - actual) <= tolerance,
            $"Expected {expected:R}, actual {actual:R}, " +
            $"tolerance {tolerance:R}" +
            (context is null
                ? string.Empty
                : $", {context}."));
    }
}
