using ULSAlgorithms.Abstractions;
using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.Exact.CuttingPlanes;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization;
using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Optimization.Modeling;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.CuttingPlanes;

public sealed class LsCuttingPlaneSolverTests
{
    [Fact]
    public void CuttingPlaneStrategies_ImplementCommonExactContract()
    {
        IUlsSolver[] solvers =
        [
            new GeneralLsCuttingPlaneSolver(),
            new WagnerWhitinLsCuttingPlaneSolver()
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
    public async Task GeneralSolver_RecordsGeneratedAndAddedCuts()
    {
        var problem =
            new UlsProblem(
                [2.0, 2.0],
                [10.0, 10.0],
                [0.0, 0.0],
                [1.0, 0.0]);

        LinearModelSolver modelSolver =
            CreateFakeModelSolver();

        var solver =
            new GeneralLsCuttingPlaneSolver(
                modelSolver,
                new LinearModelSolveOptions
                {
                    Solver =
                        SolverKind.CoinOrCbc
                });

        UlsSolveResult result =
            await solver.SolveAsync(
                problem,
                TestContext.Current.CancellationToken);

        Assert.Equal(
            UlsSolveStatus.Optimal,
            result.Status);

        var cutResult =
            Assert.IsType<CuttingPlaneUlsSolveResult>(
                result);

        Assert.True(
            cutResult.CuttingPlaneExecution
                .Cuts.CutsGenerated > 0);

        Assert.True(
            cutResult.CuttingPlaneExecution
                .Cuts.CutsAdded > 0);

        Assert.All(
            cutResult.CuttingPlaneExecution
                .Cuts.AddedCuts,
            cut =>
            {
                Assert.True(cut.WasAdded);
                Assert.NotEmpty(
                    cut.SolverConstraintName);
                Assert.NotEmpty(
                    cut.Definition.Coefficients);
            });

        Assert.Equal(
            20.0,
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
                model.Variables.ToDictionary(
                    static variable =>
                        variable.Id,
                    static _ =>
                        0.0);

            bool hasLsCut =
                model.Constraints.Any(
                    constraint =>
                        constraint.Name.StartsWith(
                            "ls_",
                            StringComparison.Ordinal));

            if (!model.IsMixedInteger &&
                !hasLsCut)
            {
                Set(
                    model,
                    values,
                    "x[0]",
                    2.0);

                Set(
                    model,
                    values,
                    "x[1]",
                    2.0);

                Set(
                    model,
                    values,
                    "y[0]",
                    0.5);

                Set(
                    model,
                    values,
                    "y[1]",
                    1.0);
            }
            else
            {
                Set(
                    model,
                    values,
                    "x[0]",
                    2.0);

                Set(
                    model,
                    values,
                    "x[1]",
                    2.0);

                Set(
                    model,
                    values,
                    "y[0]",
                    1.0);

                Set(
                    model,
                    values,
                    "y[1]",
                    1.0);
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

        private static void Set(
            LinearModel model,
            IDictionary<int, double> values,
            string name,
            double value)
        {
            LinearVariable variable =
                model.Variables.Single(
                    candidate =>
                        candidate.Name == name);

            values[variable.Id] =
                value;
        }
    }
}
