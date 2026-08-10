using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Optimization.Modeling;
using Xunit;

namespace ULSAlgorithms.Tests.Optimization.Execution;

public sealed class PortableLpModelWriterTests
{
    [Fact]
    public void Writer_UsesStablePortableNamesAndAllSections()
    {
        var model =
            new LinearModel(
                "test",
                [
                    new LinearVariable(
                        0,
                        "x[0]",
                        LinearVariableType.Continuous,
                        0.0,
                        10.0),
                    new LinearVariable(
                        1,
                        "y[0]",
                        LinearVariableType.Binary,
                        0.0,
                        1.0)
                ],
                [
                    new LinearConstraint(
                        "link",
                        [
                            new LinearTerm(0, 1.0),
                            new LinearTerm(1, -10.0)
                        ],
                        LinearConstraintSense.LessOrEqual,
                        0.0)
                ],
                new LinearObjective(
                    [
                        new LinearTerm(0, 2.0),
                        new LinearTerm(1, 5.0)
                    ],
                    constant: 7.0));

        string directory =
            Path.Combine(
                Path.GetTempPath(),
                "uls-lp-" +
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);

        string path =
            Path.Combine(
                directory,
                "model.lp");

        try
        {
            new PortableLpModelWriter().Write(
                model,
                path);

            string text =
                File.ReadAllText(path);

            Assert.Contains(
                "Minimize",
                text,
                StringComparison.Ordinal);

            Assert.Contains(
                "v_0",
                text,
                StringComparison.Ordinal);

            Assert.Contains(
                "v_1",
                text,
                StringComparison.Ordinal);

            Assert.Contains(
                "Binaries",
                text,
                StringComparison.Ordinal);

            Assert.DoesNotContain(
                "x[0]",
                text,
                StringComparison.Ordinal);

            // Objective constant is intentionally reconstructed afterwards.
            Assert.DoesNotContain(
                " + 7",
                text,
                StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(
                directory,
                recursive: true);
        }
    }
}
