using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.WagnerWhitin;

public sealed class BahlTajPlanningHorizonSolverTests
{
    [Fact]
    public void Solver_ImplementsCommonStrategyContract()
    {
        IUlsSolver solver =
            new BahlTajPlanningHorizonSolver();

        Assert.Equal(
            UlsSolverKind.Exact,
            solver.Kind);

        Assert.Equal(
            "Bahl-Taj planning-horizon Wagner-Whitin",
            solver.Name);
    }

    [Fact]
    public void PublishedWagelmansExample_Returns864()
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
            new BahlTajPlanningHorizonSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            UlsSolveStatus.Optimal,
            result.Status);

        AssertClose(
            864.0,
            result.ObjectiveValue!.Value);
    }

    [Fact]
    public void ZeroDemandGap_DoesNotAdvancePlanningHorizonIncorrectly()
    {
        // Ordering in period 0 for both positive-demand periods is optimal.
        // Period 1 has zero demand and must not be treated as a setup-based
        // planning-horizon certificate merely because F(2) = F(1).
        var problem = new UlsProblem(
            [10.0, 0.0, 10.0],
            [100.0, 100.0, 100.0],
            [0.0, 0.0, 0.0],
            [1.0, 1.0, 0.0]);

        var token =
            TestContext.Current.CancellationToken;

        var expected =
            QuadraticWagnerWhitinOracle.GetOptimalCost(
                problem,
                token);

        var result =
            new BahlTajPlanningHorizonSolver().Solve(
                problem,
                token);

        AssertClose(
            expected,
            result.ObjectiveValue!.Value);
    }

    [Fact]
    public void ZeroDemandPeriod_CanRemainCandidateForFutureProduction()
    {
        // Period 0 has no demand but is the cheapest setup/production period
        // for the demand in period 1.
        var problem = new UlsProblem(
            [0.0, 10.0],
            [1.0, 100.0],
            [1.0, 2.0],
            [1.0, 0.0]);

        Assert.True(
            BahlTajPlanningHorizonSolver.IsApplicable(problem));

        var result =
            new BahlTajPlanningHorizonSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.NotNull(result.Solution);

        AssertClose(
            21.0,
            result.Solution.TotalCost);

        Assert.Equal(
            [true, false],
            result.Solution.SetupDecisions.ToArray());

        Assert.Equal(
            [10.0, 0.0],
            result.Solution.ProductionQuantities.ToArray());
    }

    [Fact]
    public void AllZeroDemand_ReturnsZeroWithoutSetups()
    {
        var problem = new UlsProblem(
            [0.0, 0.0, 0.0, 0.0],
            [10.0, 20.0, 30.0, 40.0],
            [8.0, 7.0, 6.0, 5.0],
            [2.0, 3.0, 4.0, 0.0]);

        Assert.True(
            BahlTajPlanningHorizonSolver.IsApplicable(problem));

        var result =
            new BahlTajPlanningHorizonSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.NotNull(result.Solution);

        AssertClose(
            0.0,
            result.Solution.TotalCost);

        Assert.DoesNotContain(
            true,
            result.Solution.SetupDecisions.ToArray());
    }

    [Fact]
    public void SpeculativeCostInstance_IsRejected()
    {
        var problem = new UlsProblem(
            [6.0, 8.0, 5.0],
            [18.0, 12.0, 22.0],
            [1.0, 20.0, 2.0],
            [1.0, 1.0, 0.0]);

        Assert.False(
            BahlTajPlanningHorizonSolver.IsApplicable(problem));

        Assert.Throws<NotSupportedException>(() =>
            new BahlTajPlanningHorizonSolver().Solve(
                problem,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public void RandomWagnerWhitinInstances_MatchOracleEvansAndLinearSolver()
    {
        var token =
            TestContext.Current.CancellationToken;

        var random =
            new Random(19910291);

        var bahlTaj =
            new BahlTajPlanningHorizonSolver();

        var evans =
            new WagnerWhitinEvansSolver();

        var linear =
            new WagnerWhitinSolver();

        const int instanceCount = 5_000;

        for (var instance = 0;
             instance < instanceCount;
             instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon =
                random.Next(1, 101);

            var demands =
                new double[horizon];

            var setupCosts =
                new double[horizon];

            var productionCosts =
                new double[horizon];

            var holdingCosts =
                new double[horizon];

            for (var period = 0;
                 period < horizon;
                 period++)
            {
                demands[period] =
                    random.NextDouble() < 0.20
                        ? 0.0
                        : random.Next(1, 41);

                setupCosts[period] =
                    random.Next(0, 151);

                holdingCosts[period] =
                    random.Next(0, 16);
            }

            productionCosts[0] =
                random.Next(0, 31);

            for (var period = 1;
                 period < horizon;
                 period++)
            {
                var upperBound =
                    (int)(
                        productionCosts[period - 1] +
                        holdingCosts[period - 1]);

                productionCosts[period] =
                    random.Next(
                        0,
                        upperBound + 1);
            }

            var problem =
                new UlsProblem(
                    demands,
                    setupCosts,
                    productionCosts,
                    holdingCosts);

            Assert.True(
                BahlTajPlanningHorizonSolver.IsApplicable(problem));

            var expected =
                QuadraticWagnerWhitinOracle.GetOptimalCost(
                    problem,
                    token);

            var a =
                bahlTaj
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var b =
                evans
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var c =
                linear
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            AssertClose(
                expected,
                a,
                $"Bahl-Taj instance {instance}");

            AssertClose(
                expected,
                b,
                $"Evans instance {instance}");

            AssertClose(
                expected,
                c,
                $"linear instance {instance}");
        }
    }

    [Fact]
    public void FrequentSetupInstances_MatchLinearWagnerWhitinSolver()
    {
        var token =
            TestContext.Current.CancellationToken;

        var random =
            new Random(19910991);

        var bahlTaj =
            new BahlTajPlanningHorizonSolver();

        var linear =
            new WagnerWhitinSolver();

        const int instanceCount = 1_000;

        for (var instance = 0;
             instance < instanceCount;
             instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon =
                random.Next(2, 151);

            var demands =
                new double[horizon];

            var setupCosts =
                new double[horizon];

            var productionCosts =
                new double[horizon];

            var holdingCosts =
                new double[horizon];

            for (var period = 0;
                 period < horizon;
                 period++)
            {
                demands[period] =
                    random.Next(1, 31);

                // Low setups and large holding costs make late/frequent
                // setups common, exercising planning-horizon advancement.
                setupCosts[period] =
                    random.Next(0, 6);

                productionCosts[period] =
                    10.0;

                holdingCosts[period] =
                    random.Next(20, 51);
            }

            var problem =
                new UlsProblem(
                    demands,
                    setupCosts,
                    productionCosts,
                    holdingCosts);

            var a =
                bahlTaj
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var b =
                linear
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            AssertClose(
                a,
                b,
                $"instance {instance}");
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

        using var cancellation =
            new CancellationTokenSource();

        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            new BahlTajPlanningHorizonSolver().Solve(
                problem,
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

        var tolerance =
            1e-10 * scale;

        Assert.True(
            Math.Abs(expected - actual) <= tolerance,
            $"Expected {expected:R}, actual {actual:R}, " +
            $"tolerance {tolerance:R}" +
            (context is null
                ? string.Empty
                : $", {context}."));
    }
}
