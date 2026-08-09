using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.FedergruenTzur.Internal;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.FedergruenTzur;

/// <summary>
/// Implements the general forward Federgruen-Tzur dynamic lot-sizing algorithm.
/// </summary>
/// <remarks>
/// <para>
/// This exact solver maintains Federgruen and Tzur's Minimal Optimal
/// Predecessor set as a lower envelope of the affine functions
/// <c>F(i,t) - S(t)</c>. Candidate periods are ranked by
/// <c>C(i) = p[i] - H(i-1)</c>; adjacent envelope intersections are the
/// <c>G(k,l)</c> cumulative-demand thresholds of the paper.
/// </para>
/// <para>
/// The ranked predecessor structure is implemented as an array-backed AVL
/// binary search tree augmented with predecessor/successor links. Each period
/// enters the structure at most once and a dominated period is deleted at most
/// once. Insertions/deletions cost <c>O(log n)</c>, so the complete general
/// algorithm uses <c>O(n log n)</c> time and <c>O(n)</c> memory.
/// </para>
/// <para>
/// Reference:
/// A. Federgruen and M. Tzur,
/// "A Simple Forward Algorithm to Solve General Dynamic Lot Sizing Models with
/// n Periods in O(n log n) or O(n) Time",
/// Management Science 37(8), 909-925, 1991.
/// DOI: 10.1287/mnsc.37.8.909.
/// </para>
/// <para>
/// The implementation is an equivalent modern representation of the paper's
/// Minimal Optimal Predecessor list. The paper explicitly recommends a balanced
/// binary tree to avoid linear fetch/store work for insertion and deletion.
/// Here the balanced tree is stored in pooled primitive arrays to avoid
/// per-candidate managed allocations.
/// </para>
/// </remarks>
public sealed class FedergruenTzurSolver : IUlsSolver
{
    private const int CancellationCheckMask = 255;

    /// <inheritdoc />
    public string Name => "Federgruen-Tzur general O(n log n)";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <inheritdoc />
    /// <exception cref="ArithmeticException">
    /// Thrown when cumulative or transformed values cannot be represented as
    /// finite <see cref="double"/> values.
    /// </exception>
    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        var horizon = problem.Horizon;
        var valueBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);
        var predecessorBuffer = ArrayPool<int>.Shared.Rent(horizon + 1);

        try
        {
            var value = valueBuffer.AsSpan(0, horizon + 1);
            var predecessor = predecessorBuffer.AsSpan(0, horizon + 1);

            value[0] = 0.0;
            predecessor[0] = -1;

            var demands = problem.Demands;
            var setupCosts = problem.SetupCosts;
            var productionCosts = problem.UnitProductionCosts;
            var holdingCosts = problem.HoldingCosts;

            var cumulativeDemand = 0.0;
            var cumulativeHoldingBefore = 0.0;
            var firstPeriodHoldingCost = 0.0;

            using var candidates =
                new FedergruenTzurCandidateTree(horizon);

            for (var period = 0; period < horizon; period++)
            {
                if ((period & CancellationCheckMask) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                var previousCumulativeDemand = cumulativeDemand;

                cumulativeDemand = AddFinite(
                    cumulativeDemand,
                    demands[period],
                    "cumulative demand");

                firstPeriodHoldingCost = AddFinite(
                    firstPeriodHoldingCost,
                    MultiplyFinite(
                        demands[period],
                        cumulativeHoldingBefore,
                        "first-period-order holding-cost transform"),
                    "first-period-order holding-cost transform");

                // Federgruen-Tzur notation:
                // C(i) = c_i - H(i-1).
                var transformedVariableCost =
                    productionCosts[period] -
                    cumulativeHoldingBefore;

                EnsureFinite(
                    transformedVariableCost,
                    "transformed variable production cost");

                // From equations (1d) and (2) in Federgruen-Tzur:
                //
                // F(i,t) = S(t) + B_i + C(i) D(t),
                //
                // B_i = F(i-1) + K_i - S(i)
                //       + D(i)H(i-1) - c_i D(i-1).
                var intercept = value[period];

                intercept = AddFinite(
                    intercept,
                    setupCosts[period],
                    "candidate intercept");

                intercept = AddFinite(
                    intercept,
                    -firstPeriodHoldingCost,
                    "candidate intercept");

                intercept = AddFinite(
                    intercept,
                    MultiplyFinite(
                        cumulativeDemand,
                        cumulativeHoldingBefore,
                        "candidate intercept"),
                    "candidate intercept");

                intercept = AddFinite(
                    intercept,
                    -MultiplyFinite(
                        productionCosts[period],
                        previousCumulativeDemand,
                        "candidate intercept"),
                    "candidate intercept");

                candidates.Add(
                    period,
                    transformedVariableCost,
                    intercept);

                var bestPeriod =
                    candidates.GetBestAndDiscardPast(
                        cumulativeDemand);

                var bestLineValue = AddFinite(
                    candidates.GetIntercept(bestPeriod),
                    MultiplyFinite(
                        candidates.GetSlope(bestPeriod),
                        cumulativeDemand,
                        "candidate line evaluation"),
                    "candidate line evaluation");

                var orderValue = AddFinite(
                    firstPeriodHoldingCost,
                    bestLineValue,
                    "forward dynamic-programming value");

                // If the current period has zero demand, the horizon can be
                // extended without an order. Represent this zero-length arc by
                // predecessor=period so the shared ZIO reconstruction remains
                // valid without creating a zero-quantity setup.
                if (demands[period] == 0.0 &&
                    value[period] <= orderValue)
                {
                    value[period + 1] = value[period];
                    predecessor[period + 1] = period;
                }
                else
                {
                    value[period + 1] = orderValue;
                    predecessor[period + 1] = bestPeriod;
                }

                cumulativeHoldingBefore = AddFinite(
                    cumulativeHoldingBefore,
                    holdingCosts[period],
                    "cumulative holding cost");
            }

            cancellationToken.ThrowIfCancellationRequested();

            return ZeroInventoryOrderSolutionBuilder.Build(
                problem,
                predecessor,
                Name,
                cancellationToken);
        }
        finally
        {
            ArrayPool<double>.Shared.Return(
                valueBuffer,
                clearArray: false);

            ArrayPool<int>.Shared.Return(
                predecessorBuffer,
                clearArray: false);
        }
    }

    private static double AddFinite(
        double left,
        double right,
        string operation)
    {
        var value = left + right;
        EnsureFinite(value, operation);
        return value;
    }

    private static double MultiplyFinite(
        double left,
        double right,
        string operation)
    {
        var value = left * right;
        EnsureFinite(value, operation);
        return value;
    }

    private static void EnsureFinite(
        double value,
        string operation)
    {
        if (!double.IsFinite(value))
        {
            throw new ArithmeticException(
                $"Numerical overflow while computing {operation}.");
        }
    }
}
