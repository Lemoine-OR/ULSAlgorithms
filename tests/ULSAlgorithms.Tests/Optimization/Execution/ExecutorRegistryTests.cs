using ULSAlgorithms.Optimization.Execution;
using Xunit;

namespace ULSAlgorithms.Tests.Optimization.Execution;

public sealed class ExecutorRegistryTests
{
    [Fact]
    public void DefaultRegistry_ContainsFourExecutionBackends()
    {
        LinearModelExecutorRegistry registry =
            DefaultLinearModelExecutorRegistry.Create();

        Assert.Equal(
            4,
            registry.Count);

        Assert.Equal(
            SolverKind.Cplex,
            registry.GetRequired(
                SolverKind.Cplex).SolverKind);

        Assert.Equal(
            SolverKind.Gurobi,
            registry.GetRequired(
                SolverKind.Gurobi).SolverKind);

        Assert.Equal(
            SolverKind.Xpress,
            registry.GetRequired(
                SolverKind.Xpress).SolverKind);

        Assert.Equal(
            SolverKind.CoinOrCbc,
            registry.GetRequired(
                SolverKind.CoinOrCbc).SolverKind);
    }
}
