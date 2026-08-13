using ULSAlgorithms.Exact.Formulations;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization;
using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Optimization.Modeling;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.Formulations;

public sealed class FixedIntegerPolishingTests
{
    [Fact]
    public async Task RejectedMipCandidate_IsRecoveredWithoutUpgradingOptimalityProof()
    {
        var executor = new RecoverableCandidateExecutor();
        LinearModelSolver modelSolver = CreateModelSolver(executor);

        var solver =
            new InventoryEliminatedFormulationSolver(
                modelSolver,
                new LinearModelSolveOptions
                {
                    Solver = SolverKind.CoinOrCbc,
                    EnableFixedIntegerPolishing = true
                });

        UlsProblem problem =
            new(
                [1.0, 1.0],
                [10.0, 10.0],
                [0.0, 0.0],
                [1.0, 0.0]);

        UlsSolveResult result =
            await solver.SolveAsync(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(2, executor.CallCount);
        Assert.Equal(UlsSolveStatus.Feasible, result.Status);
        Assert.NotNull(result.Solution);
        Assert.Equal(2.0, result.Solution!.ProductionQuantities[0], 10);
        Assert.Equal(0.0, result.Solution.ProductionQuantities[1], 10);

        var solverBacked =
            Assert.IsType<SolverBackedUlsSolveResult>(result);

        Assert.True(solverBacked.ModelExecution.HasFeasibleSolution);
        Assert.Equal(
            LinearModelSolveStatus.Feasible,
            solverBacked.ModelExecution.SolverReportedStatus);

        Assert.Contains(
            solverBacked.ModelExecution.Diagnostics,
            message =>
                message.Contains(
                    "Fixed-integer polishing recovered",
                    StringComparison.Ordinal));
    }

    [Fact]
    public async Task PolishingCanBeDisabled()
    {
        var executor = new RecoverableCandidateExecutor();
        LinearModelSolver modelSolver = CreateModelSolver(executor);

        var solver =
            new InventoryEliminatedFormulationSolver(
                modelSolver,
                new LinearModelSolveOptions
                {
                    Solver = SolverKind.CoinOrCbc,
                    EnableFixedIntegerPolishing = false
                });

        UlsProblem problem =
            new(
                [1.0, 1.0],
                [10.0, 10.0],
                [0.0, 0.0],
                [1.0, 0.0]);

        UlsSolveResult result =
            await solver.SolveAsync(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(1, executor.CallCount);
        Assert.Equal(UlsSolveStatus.Failed, result.Status);
        Assert.Null(result.Solution);
    }

    private static LinearModelSolver CreateModelSolver(
        RecoverableCandidateExecutor executor)
    {
        var adapters = new SolverAdapterRegistry();
        adapters.Register(new FakeAdapter());

        var executors = new LinearModelExecutorRegistry();
        executors.Register(executor);

        return new LinearModelSolver(
            adapters,
            executors,
            new SolverSelectionService());
    }

    private sealed class FakeAdapter :
        IOptimizationSolverAdapter
    {
        public string AdapterId => "test.cbc";
        public string AdapterName => "Test CBC";
        public string AdapterVersion => "1.0";
        public SolverKind SolverKind => SolverKind.CoinOrCbc;

        public IReadOnlyCollection<SolverCapability> Capabilities =>
        [
            SolverCapability.LinearProgramming,
            SolverCapability.MixedIntegerLinearProgramming
        ];

        public bool SupportsCapability(
            SolverCapability capability) =>
            Capabilities.Contains(capability);

        public ValueTask<SolverAvailabilityInfo> CheckAvailabilityAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return ValueTask.FromResult(
                new SolverAvailabilityInfo(
                    SolverKind.CoinOrCbc,
                    SolverAvailabilityStatus.Available,
                    solverName: "Fake CBC",
                    solverVersion: "test"));
        }
    }

    private sealed class RecoverableCandidateExecutor :
        ILinearModelSolverExecutor
    {
        public int CallCount { get; private set; }

        public SolverKind SolverKind =>
            SolverKind.CoinOrCbc;

        public ValueTask<LinearModelSolveResult> SolveAsync(
            LinearModel model,
            SolverSelectionResult selection,
            LinearModelSolveOptions options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;

            var values =
                model.Variables.ToDictionary(
                    variable => variable.Id,
                    variable =>
                        variable.LowerBound ==
                        variable.UpperBound
                            ? variable.LowerBound
                            : 0.0);

            if (model.IsMixedInteger)
            {
                Set(values, model, "x[0]", 1.99997);
                Set(values, model, "x[1]", 0.00003);
                Set(values, model, "y[0]", 1.0);
                Set(values, model, "y[1]", 0.0);

                LinearModelSolutionValidation invalid =
                    LinearModelSolutionValidator.Validate(
                        model,
                        values,
                        options.FeasibilityTolerance,
                        options.IntegralityTolerance);

                Assert.False(invalid.IsFeasible);

                return ValueTask.FromResult(
                    new LinearModelSolveResult(
                        model.Name,
                        LinearModelSolveStatus.Feasible,
                        new SolverExecutionInfo(selection),
                        values,
                        invalid,
                        TimeSpan.FromMilliseconds(1),
                        "integer optimal, tolerance"));
            }

            Set(values, model, "x[0]", 2.0);
            Set(values, model, "x[1]", 0.0);

            LinearModelSolutionValidation valid =
                LinearModelSolutionValidator.Validate(
                    model,
                    values,
                    options.FeasibilityTolerance,
                    options.IntegralityTolerance);

            Assert.True(valid.IsFeasible);

            return ValueTask.FromResult(
                new LinearModelSolveResult(
                    model.Name,
                    LinearModelSolveStatus.Optimal,
                    new SolverExecutionInfo(selection),
                    values,
                    valid,
                    TimeSpan.FromMilliseconds(1),
                    "optimal"));
        }

        private static void Set(
            IDictionary<int, double> values,
            LinearModel model,
            string name,
            double value)
        {
            LinearVariable variable =
                model.Variables.Single(v => v.Name == name);

            values[variable.Id] = value;
        }
    }
}
