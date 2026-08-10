using ULSAlgorithms.Optimization.Modeling;
using Xunit;

namespace ULSAlgorithms.Tests.Optimization;

public sealed class LinearModelTests
{
    [Fact]
    public void LinearModel_RejectsUnknownConstraintVariable()
    {
        var variable =
            new LinearVariable(
                0,
                "x",
                LinearVariableType.Continuous,
                0.0,
                1.0);

        var constraint =
            new LinearConstraint(
                "bad",
                [new LinearTerm(1, 1.0)],
                LinearConstraintSense.LessOrEqual,
                1.0);

        Assert.Throws<ArgumentException>(
            () =>
                new LinearModel(
                    "test",
                    [variable],
                    [constraint],
                    new LinearObjective([])));
    }

    [Fact]
    public void LinearModel_DetectsMixedIntegerModels()
    {
        var continuous =
            new LinearModel(
                "lp",
                [
                    new LinearVariable(
                        0,
                        "x",
                        LinearVariableType.Continuous,
                        0.0,
                        double.PositiveInfinity)
                ],
                [],
                new LinearObjective([]));

        var mixed =
            new LinearModel(
                "mip",
                [
                    new LinearVariable(
                        0,
                        "y",
                        LinearVariableType.Binary,
                        0.0,
                        1.0)
                ],
                [],
                new LinearObjective([]));

        Assert.False(continuous.IsMixedInteger);
        Assert.True(mixed.IsMixedInteger);
    }
}
