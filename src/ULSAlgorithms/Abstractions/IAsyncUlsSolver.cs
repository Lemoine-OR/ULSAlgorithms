using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Abstractions;

/// <summary>
/// Optional asynchronous companion contract for ULS strategies whose
/// implementation delegates work to an external optimization engine.
/// </summary>
/// <remarks>
/// Every implementation also implements <see cref="IUlsSolver"/> so existing
/// Strategy-pattern code keeps the same synchronous signature.
/// </remarks>
public interface IAsyncUlsSolver
{
    /// <summary>Solves a ULS problem asynchronously.</summary>
    ValueTask<UlsSolveResult> SolveAsync(
        UlsProblem problem,
        CancellationToken cancellationToken = default);
}
