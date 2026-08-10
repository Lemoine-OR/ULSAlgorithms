using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Optimization.Modeling;
using Xunit;

namespace ULSAlgorithms.Tests.Optimization.Execution;

public sealed class LinearModelSolutionValidatorTests
{
    [Fact]
    public void Validator_AcceptsFeasibleIntegerSolutionAndRecomputesObjective()
    {
        LinearModel model =
            BuildModel();

        LinearModelSolutionValidation validation =
            LinearModelSolutionValidator.Validate(
                model,
                new Dictionary<int, double>
                {
                    [0] = 4.0,
                    [1] = 1.0
                },
                feasibilityTolerance: 1.0e-8,
                integralityTolerance: 1.0e-8);

        Assert.True(
            validation.IsFeasible);

        Assert.Equal(
            18.0,
            validation.ObjectiveValue);

        Assert.Equal(
            0.0,
            validation.MaximumConstraintViolation);
    }

    [Fact]
    public void Validator_RejectsConstraintAndIntegralityViolations()
    {
        LinearModel model =
            BuildModel();

        LinearModelSolutionValidation validation =
            LinearModelSolutionValidator.Validate(
                model,
                new Dictionary<int, double>
                {
                    [0] = 11.0,
                    [1] = 0.4
                },
                feasibilityTolerance: 1.0e-8,
                integralityTolerance: 1.0e-8);

        Assert.False(
            validation.IsFeasible);

        Assert.True(
            validation.MaximumConstraintViolation > 0.0);

        Assert.True(
            validation.MaximumIntegralityViolation > 0.0);
    }

    private static LinearModel BuildModel()
    {
        return new LinearModel(
            "validation",
            [
                new LinearVariable(
                    0,
                    "x",
                    LinearVariableType.Continuous,
                    0.0,
                    10.0),
                new LinearVariable(
                    1,
                    "y",
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
                constant: 5.0));
    }
}
