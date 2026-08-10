using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Optimization.Modeling;
using Xunit;

namespace ULSAlgorithms.Tests.Optimization.Execution;

/// <summary>
/// Tests numerical cleanup of raw solver values before validation and mapping.
/// </summary>
public sealed class LinearVariableValueNormalizerTests
{
    [Fact]
    public void Id62StyleSmallNegativeContinuousResidual_IsNormalizedToZero()
    {
        LinearVariable variable =
            ContinuousVariable();

        var normalizer =
            new LinearVariableValueNormalizer();

        const double rawSolverValue =
            -4.999947122996673E-09;

        Assert.Equal(
            0.0,
            normalizer.Normalize(
                variable,
                rawSolverValue));
    }

    [Fact]
    public void MateriallyNegativeContinuousValue_IsPreserved()
    {
        LinearVariable variable =
            ContinuousVariable();

        var normalizer =
            new LinearVariableValueNormalizer();

        const double rawSolverValue =
            -1.0E-06;

        Assert.Equal(
            rawSolverValue,
            normalizer.Normalize(
                variable,
                rawSolverValue));
    }

    [Fact]
    public void NearIntegerContinuousValue_IsCleaned()
    {
        LinearVariable variable =
            ContinuousVariable();

        var normalizer =
            new LinearVariableValueNormalizer();

        Assert.Equal(
            180.0,
            normalizer.Normalize(
                variable,
                180.00000000000006));
    }

    [Fact]
    public void NearOneBinaryValue_IsNormalizedToOne()
    {
        var variable =
            new LinearVariable(
                0,
                "y",
                LinearVariableType.Binary,
                0.0,
                1.0);

        var normalizer =
            new LinearVariableValueNormalizer();

        Assert.Equal(
            1.0,
            normalizer.Normalize(
                variable,
                0.99999995));
    }

    [Fact]
    public void MateriallyFractionalBinaryValue_IsRejected()
    {
        var variable =
            new LinearVariable(
                0,
                "y",
                LinearVariableType.Binary,
                0.0,
                1.0);

        var normalizer =
            new LinearVariableValueNormalizer();

        Assert.Throws<InvalidOperationException>(
            () =>
                normalizer.Normalize(
                    variable,
                    0.75));
    }

    [Fact]
    public void NearIntegerIntegerValue_IsNormalized()
    {
        var variable =
            new LinearVariable(
                0,
                "n",
                LinearVariableType.Integer,
                0.0,
                100.0);

        var normalizer =
            new LinearVariableValueNormalizer();

        Assert.Equal(
            12.0,
            normalizer.Normalize(
                variable,
                12.00000005));
    }

    private static LinearVariable ContinuousVariable()
    {
        return new LinearVariable(
            0,
            "inventory",
            LinearVariableType.Continuous,
            0.0,
            double.PositiveInfinity);
    }
}
