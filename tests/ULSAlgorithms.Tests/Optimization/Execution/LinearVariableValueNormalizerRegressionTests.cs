using Xunit;
using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Tests.Optimization.Execution;

public sealed class LinearVariableValueNormalizerRegressionTests
{
    private static readonly LinearVariable Binary =
        new(
            0,
            "y",
            LinearVariableType.Binary,
            0.0,
            1.0);

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(7.3354959499897174E-08, 0.0)]
    [InlineData(8.5441582786671688E-07, 0.0)]
    [InlineData(0.99999984687336652, 1.0)]
    [InlineData(0.99999954930027901, 1.0)]
    public void DefaultBinaryNormalization_AcceptsCplexScaleResiduals(
        double raw,
        double expected)
    {
        var normalizer =
            new LinearVariableValueNormalizer();

        Assert.Equal(
            expected,
            normalizer.Normalize(
                Binary,
                raw));
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(0.999)]
    [InlineData(0.5)]
    public void MateriallyFractionalBinaryValues_AreStillRejected(
        double raw)
    {
        var normalizer =
            new LinearVariableValueNormalizer();

        Assert.Throws<InvalidOperationException>(
            () => normalizer.Normalize(
                Binary,
                raw));
    }
}
