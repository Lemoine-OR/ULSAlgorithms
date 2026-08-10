using ULSAlgorithms.Optimization.Execution;
using Xunit;

namespace ULSAlgorithms.Tests.Optimization.Execution;

public sealed class CplexXmlSolutionParserTests
{
    [Fact]
    public void Parser_UsesValueAttributeRatherThanVariableIndex()
    {
        string directory =
            Path.Combine(
                Path.GetTempPath(),
                "uls-cplex-sol-" +
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);

        string path =
            Path.Combine(
                directory,
                "solution.sol");

        try
        {
            File.WriteAllText(
                path,
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <CPLEXSolution version="1.2">
                  <header solutionStatusString="integer optimal solution"/>
                  <variables>
                    <variable name="v_0" index="0" value="7.5"/>
                    <variable name="v_1" index="1" value="1"/>
                  </variables>
                </CPLEXSolution>
                """);

            CplexXmlSolution result =
                CplexXmlSolutionParser.Parse(
                    path);

            Assert.Contains(
                "optimal",
                result.Status,
                StringComparison.OrdinalIgnoreCase);

            Assert.Equal(
                7.5,
                result.Values[0]);

            Assert.Equal(
                1.0,
                result.Values[1]);
        }
        finally
        {
            Directory.Delete(
                directory,
                recursive: true);
        }
    }
}
