using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Abstractions;

/// <summary>
/// Defines the common strategy contract implemented by every ULS solver.
/// </summary>
/// <remarks>
/// Exact algorithms and heuristics deliberately share this interface so that they
/// can be exchanged through the Strategy pattern without changing calling code.
/// </remarks>
public interface IUlsSolver
{
    /// <summary>
    /// Gets the stable human-readable name of the solver.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the broad family of the solver.
    /// </summary>
    UlsSolverKind Kind { get; }

    /// <summary>
    /// Solves an uncapacitated lot-sizing problem.
    /// </summary>
    /// <param name="problem">The validated ULS problem.</param>
    /// <param name="cancellationToken">A token used to cancel the computation.</param>
    /// <returns>The solve result.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="problem"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when cancellation is requested.
    /// </exception>
    UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default);
}
