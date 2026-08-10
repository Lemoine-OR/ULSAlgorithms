using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using ULSAlgorithms.Validation;
using Xunit;

namespace ULSAlgorithms.Tests.Validation;

public sealed class UlsSolutionValidatorTests
{
    [Fact]
    public void Validator_AcceptsConsistentPlan()
    {
        var problem =
            new UlsProblem(
                [2.0, 2.0, 2.0],
                [10.0, 10.0, 10.0],
                [1.0, 1.0, 1.0],
                [1.0, 1.0, 0.0]);

        var solution =
            new UlsSolution(
                [6.0, 0.0, 0.0],
                [4.0, 2.0, 0.0],
                [true, false, false],
                setupCost: 10.0,
                productionCost: 6.0,
                holdingCost: 6.0);

        UlsSolutionValidationResult validation =
            UlsSolutionValidator.Validate(
                problem,
                solution);

        Assert.True(
            validation.IsFeasible);

        Assert.Equal(
            22.0,
            validation.RecomputedTotalCost);
    }

    [Fact]
    public void Validator_RejectsProductionWithoutSetup()
    {
        var problem =
            new UlsProblem(
                [2.0],
                [10.0],
                [1.0],
                [0.0]);

        var solution =
            new UlsSolution(
                [2.0],
                [0.0],
                [false],
                setupCost: 0.0,
                productionCost: 2.0,
                holdingCost: 0.0);

        UlsSolutionValidationResult validation =
            UlsSolutionValidator.Validate(
                problem,
                solution);

        Assert.False(
            validation.IsFeasible);

        Assert.Equal(
            1,
            validation.SetupLinkViolations);
    }
}
