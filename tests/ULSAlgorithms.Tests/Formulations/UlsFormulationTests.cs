using ULSAlgorithms.Formulations;
using ULSAlgorithms.Formulations.Aggregate;
using ULSAlgorithms.Formulations.FacilityLocation;
using ULSAlgorithms.Formulations.InventoryEliminated;
using ULSAlgorithms.Formulations.ShortestPath;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization.Modeling;
using Xunit;

namespace ULSAlgorithms.Tests.Formulations;

public sealed class UlsFormulationTests
{
    [Fact]
    public void Catalog_ContainsFourClassicalFormulations()
    {
        IReadOnlyList<IUlsFormulationBuilder> builders =
            UlsFormulationCatalog.CreateAll();

        Assert.Equal(4, builders.Count);

        Assert.Equal(
            [
                UlsFormulationKind.AggregateInventory,
                UlsFormulationKind.FacilityLocation,
                UlsFormulationKind.ShortestPath,
                UlsFormulationKind.InventoryEliminated
            ],
            builders.Select(
                static builder =>
                    builder.Kind));
    }

    [Fact]
    public void Aggregate_UsesTightSuffixDemandBigM()
    {
        UlsProblem problem =
            CreateReferenceProblem();

        UlsFormulation formulation =
            new AggregateInventoryFormulationBuilder()
                .Build(problem);

        Assert.Equal(
            3 * problem.Horizon,
            formulation.Model.VariableCount);

        for (int period = 0;
             period < problem.Horizon;
             period++)
        {
            int xId =
                formulation.Variables.Production[period];

            LinearVariable x =
                formulation.Model.GetVariable(xId);

            double expected =
                problem.Demands
                    .Slice(period)
                    .ToArray()
                    .Sum();

            Assert.Equal(
                expected,
                x.UpperBound);
        }

        int lastInventory =
            formulation.Variables.Inventory[
                problem.Horizon - 1];

        Assert.Equal(
            0.0,
            formulation.Model
                .GetVariable(lastInventory)
                .UpperBound);
    }

    [Fact]
    public void FacilityLocation_BuildsOnlyPositiveDemandAssignments()
    {
        var problem =
            new UlsProblem(
                [0.0, 3.0, 0.0, 2.0],
                [5.0, 6.0, 7.0, 8.0],
                [1.0, 1.0, 1.0, 1.0],
                [2.0, 2.0, 2.0, 0.0]);

        UlsFormulation formulation =
            new FacilityLocationFormulationBuilder()
                .Build(problem);

        // Demand 1: production periods 0..1 => 2 variables.
        // Demand 3: production periods 0..3 => 4 variables.
        Assert.Equal(
            6,
            formulation.Variables.Disaggregated.Count);

        Assert.Equal(
            problem.Horizon,
            formulation.Variables.Setup.Count);

        Assert.DoesNotContain(
            formulation.Variables.Disaggregated.Keys,
            key =>
                key.Second == 0 ||
                key.Second == 2);
    }

    [Fact]
    public void FacilityLocation_DeliveredCostIncludesHolding()
    {
        var problem =
            new UlsProblem(
                [1.0, 1.0, 1.0],
                [10.0, 10.0, 10.0],
                [3.0, 5.0, 7.0],
                [2.0, 4.0, 0.0]);

        UlsFormulation formulation =
            new FacilityLocationFormulationBuilder()
                .Build(problem);

        int q02 =
            formulation.Variables.Disaggregated[(0, 2)];

        double coefficient =
            formulation.Model.Objective.Terms
                .Single(
                    term =>
                        term.VariableId == q02)
                .Coefficient;

        Assert.Equal(
            3.0 + 2.0 + 4.0,
            coefficient);
    }

    [Fact]
    public void ShortestPath_RejectsSpeculativeMotiveCosts()
    {
        var problem =
            new UlsProblem(
                [1.0, 1.0],
                [5.0, 5.0],
                [1.0, 10.0],
                [1.0, 0.0]);

        var builder =
            new ShortestPathFormulationBuilder();

        Assert.False(
            builder.IsApplicable(problem));

        Assert.Throws<NotSupportedException>(
            () =>
                builder.Build(problem));
    }

    [Fact]
    public void ShortestPath_AddsZeroDemandSkipArc()
    {
        var problem =
            new UlsProblem(
                [0.0, 2.0, 1.0],
                [5.0, 6.0, 7.0],
                [1.0, 1.0, 1.0],
                [2.0, 2.0, 0.0]);

        UlsFormulation formulation =
            new ShortestPathFormulationBuilder()
                .Build(problem);

        Assert.True(
            formulation.Variables.Arcs.ContainsKey(
                (0, 1)));

        int skip =
            formulation.Variables.Arcs[(0, 1)];

        Assert.Equal(
            LinearVariableType.Continuous,
            formulation.Model
                .GetVariable(skip)
                .Type);

        Assert.Equal(
            0.0,
            GetObjectiveCoefficient(
                formulation,
                skip));
    }

    [Fact]
    public void InventoryEliminated_ContainsNoInventoryVariables()
    {
        UlsProblem problem =
            CreateReferenceProblem();

        UlsFormulation formulation =
            new InventoryEliminatedFormulationBuilder()
                .Build(problem);

        Assert.Empty(
            formulation.Variables.Inventory);

        Assert.Equal(
            2 * problem.Horizon,
            formulation.Model.VariableCount);

        Assert.Equal(
            2 * problem.Horizon,
            formulation.Model.ConstraintCount);
    }

    [Fact]
    public void InventoryEliminated_ObjectiveMatchesReconstructedInventoryCost()
    {
        var problem =
            new UlsProblem(
                [2.0, 3.0],
                [5.0, 7.0],
                [1.0, 4.0],
                [2.0, 0.0]);

        UlsFormulation formulation =
            new InventoryEliminatedFormulationBuilder()
                .Build(problem);

        // x0 coefficient = p0 + h0 = 3
        // x1 coefficient = p1 = 4
        // constant = -h0 * d0 = -4
        Assert.Equal(
            3.0,
            GetObjectiveCoefficient(
                formulation,
                formulation.Variables.Production[0]));

        Assert.Equal(
            4.0,
            GetObjectiveCoefficient(
                formulation,
                formulation.Variables.Production[1]));

        Assert.Equal(
            -4.0,
            formulation.Model.Objective.Constant);
    }

    private static UlsProblem CreateReferenceProblem()
    {
        return new UlsProblem(
            [2.0, 3.0, 4.0, 5.0],
            [10.0, 11.0, 12.0, 13.0],
            [1.0, 1.0, 1.0, 1.0],
            [2.0, 2.0, 2.0, 0.0]);
    }

    private static double GetObjectiveCoefficient(
        UlsFormulation formulation,
        int variableId)
    {
        return formulation.Model.Objective.Terms
            .Where(
                term =>
                    term.VariableId == variableId)
            .Select(
                static term =>
                    term.Coefficient)
            .SingleOrDefault();
    }
}
