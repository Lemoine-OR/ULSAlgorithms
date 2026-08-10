using ULSAlgorithms.Formulations;
using ULSAlgorithms.Optimization;
using ULSAlgorithms.Optimization.Execution;

namespace ULSAlgorithms.Results;

/// <summary>
/// ULS solve result enriched with mathematical-formulation and optimization
/// engine provenance.
/// </summary>
public sealed class SolverBackedUlsSolveResult :
    UlsSolveResult
{
    /// <summary>Initializes a solver-backed ULS result.</summary>
    public SolverBackedUlsSolveResult(
        string solverName,
        UlsSolveStatus status,
        UlsFormulationKind formulationKind,
        LinearModelSolveResult modelExecution,
        UlsSolution? solution = null,
        string? message = null)
        : base(
            solverName,
            status,
            solution,
            message)
    {
        FormulationKind = formulationKind;
        ModelExecution =
            modelExecution ??
            throw new ArgumentNullException(
                nameof(modelExecution));
    }

    /// <summary>Gets the mathematical formulation used.</summary>
    public UlsFormulationKind FormulationKind { get; }

    /// <summary>
    /// Gets the complete portable-model execution result, including normalized
    /// variable values and independent model validation.
    /// </summary>
    public LinearModelSolveResult ModelExecution { get; }

    /// <summary>Gets the concrete selected optimization engine, when available.</summary>
    public SolverExecutionInfo? OptimizationSolver =>
        ModelExecution.Solver;
}
