using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Optimization.Execution.Providers;
using Xunit;

namespace ULSAlgorithms.Tests.Optimization.Execution;

public sealed class CoinOrCbcLinearModelExecutorStatusTests
{
    [Fact]
    public void OptimalSolutionHeader_WinsOverIntermediateInfeasibleDiagnostics()
    {
        const string output = """
            Presolve 8 (-4) rows, 11 (-5) columns and 24 (-8) elements
            Primal infeasible - objective value 640
            Result - Optimal solution found
            Objective value: 680.00000000
            """;

        const string solutionHeader =
            "Optimal - objective value 680.00000000";

        Assert.Equal(
            LinearModelSolveStatus.Optimal,
            CoinOrCbcLinearModelExecutor.MapStatus(
                output,
                solutionHeader,
                solutionExists: true,
                exitCode: 0));
    }

    [Fact]
    public void InfeasibleSolutionHeader_IsAuthoritative()
    {
        Assert.Equal(
            LinearModelSolveStatus.Infeasible,
            CoinOrCbcLinearModelExecutor.MapStatus(
                "CBC terminated normally.",
                "Infeasible - objective value 0",
                solutionExists: false,
                exitCode: 0));
    }

    [Fact]
    public void ExistingCandidateWithoutOptimalTerminalStatus_IsFeasible()
    {
        Assert.Equal(
            LinearModelSolveStatus.Feasible,
            CoinOrCbcLinearModelExecutor.MapStatus(
                "Stopped on time limit",
                "Stopped on time - objective value 700",
                solutionExists: true,
                exitCode: 0));
    }

    [Fact]
    public void UnboundedSolutionHeader_IsAuthoritative()
    {
        Assert.Equal(
            LinearModelSolveStatus.Unbounded,
            CoinOrCbcLinearModelExecutor.MapStatus(
                "CBC terminated normally.",
                "Unbounded - objective value 0",
                solutionExists: false,
                exitCode: 0));
    }
}