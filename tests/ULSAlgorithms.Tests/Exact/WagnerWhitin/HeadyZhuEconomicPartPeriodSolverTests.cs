using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.WagnerWhitin;

public sealed class HeadyZhuEconomicPartPeriodSolverTests
{
    [Fact]
    public void Solver_ImplementsCommonStrategyContract()
    {
        IUlsSolver solver =
            new HeadyZhuEconomicPartPeriodSolver();

        Assert.Equal(
            UlsSolverKind.Exact,
            solver.Kind);

        Assert.Equal(
            "Heady-Zhu economic-part-period Wagner-Whitin",
            solver.Name);
    }

    [Fact]
    public void PublishedFixedCostExample_HasEconomicPartPeriod135AndOptimalCost501Point2()
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

        Assert.True(
            HeadyZhuEconomicPartPeriodSolver.IsApplicable(problem));

        AssertClose(
            135.0,
            HeadyZhuEconomicPartPeriodSolver
                .GetEconomicPartPeriodThreshold(problem));

        var token =
            TestContext.Current.CancellationToken;

        var expected =
            QuadraticWagnerWhitinOracle.GetOptimalCost(
                problem,
                token);

        var result =
            new HeadyZhuEconomicPartPeriodSolver().Solve(
                problem,
                token);

        Assert.Equal(
            UlsSolveStatus.Optimal,
            result.Status);

        AssertClose(501.2, expected);
        AssertClose(
            expected,
            result.ObjectiveValue!.Value);

        Assert.Equal(
            [true, false, false, true, true, false,
             true, false, true, true, true, false],
            result.Solution!.SetupDecisions.ToArray());

