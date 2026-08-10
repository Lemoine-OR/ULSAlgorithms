using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.FedergruenTzur;
using ULSAlgorithms.Exact.Wagelmans;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using ULSAlgorithms.Selection;
using Xunit;

namespace ULSAlgorithms.Tests.Selection;

public sealed class AdaptiveExactUlsSolverTests
{
    [Fact]
    public void Solver_ImplementsCommonExactStrategyContract()
    {
        IUlsSolver solver = new AdaptiveExactUlsSolver();

        Assert.Equal(UlsSolverKind.Exact, solver.Kind);
        Assert.Equal("Adaptive exact ULS solver", solver.Name);
    }

    [Fact]
    public void Analyzer_DetectsNoSpeculativeMotiveInstance()
    {
        var problem = new UlsProblem(
            [10.0, 20.0, 15.0, 12.0],
            [100.0, 100.0, 100.0, 100.0],
            [8.0, 9.0, 9.5, 10.0],
            [2.0, 1.0, 1.0, 0.0]);

        var characteristics = UlsProblemAnalyzer.Analyze(problem);

        Assert.True(characteristics.HasNoSpeculativeMotiveCosts);
        Assert.Equal(4, characteristics.PositiveDemandPeriods);
        Assert.Equal(1.0, characteristics.DemandDensity);
        Assert.True(characteristics.HasConstantSetupCosts);
        Assert.False(characteristics.HasConstantUnitProductionCosts);
    }

    [Fact]
    public void Analyzer_DetectsSpeculativeMotiveInstance()
    {
        var problem = CreateGeneralProblem();

        var characteristics = UlsProblemAnalyzer.Analyze(problem);

        Assert.False(characteristics.HasNoSpeculativeMotiveCosts);
    }

    [Fact]
    public void SelectSolver_UsesLinearSolverWhenApplicable()
    {
        var problem = new UlsProblem(
            [5.0, 6.0, 7.0],
            [20.0, 20.0, 20.0],
            [0.0, 0.0, 0.0],
            [1.0, 1.0, 0.0]);

        var selector = new AdaptiveExactUlsSolver();

        Assert.IsType<WagnerWhitinSolver>(selector.SelectSolver(problem));
    }

    [Fact]
    public void SelectSolver_DefaultsToWagelmansForGeneralCosts()
    {
        var selector = new AdaptiveExactUlsSolver();

        Assert.IsType<WagelmansGeneralSolver>(
            selector.SelectSolver(CreateGeneralProblem()));
    }

    [Fact]
    public void SelectSolver_CanUseFedergruenTzurFallback()
    {
        var selector = new AdaptiveExactUlsSolver(
            UlsGeneralExactFallback.FedergruenTzurGeneral);

        Assert.IsType<FedergruenTzurSolver>(
            selector.SelectSolver(CreateGeneralProblem()));
    }

    [Fact]
    public void SelectSolver_WithCharacteristics_RejectsMismatchedProfile()
    {
        var first = CreateGeneralProblem();
        var second = new UlsProblem(
            [1.0, 2.0],
            [3.0, 3.0],
            [0.0, 0.0],
            [1.0, 0.0]);

        var characteristics = UlsProblemAnalyzer.Analyze(first);
        var selector = new AdaptiveExactUlsSolver();

        Assert.Throws<ArgumentException>(() =>
            selector.SelectSolver(second, in characteristics));
    }

    [Fact]
    public void Solve_GeneralCaseMatchesDirectWagelmansResult()
    {
        var problem = CreateGeneralProblem();
        var selector = new AdaptiveExactUlsSolver();
        var reference = new WagelmansGeneralSolver();

        var selectedResult = selector.Solve(
            problem,
            TestContext.Current.CancellationToken);
        var referenceResult = reference.Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.Equal(UlsSolveStatus.Optimal, selectedResult.Status);
        Assert.Equal(UlsSolveStatus.Optimal, referenceResult.Status);
        Assert.NotNull(selectedResult.ObjectiveValue);
        Assert.NotNull(referenceResult.ObjectiveValue);
        Assert.Equal(
            referenceResult.ObjectiveValue!.Value,
            selectedResult.ObjectiveValue!.Value,
            precision: 10);
    }

    [Fact]
    public void Solve_HonorsCancellation()
    {
        var selector = new AdaptiveExactUlsSolver();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            selector.Solve(CreateGeneralProblem(), cancellation.Token));
    }

    private static UlsProblem CreateGeneralProblem()
    {
        // p[0] + h[0] < p[1], therefore the WW/NSM specialization
        // is intentionally not applicable.
        return new UlsProblem(
            [8.0, 12.0, 5.0, 20.0, 9.0],
            [50.0, 35.0, 60.0, 30.0, 45.0],
            [1.0, 8.0, 2.0, 7.0, 3.0],
            [1.0, 1.5, 0.5, 1.0, 0.0]);
    }
}
