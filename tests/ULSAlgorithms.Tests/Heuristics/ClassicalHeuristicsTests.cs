using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Heuristics;

public sealed class ClassicalHeuristicsTests
{
    [Fact]
    public void EveryHeuristic_UsesCommonStrategyContractAndReturnsFeasible()
    {
        var problem = CreateFivePeriodExample();

        IUlsSolver[] solvers =
        [
            new LotForLotSolver(),
            new SilverMealSolver(),
            new LeastUnitCostSolver(),
            new PartPeriodBalancingSolver(),
            new GroffSolver(),
            new PeriodicOrderQuantitySolver()
        ];

        foreach (var solver in solvers)
        {
            Assert.Equal(UlsSolverKind.Heuristic, solver.Kind);

            var result = solver.Solve(
                problem,
                TestContext.Current.CancellationToken);

            Assert.Equal(UlsSolveStatus.Feasible, result.Status);
            Assert.NotNull(result.Solution);
            Assert.Equal(problem.Horizon, result.Solution.Horizon);
        }
    }

    [Fact]
    public void LotForLot_ProducesExactlyEachPeriodsDemand()
    {
        var problem = new UlsProblem(
            [5.0, 0.0, 7.0, 3.0],
            [10.0, 20.0, 30.0, 40.0],
            [1.0, 5.0, 2.0, 6.0],
            [2.0, 3.0, 4.0, 0.0]);

        var result = new LotForLotSolver().Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [5.0, 0.0, 7.0, 3.0],
            result.Solution!.ProductionQuantities.ToArray());

        Assert.Equal(
            [true, false, true, true],
            result.Solution.SetupDecisions.ToArray());