        Assert.Equal(
            [84.0, 0.0, 0.0, 130.0, 283.0, 0.0,
             140.0, 0.0, 124.0, 160.0, 279.0, 0.0],
            result.Solution.ProductionQuantities.ToArray());
    }

    [Fact]
    public void AllZeroDemand_ReturnsZeroWithoutSetups()
    {
        var problem = new UlsProblem(
            [0.0, 0.0, 0.0, 0.0],
            [25.0, 25.0, 25.0, 25.0],
            [7.0, 7.0, 7.0, 7.0],
            [2.0, 2.0, 2.0, 2.0]);

        var result =
            new HeadyZhuEconomicPartPeriodSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.NotNull(result.Solution);
        AssertClose(0.0, result.Solution.TotalCost);

        Assert.DoesNotContain(
            true,
            result.Solution.SetupDecisions.ToArray());
    }

    [Fact]
    public void ZeroHoldingCost_IsSupported()
    {
        var problem = new UlsProblem(
            [5.0, 0.0, 7.0, 3.0],
            [20.0, 20.0, 20.0, 20.0],
            [4.0, 4.0, 4.0, 4.0],
            [0.0, 0.0, 0.0, 0.0]);

        Assert.Equal(
            double.PositiveInfinity,
            HeadyZhuEconomicPartPeriodSolver
                .GetEconomicPartPeriodThreshold(problem));

        var token =
            TestContext.Current.CancellationToken;

        var expected =
            QuadraticWagnerWhitinOracle.GetOptimalCost(
                problem,
                token);

        var result =
            new HeadyZhuEconomicPartPeriodSolver().Solve(
                problem,
                token);

        AssertClose(
            expected,
            result.ObjectiveValue!.Value);
    }

    [Fact]
    public void VaryingSetupCosts_AreRejected()
    {
        var problem = new UlsProblem(
            [5.0, 5.0, 5.0],
            [20.0, 21.0, 20.0],
            [4.0, 4.0, 4.0],
            [2.0, 2.0, 2.0]);

        Assert.False(
            HeadyZhuEconomicPartPeriodSolver.IsApplicable(problem));

        Assert.Throws<NotSupportedException>(() =>
            new HeadyZhuEconomicPartPeriodSolver().Solve(
                problem,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public void VaryingProductionCosts_AreRejected()
    {
        var problem = new UlsProblem(
            [5.0, 5.0, 5.0],
            [20.0, 20.0, 20.0],
            [4.0, 5.0, 4.0],
            [2.0, 2.0, 2.0]);

        Assert.False(
            HeadyZhuEconomicPartPeriodSolver.IsApplicable(problem));
    }

    [Fact]
    public void VaryingRelevantHoldingCosts_AreRejected()
    {
        var problem = new UlsProblem(
            [5.0, 5.0, 5.0, 5.0],
            [20.0, 20.0, 20.0, 20.0],
            [4.0, 4.0, 4.0, 4.0],
            [2.0, 3.0, 2.0, 99.0]);

        Assert.False(
            HeadyZhuEconomicPartPeriodSolver.IsApplicable(problem));
    }

    [Fact]
    public void LastHoldingCost_IsIgnoredByApplicability()
    {
        var problem = new UlsProblem(
            [5.0, 5.0, 5.0, 5.0],
            [20.0, 20.0, 20.0, 20.0],
            [4.0, 4.0, 4.0, 4.0],
            [2.0, 2.0, 2.0, 999.0]);

        Assert.True(
            HeadyZhuEconomicPartPeriodSolver.IsApplicable(problem));
    }

    [Fact]
    public void RandomFixedCostInstances_MatchOracleEvansBahlTajAndLinearWagelmans()
    {
        var token =
            TestContext.Current.CancellationToken;

        var random =
            new Random(19940301);

        var headyZhu =
            new HeadyZhuEconomicPartPeriodSolver();

        var evans =
            new WagnerWhitinEvansSolver();

        var bahlTaj =
            new BahlTajPlanningHorizonSolver();

        var wagelmansLinear =
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

            var setupCost =
                random.Next(0, 201);

            var productionCost =
                random.Next(0, 31);

            var holdingCost =
                random.Next(0, 16);

            for (var period = 0;
                 period < horizon;
                 period++)
            {
                demands[period] =
                    random.NextDouble() < 0.20
                        ? 0.0
                        : random.Next(1, 51);

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

            Assert.True(
                HeadyZhuEconomicPartPeriodSolver.IsApplicable(problem));

            var expected =
                QuadraticWagnerWhitinOracle.GetOptimalCost(
                    problem,
                    token);

            var a =
                headyZhu
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var b =
                evans
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var c =
                bahlTaj
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var d =
                wagelmansLinear
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            AssertClose(
                expected,
                a,
                $"Heady-Zhu instance {instance}");

            AssertClose(
                expected,
                b,
                $"Evans instance {instance}");

            AssertClose(
                expected,
                c,
                $"Bahl-Taj instance {instance}");

            AssertClose(
                expected,
                d,
                $"Wagelmans instance {instance}");
        }
    }

    [Fact]
    public void HighDemandLowSetupInstances_ExerciseEconomicPartPeriodPruning()
    {
        var token =
            TestContext.Current.CancellationToken;

        var random =
            new Random(940301);

        var solver =
            new HeadyZhuEconomicPartPeriodSolver();

        const int instanceCount = 1_000;

        for (var instance = 0;
             instance < instanceCount;
             instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon =
                random.Next(10, 151);

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
                    random.Next(20, 101);

                setupCosts[period] = 10.0;
                productionCosts[period] = 2.0;
                holdingCosts[period] = 5.0;
            }

            var problem =
                new UlsProblem(
                    demands,
                    setupCosts,
                    productionCosts,
                    holdingCosts);

            var expected =
                new WagnerWhitinSolver()
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            var actual =
                solver
                    .Solve(problem, token)
                    .ObjectiveValue!.Value;

            AssertClose(
                expected,
                actual,
                $"pruning instance {instance}");
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

        using var cancellation =
            new CancellationTokenSource();

        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            new HeadyZhuEconomicPartPeriodSolver().Solve(
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
