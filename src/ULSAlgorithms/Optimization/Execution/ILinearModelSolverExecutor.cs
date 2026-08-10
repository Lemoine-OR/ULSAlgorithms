using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Optimization.Execution;

/// <summary>
/// Executes the portable <see cref="LinearModel"/> with one concrete solver.
/// </summary>
public interface ILinearModelSolverExecutor
{
    /// <summary>Gets the concrete solver implemented by this executor.</summary>
    SolverKind SolverKind { get; }

    /// <summary>Executes one portable model using an already selected solver.</summary>
    ValueTask<LinearModelSolveResult> SolveAsync(
        LinearModel model,
        SolverSelectionResult selection,
        LinearModelSolveOptions options,
        CancellationToken cancellationToken = default);
}
