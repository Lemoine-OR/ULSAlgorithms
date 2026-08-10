namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// Result of executing a solver-independent linear or mixed-integer model.
/// </summary>
public sealed class LinearModelSolveResult
{
    private readonly IReadOnlyDictionary<int, double> _variableValues;
    private readonly string[] _diagnostics;

    /// <summary>Initializes an execution result.</summary>
    public LinearModelSolveResult(
        string modelName,
        LinearModelSolveStatus status,
        SolverExecutionInfo? solver,
        IReadOnlyDictionary<int, double>? variableValues,
        LinearModelSolutionValidation? validation,
        TimeSpan solveDuration,
        string nativeStatus,
        IEnumerable<string>? diagnostics = null,
        string artifactDirectory = "")
    {
        ModelName = modelName ?? string.Empty;
        Status = status;
        Solver = solver;
        _variableValues =
            variableValues is null
                ? new Dictionary<int, double>()
                : new Dictionary<int, double>(variableValues);
        Validation = validation;
        SolveDuration = solveDuration;
        NativeStatus = nativeStatus ?? string.Empty;
        _diagnostics =
            diagnostics?.ToArray() ??
            [];
        ArtifactDirectory = artifactDirectory ?? string.Empty;
    }

    /// <summary>Gets the submitted model name.</summary>
    public string ModelName { get; }

    /// <summary>Gets the normalized solve status.</summary>
    public LinearModelSolveStatus Status { get; }

    /// <summary>Gets selected-solver provenance, when a solver was selected.</summary>
    public SolverExecutionInfo? Solver { get; }

    /// <summary>Gets variable values keyed by portable variable id.</summary>
    public IReadOnlyDictionary<int, double> VariableValues =>
        _variableValues;

    /// <summary>Gets the independent validation result, when a solution exists.</summary>
    public LinearModelSolutionValidation? Validation { get; }

    /// <summary>Gets the independently recomputed objective value.</summary>
    public double? ObjectiveValue =>
        Validation?.ObjectiveValue;

    /// <summary>Gets elapsed solver execution time.</summary>
    public TimeSpan SolveDuration { get; }

    /// <summary>Gets the provider-native status/log summary.</summary>
    public string NativeStatus { get; }

    /// <summary>Gets execution and validation diagnostics.</summary>
    public IReadOnlyList<string> Diagnostics => _diagnostics;

    /// <summary>
    /// Gets the retained temporary artifact directory when
    /// KeepTemporaryFiles was enabled.
    /// </summary>
    public string ArtifactDirectory { get; }

    /// <summary>Gets whether the result contains an independently valid solution.</summary>
    public bool HasFeasibleSolution =>
        Validation?.IsFeasible == true &&
        Status is
            LinearModelSolveStatus.Optimal or
            LinearModelSolveStatus.Feasible;
}
