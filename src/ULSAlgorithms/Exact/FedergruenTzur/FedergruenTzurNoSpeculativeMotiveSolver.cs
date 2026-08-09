using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.FedergruenTzur.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.FedergruenTzur;

/// <summary>
/// Implements Federgruen-Tzur's linear-time forward algorithm for models
/// without speculative inventory motives.
/// </summary>
/// <remarks>
/// <para>
/// Applicability is equivalent to the transformed variable-cost sequence
/// <c>C(t)=p[t]-H(t-1)</c> being nonincreasing. In the original cost notation,
/// this is the adjacent condition
/// <c>p[t] + h[t] &gt;= p[t+1]</c>.
/// </para>
/// <para>
/// Under this condition Federgruen and Tzur show that each new period is
/// inserted at the end of the Minimal Optimal Predecessor list. Candidates are
/// deleted only from the front or back, so an ordinary array/list structure
/// gives <c>O(n)</c> time and <c>O(n)</c> space.
/// </para>
/// <para>
/// Reference:
/// A. Federgruen and M. Tzur,
/// "A Simple Forward Algorithm to Solve General Dynamic Lot Sizing Models with
/// n Periods in O(n log n) or O(n) Time",
/// Management Science 37(8), 909-925, 1991, Section 4.
/// DOI: 10.1287/mnsc.37.8.909.
/// </para>
/// </remarks>
public sealed class FedergruenTzurNoSpeculativeMotiveSolver : IUlsSolver
{
    /// <inheritdoc />
    public string Name =>
        "Federgruen-Tzur no-speculative-motive O(n)";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <summary>
    /// Determines whether the no-speculative-motive condition holds.
    /// </summary>
    public static bool IsApplicable(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        for (var period = 0; period < problem.Horizon - 1; period++)
        {
            var deliveredNext =
                productionCosts[period] +
                holdingCosts[period];

            if (!double.IsFinite(deliveredNext) ||
                deliveredNext < productionCosts[period + 1])
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">
    /// Thrown when the no-speculative-motive condition is violated.
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
                "FedergruenTzurNoSpeculativeMotiveSolver requires " +
                "p[t] + h[t] >= p[t+1] for every adjacent period.");
        }

        return FedergruenTzurLinearCore.SolveNoSpeculativeMotive(
            problem,
            Name,
            cancellationToken);
    }
}
