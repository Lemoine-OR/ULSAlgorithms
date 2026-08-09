using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Results;

public sealed class UlsSolveResultTests
{
    private static UlsSolution CreateSolution()
    {
        return new UlsSolution(
            [10.0],
            [0.0],
            [true],
            setupCost: 5.0,
            productionCost: 10.0,
            holdingCost: 0.0);
    }

    [Fact]
    public void OptimalResult_RequiresAndExposesSolution()
    {
        var solution = CreateSolution();

        var result = new UlsSolveResult(
            "Reference solver",
            UlsSolveStatus.Optimal,
            solution);

        Assert.True(result.HasSolution);
        Assert.Same(solution, result.Solution);
        Assert.Equal(15.0, result.ObjectiveValue);
    }

    [Fact]
    public void FeasibleResult_RequiresSolution()
    {
        Assert.Throws<ArgumentException>(() =>
            new UlsSolveResult(
                "Heuristic",
                UlsSolveStatus.Feasible));
    }

    [Fact]
    public void InfeasibleResult_RejectsSolution()
    {
        Assert.Throws<ArgumentException>(() =>
            new UlsSolveResult(
                "Exact solver",
                UlsSolveStatus.Infeasible,
                CreateSolution()));
    }

    [Fact]
    public void LimitReached_MayContainIncumbent()
    {
        var solution = CreateSolution();

        var result = new UlsSolveResult(
            "Solver",
            UlsSolveStatus.LimitReached,
            solution,
            "Time limit reached.");

        Assert.True(result.HasSolution);
        Assert.Equal(15.0, result.ObjectiveValue);
        Assert.Equal("Time limit reached.", result.Message);
    }

    [Fact]
    public void FailedResult_HasNoObjectiveValue()
    {
        var result = new UlsSolveResult(
            "Solver",
            UlsSolveStatus.Failed,
            message: "Failure.");

        Assert.False(result.HasSolution);
        Assert.Null(result.ObjectiveValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsBlankSolverName(string solverName)
    {
        Assert.Throws<ArgumentException>(() =>
            new UlsSolveResult(
                solverName,
                UlsSolveStatus.NotSolved));
    }
}
