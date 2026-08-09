using ULSAlgorithms.Results;
using Xunit;

namespace ULSAlgorithms.Tests.Results;

public sealed class UlsSolutionTests
{
    [Fact]
    public void Constructor_ComputesTotalCost()
    {
        var solution = new UlsSolution(
            [10.0, 0.0, 5.0],
            [2.0, 0.0, 0.0],
            [true, false, true],
            setupCost: 150.0,
            productionCost: 35.0,
            holdingCost: 4.0);

        Assert.Equal(3, solution.Horizon);
        Assert.Equal(150.0, solution.SetupCost);
        Assert.Equal(35.0, solution.ProductionCost);
        Assert.Equal(4.0, solution.HoldingCost);
        Assert.Equal(189.0, solution.TotalCost);
    }

    [Fact]
    public void Constructor_DefensivelyCopiesDecisionVectors()
    {
        double[] production = [10.0, 0.0];
        double[] inventory = [5.0, 0.0];
        bool[] setup = [true, false];

        var solution = new UlsSolution(
            production,
            inventory,
            setup,
            setupCost: 10.0,
            productionCost: 20.0,
            holdingCost: 5.0);

        production[0] = 999.0;
        inventory[0] = 999.0;
        setup[0] = false;

        Assert.Equal(10.0, solution.ProductionQuantities[0]);
        Assert.Equal(5.0, solution.EndingInventories[0]);
        Assert.True(solution.SetupDecisions[0]);
    }

    [Fact]
    public void Constructor_RejectsMismatchedDecisionLengths()
    {
        Assert.Throws<ArgumentException>(() =>
            new UlsSolution(
                [10.0, 0.0],
                [0.0],
                [true, false],
                10.0,
                20.0,
                0.0));
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidCost(double invalidCost)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new UlsSolution(
                [1.0],
                [0.0],
                [true],
                invalidCost,
                0.0,
                0.0));
    }
}
