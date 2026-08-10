using ULSAlgorithms.Optimization.Execution;
using Xunit;

namespace ULSAlgorithms.Tests.Optimization.Execution;

public sealed class NamedSolutionValueParserTests
{
    [Fact]
    public void Parser_HandlesGurobiAndCbcStyleLines()
    {
        IReadOnlyDictionary<int, double> values =
            NamedSolutionValueParser.ParseLines(
            [
                "v_0 12.5",
                "  1 v_1 1",
                "v_2 -3.25e-2"
            ]);

        Assert.Equal(
            12.5,
            values[0]);

        Assert.Equal(
            1.0,
            values[1]);

        Assert.Equal(
            -3.25e-2,
            values[2]);
    }
}
