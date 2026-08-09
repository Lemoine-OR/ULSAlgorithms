using ULSAlgorithms.Models;
using Xunit;

namespace ULSAlgorithms.Tests.Models;

public sealed class UlsProblemTests
{
    [Fact]
    public void Constructor_StoresValidatedVectorsAndTotalDemand()
    {
        var problem = new UlsProblem(
            [10.0, 20.0, 5.0],
            [100.0, 110.0, 120.0],
            [2.0, 2.5, 3.0],
            [1.0, 1.5, 0.0]);

        Assert.Equal(3, problem.Horizon);
        Assert.Equal(35.0, problem.TotalDemand);
        Assert.Equal([10.0, 20.0, 5.0], problem.Demands.ToArray());
        Assert.Equal([100.0, 110.0, 120.0], problem.SetupCosts.ToArray());
        Assert.Equal([2.0, 2.5, 3.0], problem.UnitProductionCosts.ToArray());
        Assert.Equal([1.0, 1.5, 0.0], problem.HoldingCosts.ToArray());
    }

    [Fact]
    public void Constructor_DefensivelyCopiesInputVectors()
    {
        double[] demands = [10.0, 20.0];
        double[] setupCosts = [100.0, 200.0];
        double[] productionCosts = [1.0, 2.0];
        double[] holdingCosts = [0.5, 0.0];

        var problem = new UlsProblem(
            demands,
            setupCosts,
            productionCosts,
            holdingCosts);

        demands[0] = 999.0;
        setupCosts[0] = 999.0;
        productionCosts[0] = 999.0;
        holdingCosts[0] = 999.0;

        Assert.Equal(10.0, problem.Demands[0]);
        Assert.Equal(100.0, problem.SetupCosts[0]);
        Assert.Equal(1.0, problem.UnitProductionCosts[0]);
        Assert.Equal(0.5, problem.HoldingCosts[0]);
    }

    [Fact]
    public void Constructor_RejectsEmptyHorizon()
    {
        Assert.Throws<ArgumentException>(() =>
            new UlsProblem([], [], [], []));
    }

    [Fact]
    public void Constructor_RejectsMismatchedVectorLengths()
    {
        Assert.Throws<ArgumentException>(() =>
            new UlsProblem(
                [1.0, 2.0],
                [10.0],
                [1.0, 1.0],
                [0.0, 0.0]));
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_RejectsInvalidDemand(double invalidValue)
    {
        Assert.Throws<ArgumentException>(() =>
            new UlsProblem(
                [1.0, invalidValue],
                [10.0, 10.0],
                [1.0, 1.0],
                [0.5, 0.0]));
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidCost(double invalidValue)
    {
        Assert.Throws<ArgumentException>(() =>
            new UlsProblem(
                [1.0, 1.0],
                [10.0, invalidValue],
                [1.0, 1.0],
                [0.5, 0.0]));
    }

    [Fact]
    public void Constructor_AllowsZeroDemandAndZeroCosts()
    {
        var problem = new UlsProblem(
            [0.0, 0.0],
            [0.0, 0.0],
            [0.0, 0.0],
            [0.0, 0.0]);

        Assert.Equal(0.0, problem.TotalDemand);
    }
}
