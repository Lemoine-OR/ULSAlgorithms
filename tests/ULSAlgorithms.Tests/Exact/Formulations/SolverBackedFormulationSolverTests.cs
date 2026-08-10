using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.Formulations;
using ULSAlgorithms.Formulations;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization;
using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Optimization.Modeling;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.Formulations;

public sealed class SolverBackedFormulationSolverTests
{
    [Fact]
    public void FourFormulationStrategies_ImplementCommonExactContract()
    {
        IUlsSolver[] solvers =
        [
            new AggregateInventoryFormulationSolver(),
            new FacilityLocationFormulationSolver(),
            new ShortestPathFormulationSolver(),
            new InventoryEliminatedFormulationSolver()
        ];

        Assert.All(
            solvers,
            solver =>
                Assert.Equal(
                    UlsSolverKind.Exact,
                    solver.Kind));

        Assert.All(
            solvers,
            solver =>
                Assert.IsAssignableFrom<IAsyncUlsSolver>(
                    solver));
    }

    [Fact]
    public async Task AggregateStrategy_ReturnsSolverBackedProvenanceWithInjectedExecutor()
    {
        UlsProblem problem =
            new(
                [2.0, 2.0, 2.0],
                [10.0, 10.0, 10.0],
                [1.0, 1.0, 1.0],
                [1.0, 1.0, 0.0]);

        LinearModelSolver modelSolver =
            CreateFakeModelSolver();

        var strategy =
            new AggregateInventoryFormulationSolver(
                modelSolver,
                new LinearModelSolveOptions
                {
                    Solver =
                        SolverKind.CoinOrCbc
                });

        UlsSolveResult result =
            await strategy.SolveAsync(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            UlsSolveStatus.Optimal,
            result.Status);

        var solverBacked =
            Assert.IsType<SolverBackedUlsSolveResult>(
                result);

        Assert.Equal(
            UlsFormulationKind.AggregateInventory,
            solverBacked.FormulationKind);

        Assert.Equal(
            SolverKind.CoinOrCbc,
            solverBacked.OptimizationSolver?.SelectedSolver);

        Assert.Equal(
            22.0,
            result.ObjectiveValue);
    }

    private static LinearModelSolver CreateFakeModelSolver()
    {
        var adapters =
            new SolverAdapterRegistry();

        adapters.Register(
            new FakeAdapter());

        var executors =
            new LinearModelExecutorRegistry();

        executors.Register(
            new FakeExecutor());

        return new LinearModelSolver(
            adapters,
            executors,
            new SolverSelectionService());
    }

    private sealed class FakeAdapter :
        IOptimizationSolverAdapter
    {
        public string AdapterId =>
            "test.cbc";

        public string AdapterName =>
            "Test CBC";

        public string AdapterVersion =>
            "1.0";

        public SolverKind SolverKind =>
            SolverKind.CoinOrCbc;

        public IReadOnlyCollection<SolverCapability> Capabilities =>
        [
            SolverCapability.LinearProgramming,
            SolverCapability.MixedIntegerLinearProgramming
        ];

        public bool SupportsCapability(
            SolverCapability capability) =>
            Capabilities.Contains(
                capability);

        public ValueTask<SolverAvailabilityInfo>
            CheckAvailabilityAsync(
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

    private sealed class FakeExecutor :
        ILinearModelSolverExecutor
    {
        public SolverKind SolverKind =>
            SolverKind.CoinOrCbc;

        public ValueTask<LinearModelSolveResult> SolveAsync(
            LinearModel model,
            SolverSelectionResult selection,
            LinearModelSolveOptions options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var values =
                model.Variables
                    .ToDictionary(
                        static variable =>
                            variable.Id,
                        static _ =>
                            0.0);

            foreach (LinearVariable variable in model.Variables)
            {
                switch (variable.Name)
                {
                    case "x[0]":
                        values[variable.Id] =
                            6.0;
                        break;

                    case "I[0]":
                        values[variable.Id] =
                            4.0;
                        break;

                    case "I[1]":
                        values[variable.Id] =
                            2.0;
                        break;

                    case "y[0]":
                        values[variable.Id] =
                            1.0;
                        break;
                }
            }

            LinearModelSolutionValidation validation =
                LinearModelSolutionValidator.Validate(
                    model,
                    values,
                    options.FeasibilityTolerance,
                    options.IntegralityTolerance);

            var result =
                new LinearModelSolveResult(
                    model.Name,
                    LinearModelSolveStatus.Optimal,
                    new SolverExecutionInfo(
                        selection),
                    values,
                    validation,
                    TimeSpan.Zero,
                    "fake optimal");

            return ValueTask.FromResult(
                result);
        }
    }
}
