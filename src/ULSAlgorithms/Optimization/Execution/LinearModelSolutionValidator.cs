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
    /// <remarks>
    /// Constraint feasibility uses a mixed absolute/relative policy. The
    /// configured feasibility tolerance is multiplied by the scale of each
    /// row, where the scale is the maximum of 1, the absolute right-hand side,
    /// and the absolute sum of evaluated row terms. This keeps small rows
    /// protected by the absolute tolerance while avoiding false rejection of
    /// numerically valid solver output on larger rows.
    /// </remarks>
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
        double maxConstraintNormalizedViolation = 0.0;

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

            double rowScale =
                EvaluateConstraintScale(
                    constraint,
                    values);

            double normalizedViolation =
                double.IsFinite(violation) &&
                double.IsFinite(rowScale) &&
                rowScale > 0.0
                    ? violation / rowScale
                    : double.PositiveInfinity;

            maxConstraintViolation =
                Math.Max(
                    maxConstraintViolation,
                    violation);

            maxConstraintNormalizedViolation =
                Math.Max(
                    maxConstraintNormalizedViolation,
                    normalizedViolation);
        }

        double objective =
            model.Objective.Constant +
            EvaluateTerms(
                model.Objective.Terms,
                values);

        bool feasible =
            double.IsFinite(objective) &&
            maxBoundViolation <= feasibilityTolerance &&
            maxConstraintNormalizedViolation <= feasibilityTolerance &&
            maxIntegralityViolation <= integralityTolerance;

        if (maxBoundViolation > feasibilityTolerance)
        {
            diagnostics.Add(
                $"Maximum bound violation is {maxBoundViolation:R}.");
        }

        if (maxConstraintNormalizedViolation > feasibilityTolerance)
        {
            diagnostics.Add(
                $"Maximum constraint violation is " +
                $"{maxConstraintViolation:R}; normalized violation is " +
                $"{maxConstraintNormalizedViolation:R}.");
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

    private static double EvaluateConstraintScale(
        LinearConstraint constraint,
        IReadOnlyDictionary<int, double> values)
    {
        double absoluteTermSum = 0.0;

        foreach (LinearTerm term in constraint.Terms)
        {
            if (!values.TryGetValue(
                    term.VariableId,
                    out double value) ||
                !double.IsFinite(value))
            {
                return double.NaN;
            }

            absoluteTermSum +=
                Math.Abs(
                    term.Coefficient *
                    value);
        }

        return Math.Max(
            1.0,
            Math.Max(
                Math.Abs(
                    constraint.RightHandSide),
                absoluteTermSum));
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
