using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Heuristics;

public sealed class ClassicalHeuristicsPackIITests
{
    [Fact]
    public void EveryPackIISolver_UsesHeuristicStrategyContract()
    {
        IUlsSolver[] solvers =
        [
            new FreelandColleySolver(),
            new PattersonLaForgeIncrementalPartPeriodSolver(),
            new WemmerlovModifiedPartPeriodBalancingSolver(),
            new WemmerlovPpbLookAheadLookBackSolver(),
            new WemmerlovModifiedPpbLookAheadLookBackSolver()
        ];

        var problem = CreatePositiveStationaryProblem(
            [20.0, 30.0, 25.0, 40.0, 15.0],
            setupCost: 100.0,
            holdingCost: 2.0);

        foreach (var solver in solvers)
        {
            Assert.Equal(
                UlsSolverKind.Heuristic,
                solver.Kind);

            var result = solver.Solve(
                problem,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                UlsSolveStatus.Feasible,
                result.Status);

            Assert.NotNull(result.Solution);
        }
    }

    [Fact]
    public void FreelandColley_UsesLocalIncrementalHoldingCriterion()
    {
        var problem = CreatePositiveStationaryProblem(
            [20.0, 80.0, 40.0],
            setupCost: 100.0,
            holdingCost: 1.0);

        var result = new FreelandColleySolver().Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [140.0, 0.0, 0.0],
            result.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void IncrementalPartPeriod_UsesCumulativeHoldingCriterion()
    {
        var problem = CreatePositiveStationaryProblem(
            [20.0, 80.0, 40.0],
            setupCost: 100.0,
            holdingCost: 1.0);

        var result =
            new PattersonLaForgeIncrementalPartPeriodSolver()
                .Solve(
                    problem,
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            [100.0, 0.0, 40.0],
            result.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void WemmerlovHalfPeriodCorrection_ShortensConstantDemandCycle()
    {
        var problem = CreatePositiveStationaryProblem(
            [20.0, 20.0, 20.0, 20.0, 20.0, 20.0],
            setupCost: 100.0,
            holdingCost: 1.0);

        var result =
            new WemmerlovModifiedPartPeriodBalancingSolver()
                .Solve(
                    problem,
                    TestContext.Current.CancellationToken);

        Assert.Equal(
            [60.0, 0.0, 0.0, 60.0, 0.0, 0.0],
            result.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void LookAhead_CanMoveNextReplenishmentForwardOnePeriod()
    {
        var problem = CreatePositiveStationaryProblem(
            [10.0, 90.0, 20.0, 80.0, 50.0],
            setupCost: 100.0,
            holdingCost: 1.0);

        var standard =
            new PartPeriodBalancingSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        var lalb =
            new WemmerlovPpbLookAheadLookBackSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            [100.0, 0.0, 100.0, 0.0, 50.0],
            standard.Solution!.ProductionQuantities.ToArray());

        Assert.Equal(
            [120.0, 0.0, 0.0, 130.0, 0.0],
            lalb.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void LookBack_CanMoveLastRequirementIntoNextLot()
    {
        var problem = CreatePositiveStationaryProblem(
            [10.0, 40.0, 30.0, 10.0],
            setupCost: 100.0,
            holdingCost: 1.0);

        var standard =
            new PartPeriodBalancingSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        var lalb =
            new WemmerlovPpbLookAheadLookBackSolver().Solve(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            [80.0, 0.0, 0.0, 10.0],
            standard.Solution!.ProductionQuantities.ToArray());

        Assert.Equal(
            [50.0, 0.0, 40.0, 0.0],
            lalb.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void LookAheadLookBack_RejectsZeroDemandPeriodsConservatively()
    {
        var problem = CreatePositiveStationaryProblem(
            [10.0, 0.0, 20.0],
            setupCost: 100.0,
            holdingCost: 1.0);

        Assert.False(
            WemmerlovPpbLookAheadLookBackSolver
                .IsApplicable(problem));

        Assert.False(
            WemmerlovModifiedPpbLookAheadLookBackSolver
                .IsApplicable(problem));

        Assert.Throws<NotSupportedException>(() =>
            new WemmerlovPpbLookAheadLookBackSolver()
                .Solve(
                    problem,
                    TestContext.Current.CancellationToken));
    }

    [Fact]
    public void FreelandAndIppa_SupportZeroDemandPeriods()
    {
        var problem = CreatePositiveStationaryProblem(
            [20.0, 0.0, 40.0, 0.0, 30.0],
            setupCost: 100.0,
            holdingCost: 1.0);

        IUlsSolver[] solvers =
        [
            new FreelandColleySolver(),
            new PattersonLaForgeIncrementalPartPeriodSolver(),
            new WemmerlovModifiedPartPeriodBalancingSolver()
        ];

        foreach (var solver in solvers)
        {
            var result = solver.Solve(
                problem,
                TestContext.Current.CancellationToken);

            Assert.Equal(
                UlsSolveStatus.Feasible,
                result.Status);

            AssertFeasible(
                problem,
                result.Solution!);
        }
    }

    [Fact]
    public void RandomPositiveStationaryInstances_AreFeasibleAndNeverBeatOptimum()
    {
        var token =
            TestContext.Current.CancellationToken;

        var random =
            new Random(19831101);

        IUlsSolver[] heuristics =
        [
            new FreelandColleySolver(),
            new PattersonLaForgeIncrementalPartPeriodSolver(),
            new WemmerlovModifiedPartPeriodBalancingSolver(),
            new WemmerlovPpbLookAheadLookBackSolver(),
            new WemmerlovModifiedPpbLookAheadLookBackSolver()
        ];

        var exact = new WagnerWhitinSolver();

        const int instanceCount = 3_000;

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

            var setupCost = random.Next(1, 201);
            var productionCost = random.Next(0, 21);
            var holdingCost = random.Next(1, 16);

            for (var period = 0;
                 period < horizon;
                 period++)
            {
                demands[period] = random.Next(1, 51);
                setupCosts[period] = setupCost;
                productionCosts[period] = productionCost;
                holdingCosts[period] = holdingCost;
            }

            var problem = new UlsProblem(
                demands,
                setupCosts,
                productionCosts,
                holdingCosts);

            var optimum =
                exact.Solve(
                    problem,
                    token)
                .ObjectiveValue!.Value;

            foreach (var heuristic in heuristics)
            {
                var result =
                    heuristic.Solve(
                        problem,
                        token);

                Assert.Equal(
                    UlsSolveStatus.Feasible,
                    result.Status);

                Assert.NotNull(result.Solution);

                AssertFeasible(
                    problem,
                    result.Solution);

                var tolerance =
                    1e-9 *
                    Math.Max(
                        1.0,
                        optimum);

                Assert.True(
                    result.Solution.TotalCost +
                    tolerance >= optimum,
                    $"{heuristic.Name}, instance {instance}: " +
                    $"heuristic={result.Solution.TotalCost:R}, " +
                    $"optimum={optimum:R}.");
            }
        }
    }

    [Fact]
    public void EveryPackIISolver_HonorsCancellation()
    {
        var problem = CreatePositiveStationaryProblem(
            [20.0, 30.0, 25.0, 40.0],
            setupCost: 100.0,
            holdingCost: 2.0);

        IUlsSolver[] solvers =
        [
            new FreelandColleySolver(),
            new PattersonLaForgeIncrementalPartPeriodSolver(),
            new WemmerlovModifiedPartPeriodBalancingSolver(),
            new WemmerlovPpbLookAheadLookBackSolver(),
            new WemmerlovModifiedPpbLookAheadLookBackSolver()
        ];

        using var cancellation =
            new CancellationTokenSource();

        cancellation.Cancel();

        foreach (var solver in solvers)
        {
            Assert.Throws<OperationCanceledException>(() =>
                solver.Solve(
                    problem,
                    cancellation.Token));
        }
    }

    private static UlsProblem CreatePositiveStationaryProblem(
        double[] demands,
        double setupCost,
        double holdingCost)
    {
        var horizon = demands.Length;

        return new UlsProblem(
            demands,
            Enumerable.Repeat(
                setupCost,
                horizon).ToArray(),
            new double[horizon],
            Enumerable.Repeat(
                holdingCost,
                horizon).ToArray());
    }

    private static void AssertFeasible(
        UlsProblem problem,
        UlsSolution solution)
    {
        var stock = 0.0;
        var demands = problem.Demands;
        var production = solution.ProductionQuantities;
        var inventories = solution.EndingInventories;

        var tolerance =
            1e-9 *
            Math.Max(
                1.0,
                problem.TotalDemand);

        for (var period = 0;
             period < problem.Horizon;
             period++)
        {
            stock +=
                production[period] -
                demands[period];

            Assert.True(
                stock >= -tolerance,
                $"Backlog at period {period}: {stock:R}.");

            Assert.True(
                Math.Abs(
                    stock -
                    inventories[period]) <= tolerance,
                $"Inventory mismatch at period {period}.");
        }

        Assert.True(
            Math.Abs(stock) <= tolerance,
            $"Terminal inventory is {stock:R}.");
    }
}
