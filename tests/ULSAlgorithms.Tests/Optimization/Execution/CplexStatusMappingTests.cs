using System.Reflection;
using Xunit;
using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Optimization.Execution.Providers;

namespace ULSAlgorithms.Tests.Optimization.Execution;

public sealed class CplexStatusMappingTests
{
    [Theory]
    [InlineData("integer optimal solution", "", true, LinearModelSolveStatus.Optimal)]
    [InlineData("optimal", "", true, LinearModelSolveStatus.Optimal)]
    [InlineData("integer optimal, tolerance", "", true, LinearModelSolveStatus.Feasible)]
    [InlineData("integer optimal within tolerance", "", true, LinearModelSolveStatus.Feasible)]
    [InlineData(
        "integer optimal, tolerance",
        "MIP start rejected: infeasible; intermediate infeasibilities = 3",
        true,
        LinearModelSolveStatus.Feasible)]
    [InlineData(
        "integer optimal solution",
        "intermediate node was infeasible",
        true,
        LinearModelSolveStatus.Optimal)]
    public void NativeCplexStatus_HasPriorityOverConsoleText(
        string nativeStatus,
        string output,
        bool solutionExists,
        LinearModelSolveStatus expected)
    {
        MethodInfo method =
            typeof(CplexLinearModelExecutor)
                .GetMethod(
                    "MapStatus",
                    BindingFlags.NonPublic |
                    BindingFlags.Static)
            ?? throw new InvalidOperationException(
                "CplexLinearModelExecutor.MapStatus was not found.");

        object? raw =
            method.Invoke(
                null,
                [
                    nativeStatus,
                    output,
                    solutionExists,
                    0
                ]);

        Assert.Equal(
            expected,
            Assert.IsType<LinearModelSolveStatus>(raw));
    }
}
