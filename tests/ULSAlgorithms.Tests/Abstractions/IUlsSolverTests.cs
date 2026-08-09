using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Abstractions;

public sealed class IUlsSolverTests
{
    [Fact]
    public void StrategyContract_AllowsInterchangeableSolver()
    {
        IUlsSolver solver = new StubSolver();

        var problem = new UlsProblem(
            [10.0],
            [5.0],
            [1.0],
            [0.0]);

        var result = solver.Solve(
            problem,
            TestContext.Current.CancellationToken);

        Assert.Equal("Stub exact solver", solver.Name);
        Assert.Equal(UlsSolverKind.Exact, solver.Kind);
        Assert.Equal(UlsSolveStatus.Optimal, result.Status);
        Assert.Equal(15.0, result.ObjectiveValue);
    }

    private sealed class StubSolver : IUlsSolver
    {
        public string Name => "Stub exact solver";

        public UlsSolverKind Kind => UlsSolverKind.Exact;

        public UlsSolveResult Solve(
            UlsProblem problem,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(problem);
            cancellationToken.ThrowIfCancellationRequested();

            var solution = new UlsSolution(
                problem.Demands,
                new double[problem.Horizon],
                CreateSetupDecisions(problem),
                setupCost: problem.SetupCosts[0],
                productionCost: problem.Demands[0] * problem.UnitProductionCosts[0],
                holdingCost: 0.0);

            return new UlsSolveResult(
                Name,
                UlsSolveStatus.Optimal,
                solution);
        }

        private static bool[] CreateSetupDecisions(UlsProblem problem)
        {
            var setup = new bool[problem.Horizon];
            if (problem.Demands[0] > 0.0)
            {
                setup[0] = true;
            }

            return setup;
        }
    }
}
