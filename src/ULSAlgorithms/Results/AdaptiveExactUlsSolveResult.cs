namespace ULSAlgorithms.Results;

/// <summary>
/// Solve result returned by the adaptive exact strategy.
/// </summary>
/// <remarks>
/// In addition to the common <see cref="UlsSolveResult"/> contract, this result
/// records the stable public strategy identifier of the exact algorithm
/// actually selected by the adaptive dispatcher.
/// </remarks>
public sealed class AdaptiveExactUlsSolveResult :
    UlsSolveResult
{
    /// <summary>
    /// Initializes an adaptive exact solve result.
    /// </summary>
    /// <param name="solverName">Human-readable adaptive solver name.</param>
    /// <param name="status">Mathematical solve status.</param>
    /// <param name="selectedAlgorithmId">
    /// Stable public identifier of the algorithm actually executed.
    /// </param>
    /// <param name="selectedSolverName">
    /// Human-readable name reported by the selected solver.
    /// </param>
    /// <param name="solution">Optional feasible solution.</param>
    /// <param name="message">Optional diagnostic message.</param>
    public AdaptiveExactUlsSolveResult(
        string solverName,
        UlsSolveStatus status,
        string selectedAlgorithmId,
        string selectedSolverName,
        UlsSolution? solution = null,
        string? message = null)
        : base(
            solverName,
            status,
            solution,
            message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            selectedAlgorithmId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            selectedSolverName);

        SelectedAlgorithmId =
            selectedAlgorithmId;

        SelectedSolverName =
            selectedSolverName;
    }

    /// <summary>
    /// Gets the stable catalog identifier of the exact strategy actually used.
    /// </summary>
    public string SelectedAlgorithmId { get; }

    /// <summary>
    /// Gets the human-readable solver name returned by the selected strategy.
    /// </summary>
    public string SelectedSolverName { get; }
}
