using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// Independently checks solver-returned values against the portable model.
/// </summary>
public static class LinearModelSolutionValidator
{
    /// <summary>
    /// Validates bounds, integrality, constraints and objective reconstruction.
    /// </summary>
    public static LinearModelSolutionValidation Validate(
        LinearModel model,
        IReadOnlyDictionary<int, double> values,
        double feasibilityTolerance,
        double integralityTolerance)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(values);

        if (!double.IsFinite(feasibilityTolerance) ||
            feasibilityTolerance <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(feasibilityTolerance));
        }

        if (!double.IsFinite(integralityTolerance) ||
            integralityTolerance <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(integralityTolerance));
        }

        var diagnostics =
            new List<string>();

        double maxBoundViolation = 0.0;
        double maxIntegralityViolation = 0.0;
        double maxConstraintViolation = 0.0;

        foreach (LinearVariable variable in model.Variables)
        {
            if (!values.TryGetValue(
                    variable.Id,
                    out double value) ||
                !double.IsFinite(value))
            {
                diagnostics.Add(
                    $"Variable '{variable.Name}' has no finite returned value.");

                maxBoundViolation =
                    double.PositiveInfinity;

                continue;
            }

            double lowerViolation =
                Math.Max(
                    0.0,
                    variable.LowerBound - value);

            double upperViolation =
                double.IsPositiveInfinity(variable.UpperBound)
                    ? 0.0
                    : Math.Max(
                        0.0,
                        value - variable.UpperBound);

            maxBoundViolation =
                Math.Max(
                    maxBoundViolation,
                    Math.Max(
                        lowerViolation,
                        upperViolation));

            if (variable.Type != LinearVariableType.Continuous)
            {
                double integerViolation =
                    Math.Abs(
                        value -
                        Math.Round(
                            value,
                            MidpointRounding.AwayFromZero));

                maxIntegralityViolation =
                    Math.Max(
                        maxIntegralityViolation,
                        integerViolation);
            }
        }

        foreach (LinearConstraint constraint in model.Constraints)
        {
            double activity =
                EvaluateTerms(
                    constraint.Terms,
                    values);

            double violation =
                constraint.Sense switch
                {
                    LinearConstraintSense.LessOrEqual =>
                        Math.Max(
                            0.0,
                            activity - constraint.RightHandSide),

                    LinearConstraintSense.Equal =>
                        Math.Abs(
                            activity - constraint.RightHandSide),

                    LinearConstraintSense.GreaterOrEqual =>
                        Math.Max(
                            0.0,
                            constraint.RightHandSide - activity),

                    _ =>
                        throw new NotSupportedException(
                            $"Unsupported constraint sense '{constraint.Sense}'.")
                };

            maxConstraintViolation =
                Math.Max(
                    maxConstraintViolation,
                    violation);
        }

        double objective =
            model.Objective.Constant +
            EvaluateTerms(
                model.Objective.Terms,
                values);

        bool feasible =
            double.IsFinite(objective) &&
            maxBoundViolation <= feasibilityTolerance &&
            maxConstraintViolation <= feasibilityTolerance &&
            maxIntegralityViolation <= integralityTolerance;

        if (maxBoundViolation > feasibilityTolerance)
        {
            diagnostics.Add(
                $"Maximum bound violation is {maxBoundViolation:R}.");
        }

        if (maxConstraintViolation > feasibilityTolerance)
        {
            diagnostics.Add(
                $"Maximum constraint violation is " +
                $"{maxConstraintViolation:R}.");
        }

        if (maxIntegralityViolation > integralityTolerance)
        {
            diagnostics.Add(
                $"Maximum integrality violation is " +
                $"{maxIntegralityViolation:R}.");
        }

        if (!double.IsFinite(objective))
        {
            diagnostics.Add(
                "The independently reconstructed objective is not finite.");
        }

        return new LinearModelSolutionValidation(
            feasible,
            objective,
            maxBoundViolation,
            maxIntegralityViolation,
            maxConstraintViolation,
            diagnostics);
    }

    private static double EvaluateTerms(
        IEnumerable<LinearTerm> terms,
        IReadOnlyDictionary<int, double> values)
    {
        double result = 0.0;

        foreach (LinearTerm term in terms)
        {
            if (!values.TryGetValue(
                    term.VariableId,
                    out double value) ||
                !double.IsFinite(value))
            {
                return double.NaN;
            }

            result +=
                term.Coefficient *
                value;
        }

        return result;
    }
}
