namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// Independent solver-agnostic validation of a returned variable assignment.
/// </summary>
public sealed class LinearModelSolutionValidation
{
    /// <summary>Initializes a validation result.</summary>
    public LinearModelSolutionValidation(
        bool isFeasible,
        double objectiveValue,
        double maximumBoundViolation,
        double maximumIntegralityViolation,
        double maximumConstraintViolation,
        IEnumerable<string> diagnostics)
    {
        IsFeasible = isFeasible;
        ObjectiveValue = objectiveValue;
        MaximumBoundViolation = maximumBoundViolation;
        MaximumIntegralityViolation = maximumIntegralityViolation;
        MaximumConstraintViolation = maximumConstraintViolation;
        Diagnostics = diagnostics.ToArray();
    }

    /// <summary>Gets whether all checked requirements passed.</summary>
    public bool IsFeasible { get; }

    /// <summary>Gets the independently recomputed objective value.</summary>
    public double ObjectiveValue { get; }

    /// <summary>Gets the maximum variable-bound violation.</summary>
    public double MaximumBoundViolation { get; }

    /// <summary>Gets the maximum integrality violation.</summary>
    public double MaximumIntegralityViolation { get; }

    /// <summary>Gets the maximum linear-constraint violation.</summary>
    public double MaximumConstraintViolation { get; }

    /// <summary>Gets validation diagnostics.</summary>
    public IReadOnlyList<string> Diagnostics { get; }
}
