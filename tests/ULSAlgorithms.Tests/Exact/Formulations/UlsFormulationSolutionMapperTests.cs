using ULSAlgorithms.Exact.Formulations.Internal;
using ULSAlgorithms.Formulations;
using ULSAlgorithms.Formulations.Aggregate;
using ULSAlgorithms.Formulations.FacilityLocation;
using ULSAlgorithms.Formulations.InventoryEliminated;
using ULSAlgorithms.Formulations.ShortestPath;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using ULSAlgorithms.Validation;
using Xunit;

namespace ULSAlgorithms.Tests.Exact.Formulations;

public sealed class UlsFormulationSolutionMapperTests
{
    [Theory]
    [InlineData(UlsFormulationKind.AggregateInventory)]
    [InlineData(UlsFormulationKind.FacilityLocation)]
    [InlineData(UlsFormulationKind.ShortestPath)]
    [InlineData(UlsFormulationKind.InventoryEliminated)]
    public void Mapper_ReconstructsSameProductionPlanForAllFormulations(
        UlsFormulationKind kind)
    {
        UlsProblem problem =
            CreateReferenceProblem();

        UlsFormulation formulation =
            Build(
                kind,
                problem);

        IReadOnlyDictionary<int, double> values =
            BuildAllInPeriodZeroValues(
                formulation,
                problem);

        UlsSolution solution =
            UlsFormulationSolutionMapper.Map(
                problem,
                formulation,
                values,
                zeroTolerance: 1.0e-8,
                feasibilityTolerance: 1.0e-7);

        Assert.Equal(
            [6.0, 0.0, 0.0],
            solution.ProductionQuantities.ToArray());

        Assert.Equal(
            [4.0, 2.0, 0.0],
            solution.EndingInventories.ToArray());

        Assert.Equal(
            [true, false, false],
            solution.SetupDecisions.ToArray());

        Assert.True(
            UlsSolutionValidator.Validate(
                problem,
                solution).IsFeasible);
    }

    [Fact]
    public void ShortestPathMapper_CanExtractAPathFromFractionalOptimalFlowSupport()
    {
        UlsProblem problem =
            CreateReferenceProblem();

        UlsFormulation formulation =
            new ShortestPathFormulationBuilder()
                .Build(problem);

        var values =
            formulation.Model.Variables
                .ToDictionary(
                    static variable =>
                        variable.Id,
                    static _ =>
                        0.0);

        // Two positive outgoing alternatives can occur in a fractional optimal
        // flow. The mapper follows a positive support path deterministically.
        values[
            formulation.Variables.Arcs[(0, 3)]] =
            0.6;

        values[
            formulation.Variables.Arcs[(0, 1)]] =
            0.4;

        values[
            formulation.Variables.Arcs[(1, 3)]] =
            0.4;

        UlsSolution solution =
            UlsFormulationSolutionMapper.Map(
                problem,
                formulation,
                values,
                zeroTolerance: 1.0e-8,
                feasibilityTolerance: 1.0e-7);

        Assert.Equal(
            [6.0, 0.0, 0.0],
            solution.ProductionQuantities.ToArray());
    }

    private static UlsProblem CreateReferenceProblem()
    {
        return new UlsProblem(
            [2.0, 2.0, 2.0],
            [10.0, 10.0, 10.0],
            [1.0, 1.0, 1.0],
            [1.0, 1.0, 0.0]);
    }

    private static UlsFormulation Build(
        UlsFormulationKind kind,
        UlsProblem problem)
    {
        return kind switch
        {
            UlsFormulationKind.AggregateInventory =>
                new AggregateInventoryFormulationBuilder()
                    .Build(problem),

            UlsFormulationKind.FacilityLocation =>
                new FacilityLocationFormulationBuilder()
                    .Build(problem),

            UlsFormulationKind.ShortestPath =>
                new ShortestPathFormulationBuilder()
                    .Build(problem),

            UlsFormulationKind.InventoryEliminated =>
                new InventoryEliminatedFormulationBuilder()
                    .Build(problem),

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(kind))
        };
    }

    private static IReadOnlyDictionary<int, double>
        BuildAllInPeriodZeroValues(
            UlsFormulation formulation,
            UlsProblem problem)
    {
        var values =
            formulation.Model.Variables
                .ToDictionary(
                    static variable =>
                        variable.Id,
                    static _ =>
                        0.0);

        switch (formulation.Kind)
        {
            case UlsFormulationKind.AggregateInventory:
                values[
                    formulation.Variables.Production[0]] =
                    6.0;

                values[
                    formulation.Variables.Setup[0]] =
                    1.0;

                values[
                    formulation.Variables.Inventory[0]] =
                    4.0;

                values[
                    formulation.Variables.Inventory[1]] =
                    2.0;
                break;

            case UlsFormulationKind.FacilityLocation:
                values[
                    formulation.Variables.Setup[0]] =
                    1.0;

                for (int demandPeriod = 0;
                     demandPeriod < problem.Horizon;
                     demandPeriod++)
                {
                    values[
                        formulation.Variables.Disaggregated[
                            (0, demandPeriod)]] =
                        problem.Demands[demandPeriod];
                }
                break;

            case UlsFormulationKind.ShortestPath:
                values[
                    formulation.Variables.Arcs[(0, 3)]] =
                    1.0;
                break;

            case UlsFormulationKind.InventoryEliminated:
                values[
                    formulation.Variables.Production[0]] =
                    6.0;

                values[
                    formulation.Variables.Setup[0]] =
                    1.0;
                break;
        }

        return values;
    }
}
