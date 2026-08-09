using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.FedergruenTzur.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.FedergruenTzur;

/// <summary>
/// Implements Federgruen-Tzur's linear-time forward algorithm for
/// nondecreasing setup costs.
/// </summary>
/// <remarks>
/// <para>
/// The required condition is
/// <c>f[t] &lt;= f[t+1]</c> for every adjacent pair of periods. Variable
/// production and holding costs remain general within the non-negative
/// <see cref="UlsProblem"/> contract; speculative inventory motives are allowed.
/// </para>
/// <para>
/// Federgruen and Tzur prove that a new period either does not enter the
/// Minimal Optimal Predecessor set or is inserted directly at its end. All
/// insertions and deletions therefore occur at the list ends and cost constant
/// amortized time. The complete algorithm is <c>O(n)</c> time and
/// <c>O(n)</c> space.
/// </para>
/// <para>
/// Reference:
/// A. Federgruen and M. Tzur,
/// "A Simple Forward Algorithm to Solve General Dynamic Lot Sizing Models with
/// n Periods in O(n log n) or O(n) Time",
/// Management Science 37(8), 909-925, 1991, Section 3.
/// DOI: 10.1287/mnsc.37.8.909.
/// </para>
/// </remarks>
public sealed class FedergruenTzurNondecreasingSetupSolver : IUlsSolver
{
    /// <inheritdoc />
    public string Name =>
        "Federgruen-Tzur nondecreasing-setup O(n)";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <summary>
    /// Determines whether setup costs are nondecreasing.
    /// </summary>
    public static bool IsApplicable(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var setupCosts = problem.SetupCosts;

        for (var period = 0; period < problem.Horizon - 1; period++)
        {
            if (setupCosts[period] > setupCosts[period + 1])
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">
    /// Thrown when setup costs are not nondecreasing.
    /// </exception>
    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsApplicable(problem))
        {
            throw new NotSupportedException(
                "FedergruenTzurNondecreasingSetupSolver requires " +
                "nondecreasing setup costs.");
        }

        return FedergruenTzurLinearCore.SolveNondecreasingSetupCosts(
            problem,
            Name,
            cancellationToken);
    }
}
