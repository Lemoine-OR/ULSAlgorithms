using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.CuttingPlanes.Separation;
using ULSAlgorithms.Formulations.Aggregate;
using ULSAlgorithms.Models;
using Xunit;

namespace ULSAlgorithms.Tests.CuttingPlanes.Separation;

public sealed class WagnerWhitinLsCutSeparatorTests
{
    [Fact]
    public void Separator_GeneratesPrefixSFamily()
    {
        var problem =
            new UlsProblem(
                [2.0, 3.0, 4.0],
                [10.0, 10.0, 10.0],
                [1.0, 1.0, 1.0],
                [1.0, 1.0, 0.0]);

        var formulation =
            new AggregateInventoryFormulationBuilder()
                .Build(problem);

        var values =
            formulation.Model.Variables.ToDictionary(
                static variable =>
                    variable.Id,
                static _ =>
                    0.0);

        var separator =
            new WagnerWhitinLsCutSeparator();

        IReadOnlyList<LsSeparatedCut> cuts =
            separator.Separate(
                problem,
                formulation,
                values);

        Assert.Equal(
            6,
            cuts.Count);

        Assert.Contains(
            cuts,
            cut =>
                cut.Definition.L == 2 &&
                cut.Definition.S.SequenceEqual(
                    [0, 1]));

        Assert.Equal(
            CutSeparationMethod.WagnerWhitin,
            separator.Method);
    }

    [Fact]
    public void Separator_RejectsSpeculativeCostStructure()
    {
        var problem =
            new UlsProblem(
                [1.0, 1.0],
                [10.0, 10.0],
                [1.0, 10.0],
                [1.0, 0.0]);

        var separator =
            new WagnerWhitinLsCutSeparator();

        Assert.False(
            separator.IsApplicable(problem));
    }
}
