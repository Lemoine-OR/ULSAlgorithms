using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.CuttingPlanes.Separation;
using ULSAlgorithms.Formulations.Aggregate;
using ULSAlgorithms.Models;
using Xunit;

namespace ULSAlgorithms.Tests.CuttingPlanes.Separation;

public sealed class GeneralLsCutSeparatorTests
{
    [Fact]
    public void Separator_FindsMostViolatedSubsetForEachL()
    {
        var problem =
            new UlsProblem(
                [2.0, 3.0],
                [10.0, 10.0],
                [0.0, 0.0],
                [1.0, 0.0]);

        var formulation =
            new AggregateInventoryFormulationBuilder()
                .Build(problem);

        var values =
            formulation.Model.Variables.ToDictionary(
                static variable =>
                    variable.Id,
                static _ =>
                    0.0);

        values[
            formulation.Variables.Production[0]] =
            2.0;

        values[
            formulation.Variables.Production[1]] =
            3.0;

        values[
            formulation.Variables.Setup[0]] =
            0.4;

        values[
            formulation.Variables.Setup[1]] =
            1.0;

        var separator =
            new GeneralLsCutSeparator();

        IReadOnlyList<LsSeparatedCut> cuts =
            separator.Separate(
                problem,
                formulation,
                values);

        LsSeparatedCut l0 =
            Assert.Single(
                cuts,
                cut =>
                    cut.Definition.L == 0);

        Assert.Equal(
            1.2,
            l0.Violation,
            precision: 10);

        Assert.Empty(
            l0.Definition.S);

        Assert.Equal(
            CutSeparationMethod.General,
            separator.Method);
    }

    [Fact]
    public void Separator_UsesProductionTermWhenItIsCheaperThanSetupTerm()
    {
        var problem =
            new UlsProblem(
                [2.0, 3.0],
                [10.0, 10.0],
                [0.0, 0.0],
                [1.0, 0.0]);

        var formulation =
            new AggregateInventoryFormulationBuilder()
                .Build(problem);

        var values =
            formulation.Model.Variables.ToDictionary(
                static variable =>
                    variable.Id,
                static _ =>
                    0.0);

        values[
            formulation.Variables.Production[0]] =
            1.0;

        values[
            formulation.Variables.Setup[0]] =
            1.0;

        values[
            formulation.Variables.Production[1]] =
            3.0;

        values[
            formulation.Variables.Setup[1]] =
            1.0;

        var separator =
            new GeneralLsCutSeparator();

        LsSeparatedCut l1 =
            separator
                .Separate(
                    problem,
                    formulation,
                    values)
                .Single(
                    cut =>
                        cut.Definition.L == 1);

        Assert.Contains(
            0,
            l1.Definition.S);
    }
}
