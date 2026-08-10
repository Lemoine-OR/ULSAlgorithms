using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.CuttingPlanes.Internal;
using ULSAlgorithms.Formulations.Aggregate;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization.Modeling;
using Xunit;

namespace ULSAlgorithms.Tests.CuttingPlanes.Internal;

public sealed class LsCutModelBuilderTests
{
    [Fact]
    public void Relaxation_ConvertsBinarySetupsToContinuous()
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

        LinearModel relaxation =
            LsCutModelBuilder.CreateLpRelaxation(
                formulation.Model);

        foreach (int setupId in
                 formulation.Variables.Setup.Values)
        {
            Assert.Equal(
                LinearVariableType.Continuous,
                relaxation.GetVariable(
                    setupId).Type);
        }

        Assert.False(
            relaxation.IsMixedInteger);
    }

    [Fact]
    public void AddCuts_PreservesVariableIdentifiers()
    {
        var problem =
            new UlsProblem(
                [2.0],
                [10.0],
                [0.0],
                [0.0]);

        var formulation =
            new AggregateInventoryFormulationBuilder()
                .Build(problem);

        string yName =
            formulation.Model.GetVariable(
                formulation.Variables.Setup[0]).Name;

        var cut =
            new LsCutDefinition(
                0,
                [],
                [new CutCoefficient(yName, 2.0)],
                ULSAlgorithms.CuttingPlanes.LinearConstraintSense.GreaterOrEqual,
                2.0);

        LinearModel strengthened =
            LsCutModelBuilder.AddCuts(
                formulation.Model,
                [("ls_test", cut)]);

        Assert.Equal(
            formulation.Model.ConstraintCount + 1,
            strengthened.ConstraintCount);

        Assert.Equal(
            formulation.Model.GetVariable(
                formulation.Variables.Setup[0]).Id,
            strengthened.GetVariable(
                formulation.Variables.Setup[0]).Id);
    }
}
