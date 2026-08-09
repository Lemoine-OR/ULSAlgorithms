using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.JacobsKhumawala;
using ULSAlgorithms.Exact.SaydamMcKnew;
using ULSAlgorithms.Exact.Wagelmans;
using ULSAlgorithms.Exact.Zangwill;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using ULSAlgorithms.Tests.Exact.WagnerWhitin;
using Xunit;

namespace ULSAlgorithms.Tests.Exact;

public sealed class ExactAlgorithmsPackIITests
{
    [Fact]
    public void EverySolver_UsesExactStrategyContract()
    {
        IUlsSolver[] solvers =
        [
            new SaydamMcKnewFastWagnerWhitinSolver(),
            new JacobsKhumawalaBranchAndBoundSolver(),
            new ZangwillNetworkSolver()
        ];

        var problem =
            CreateGeneralExample();

        foreach (var solver in solvers)
        {
            Assert.Equal(
                UlsSolverKind.Exact,
                solver.Kind);

            var result =
                solver.Solve(
                    problem,
                    TestContext.Current.CancellationToken);

            Assert.Equal(
                UlsSolveStatus.Optimal,
                result.Status);

            Assert.NotNull(
                result.Solution);
        }
    }

    [Fact]
    public void GeneralNonMonotoneExample_MatchesIndependentOracle()
    {
        var problem =
            CreateGeneralExample();

        var token =
            TestContext.Current.CancellationToken;

        var expected =
            QuadraticWagnerWhitinOracle.GetOptimalCost(
                problem,
                token);

        IUlsSolver[] solvers =
        [
            new SaydamMcKnewFastWagnerWhitinSolver(),
            new JacobsKhumawalaBranchAndBoundSolver(),
            new ZangwillNetworkSolver()
        ];

        foreach (var solver in solvers)
        {
            var actual =
                solver.Solve(
                    problem,
                    token)
                .ObjectiveValue!.Value;

            AssertClose(
                expected,
                actual,
                solver.Name);
        }
    }

    [Fact]
    public void ZeroDemandPeriods_AreHandledExactly()
    {
        var problem = new UlsProblem(
            [0.0, 10.0, 0.0, 8.0, 0.0, 4.0],
            [12.0, 30.0, 15.0, 22.0, 17.0, 19.0],
            [1.0, 8.0, 2.0, 7.0, 3.0, 6.0],
            [1.0, 2.0, 1.0, 3.0, 2.0, 0.0]);

        var token =
            TestContext.Current.CancellationToken;

        var expected =
            QuadraticWagnerWhitinOracle.GetOptimalCost(
                problem,
                token);

        IUlsSolver[] solvers =
        [
            new SaydamMcKnewFastWagnerWhitinSolver(),
            new JacobsKhumawalaBranchAndBoundSolver(),
            new ZangwillNetworkSolver()
        ];

        foreach (var solver in solvers)
        {
            var result =
                solver.Solve(
                    problem,
                    token);

            AssertClose(
                expected,
                result.ObjectiveValue!.Value,
                solver.Name);
        }
    }

    [Fact]
    public void AllZeroDemand_ReturnsZero()
    {
        var problem = new UlsProblem(
            [0.0, 0.0, 0.0, 0.0],
            [5.0, 7.0, 9.0, 11.0],
            [1.0, 2.0, 3.0, 4.0],
            [1.0, 2.0, 3.0, 0.0]);

        IUlsSolver[] solvers =
        [
            new SaydamMcKnewFastWagnerWhitinSolver(),
            new JacobsKhumawalaBranchAndBoundSolver(),
            new ZangwillNetworkSolver()
        ];

        foreach (var solver in solvers)
        {
            var result =
                solver.Solve(
                    problem,
                    TestContext.Current.CancellationToken);

            Assert.Equal(
                0.0,
                result.ObjectiveValue!.Value);

            Assert.DoesNotContain(
                true,
                result.Solution!.SetupDecisions.ToArray());
        }
    }

    [Fact]
    public void RandomGeneralInstances_MatchOracleAndWagelmansGeneral()
    {
        var token =
            TestContext.Current.CancellationToken;

        var random =
            new Random(19870301);

        IUlsSolver[] solvers =
        [
            new SaydamMcKnewFastWagnerWhitinSolver(),
            new JacobsKhumawalaBranchAndBoundSolver(),
            new ZangwillNetworkSolver()
        ];

        var wagelmans =
            new WagelmansGeneralSolver();

        const int instanceCount = 4_000;

        for (var instance = 0;
             instance < instanceCount;
             instance++)
        {
            token.ThrowIfCancellationRequested();

            var horizon =
                random.Next(1, 61);

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

                productionCosts[period] =
                    random.Next(0, 61);

                holdingCosts[period] =
                    random.Next(0, 16);
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

            foreach (var solver in solvers)
            {
                var actual =
                    solver.Solve(
                        problem,
                        token)
                    .ObjectiveValue!.Value;

                AssertClose(
                    expected,
                    actual,
                    $"{solver.Name}, instance {instance}");
            }

            var geometric =
                wagelmans.Solve(
                    problem,
                    token)
                .ObjectiveValue!.Value;

            AssertClose(
                expected,
                geometric,
                $"Wagelmans, instance {instance}");
        }
    }

    [Fact]
    public void EverySolver_HonorsCancellation()
    {
        var problem =
            CreateGeneralExample();

        IUlsSolver[] solvers =
        [
            new SaydamMcKnewFastWagnerWhitinSolver(),
            new JacobsKhumawalaBranchAndBoundSolver(),
            new ZangwillNetworkSolver()
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

    private static UlsProblem CreateGeneralExample()
    {
        return new UlsProblem(
            [6.0, 0.0, 8.0, 5.0, 9.0, 3.0],
            [18.0, 31.0, 12.0, 22.0, 15.0, 19.0],
            [20.0, 1.0, 18.0, 2.0, 16.0, 3.0],
            [1.0, 4.0, 1.0, 5.0, 2.0, 0.0]);
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