        Assert.All(
            result.Solution.EndingInventories.ToArray(),
            inventory => Assert.Equal(0.0, inventory));
    }

    [Fact]
    public void SilverMeal_FivePeriodExample_UsesCyclesTwoAndThreePeriods()
    {
        var result = new SilverMealSolver().Solve(
            CreateFivePeriodExample(),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [50.0, 0.0, 80.0, 0.0, 0.0],
            result.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void LeastUnitCost_FivePeriodExample_UsesExpectedCycles()
    {
        var result = new LeastUnitCostSolver().Solve(
            CreateFivePeriodExample(),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [50.0, 0.0, 65.0, 0.0, 15.0],
            result.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void PartPeriodBalancing_FivePeriodExample_UsesExpectedCycles()
    {
        var problem = CreateFivePeriodExample();

        Assert.Equal(
            50.0,
            PartPeriodBalancingSolver.GetEconomicPartPeriod(problem));

        var result = new PartPeriodBalancingSolver().Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [50.0, 0.0, 65.0, 0.0, 15.0],
            result.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void Groff_ReproducesPublishedStyleExample()
    {
        var result = new GroffSolver().Solve(
            CreateFivePeriodExample(),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [50.0, 0.0, 80.0, 0.0, 0.0],
            result.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void PeriodicOrderQuantity_FivePeriodExample_UsesTwoPeriodInterval()
    {
        var problem = CreateFivePeriodExample();

        Assert.Equal(
            2,
            PeriodicOrderQuantitySolver.GetOrderInterval(problem));

        var result = new PeriodicOrderQuantitySolver().Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [50.0, 0.0, 65.0, 0.0, 15.0],
            result.Solution!.ProductionQuantities.ToArray());
    }

    [Fact]
    public void AllZeroDemand_ReturnsZeroCostWithoutSetups()
    {
        var problem = new UlsProblem(
            [0.0, 0.0, 0.0, 0.0],
            [25.0, 25.0, 25.0, 25.0],
            [7.0, 7.0, 7.0, 7.0],
            [2.0, 2.0, 2.0, 2.0]);

        IUlsSolver[] solvers =
        [
            new LotForLotSolver(),
            new SilverMealSolver(),
            new LeastUnitCostSolver(),
            new PartPeriodBalancingSolver(),
            new GroffSolver(),
            new PeriodicOrderQuantitySolver()
        ];

        foreach (var solver in solvers)
        {
            var result = solver.Solve(
                problem,
                TestContext.Current.CancellationToken);

            Assert.Equal(UlsSolveStatus.Feasible, result.Status);
            Assert.NotNull(result.Solution);
            Assert.Equal(0.0, result.Solution.TotalCost);

            Assert.DoesNotContain(
                true,
                result.Solution.SetupDecisions.ToArray());
        }
    }

    [Fact]
    public void StationaryHeuristics_RejectTimeVaryingSetupCosts()
    {
        var problem = new UlsProblem(
            [5.0, 6.0, 7.0],
            [10.0, 11.0, 10.0],
            [2.0, 2.0, 2.0],
            [1.0, 1.0, 0.0]);

        IUlsSolver[] stationary =
        [
            new SilverMealSolver(),
            new LeastUnitCostSolver(),
            new PartPeriodBalancingSolver(),
            new GroffSolver(),
            new PeriodicOrderQuantitySolver()
        ];

        foreach (var solver in stationary)
        {
            Assert.Throws<NotSupportedException>(() =>
                solver.Solve(
                    problem,
                    TestContext.Current.CancellationToken));
        }

        // L4L is valid for the general cost model.
        var l4l = new LotForLotSolver().Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.Equal(UlsSolveStatus.Feasible, l4l.Status);
    }

    [Fact]
    public void RandomFixedCostInstances_AreFeasibleAndNeverBeatExactOptimum()
    {
        var token = TestContext.Current.CancellationToken;
        var random = new Random(19730101);

        IUlsSolver[] heuristics =
        [
            new LotForLotSolver(),
            new SilverMealSolver(),
            new LeastUnitCostSolver(),
            new PartPeriodBalancingSolver(),
            new GroffSolver(),
            new PeriodicOrderQuantitySolver()
        ];

        var exact = new WagnerWhitinSolver();

        const int instanceCount = 2_500;

        for (var instance = 0; instance < instanceCount; instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon = random.Next(1, 101);
            var demands = new double[horizon];
            var setupCosts = new double[horizon];
            var productionCosts = new double[horizon];
            var holdingCosts = new double[horizon];

            var setupCost = random.Next(0, 201);
            var productionCost = random.Next(0, 21);
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

            var optimum =
                exact.Solve(problem, token).ObjectiveValue!.Value;

            foreach (var heuristic in heuristics)
            {
                var result = heuristic.Solve(problem, token);

                Assert.Equal(UlsSolveStatus.Feasible, result.Status);
                Assert.NotNull(result.Solution);

                AssertFeasible(problem, result.Solution);

                var tolerance =
                    1e-9 * Math.Max(1.0, optimum);

                Assert.True(
                    result.Solution.TotalCost + tolerance >= optimum,
                    $"{heuristic.Name}, instance {instance}: " +
                    $"heuristic={result.Solution.TotalCost:R}, " +
                    $"optimum={optimum:R}.");
            }
        }
    }

    [Fact]
    public void EveryHeuristic_HonorsCancellation()
    {
        var problem = CreateFivePeriodExample();

        IUlsSolver[] solvers =
        [
            new LotForLotSolver(),
            new SilverMealSolver(),
            new LeastUnitCostSolver(),
            new PartPeriodBalancingSolver(),
            new GroffSolver(),
            new PeriodicOrderQuantitySolver()
        ];

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        foreach (var solver in solvers)
        {
            Assert.Throws<OperationCanceledException>(() =>
                solver.Solve(problem, cancellation.Token));
        }
    }

    private static UlsProblem CreateFivePeriodExample()
    {
        return new UlsProblem(
            [20.0, 30.0, 25.0, 40.0, 15.0],
            [200.0, 200.0, 200.0, 200.0, 200.0],
            [0.0, 0.0, 0.0, 0.0, 0.0],
            [4.0, 4.0, 4.0, 4.0, 4.0]);
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
            1e-9 * Math.Max(1.0, problem.TotalDemand);

        for (var period = 0; period < problem.Horizon; period++)
        {
            stock += production[period] - demands[period];

            Assert.True(
                stock >= -tolerance,
                $"Backlog at period {period}: {stock:R}.");

            Assert.True(
                Math.Abs(stock - inventories[period]) <= tolerance,
                $"Inventory mismatch at period {period}.");
        }

        Assert.True(
            Math.Abs(stock) <= tolerance,
            $"Terminal inventory is {stock:R}.");
    }
}
