using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.WagnerWhitin;

/// <summary>
/// Implements the data-dependent Wagner-Whitin implementation proposed by
/// Bahl and Taj, combining Evans' low-storage recurrence with the
/// Wagner-Whitin Planning Horizon Theorem.
/// </summary>
/// <remarks>
/// <para>
/// Bahl and Taj (1991) explicitly modify the efficient Evans (1985)
/// implementation by incorporating Wagner's setup/planning-horizon theorem.
/// Once the optimal solution of a positive-demand prefix has its last setup
/// at period <c>j</c>, the theorem permits all earlier candidate setup periods
/// to be excluded from subsequent prefix optimizations. The lower candidate
/// bound is therefore monotone and advances when the data reveal a later
/// planning-horizon boundary.
/// </para>
/// <para>
/// This implementation keeps Evans' incremental regeneration-interval costs:
/// no triangular <c>O(n^2)</c> matrix is materialized. Candidates pruned by
/// the planning-horizon bound are never updated again.
/// </para>
/// <para>
/// The public implementation conservatively enforces the standard
/// Wagner-Whitin / no-speculative-motive cost condition
/// <c>p[t] + h[t] &gt;= p[t+1]</c>. Under that condition the planning-horizon
/// pruning used here is valid.
/// </para>
/// <para>
/// Worst-case time complexity remains <c>O(n^2)</c>; auxiliary working memory
/// is <c>O(n)</c>. The actual number of candidate evaluations is data
/// dependent. When successive optimal prefixes repeatedly establish new,
/// late planning horizons, the practical work can approach <c>O(n)</c>.
/// </para>
/// <para>
/// Primary implementation reference:
/// H. C. Bahl and S. Taj,
/// "A data-dependent efficient implementation of the Wagner-Whitin algorithm
/// for lot-sizing",
/// Computers &amp; Industrial Engineering 20(2), 289-291, 1991.
/// DOI: 10.1016/0360-8352(91)90033-3.
/// </para>
/// <para>
/// Low-storage recurrence reference:
/// J. R. Evans,
/// "An Efficient Implementation of the Wagner-Whitin Algorithm for Dynamic
/// Lot-Sizing",
/// Journal of Operations Management 5(2), 229-235, 1985.
/// DOI: 10.1016/0272-6963(85)90009-9.
/// </para>
/// <para>
/// Planning-horizon theorem reference:
/// H. M. Wagner and T. M. Whitin,
/// "Dynamic Version of the Economic Lot Size Model",
/// Management Science 5(1), 89-96, 1958.
/// DOI: 10.1287/mnsc.5.1.89.
/// </para>
/// <para>
/// This is a modern C# realization of the algorithmic principle described by
/// Bahl and Taj, not a transliteration of their original source code.
/// </para>
/// </remarks>
public sealed class BahlTajPlanningHorizonSolver : IUlsSolver
{
    private const int CancellationCheckMask = 255;

    /// <inheritdoc />
    public string Name =>
        "Bahl-Taj planning-horizon Wagner-Whitin";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <summary>
    /// Determines whether the Wagner-Whitin / no-speculative-motive
    /// condition holds for the supplied problem.
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
    /// Thrown when the Wagner-Whitin / no-speculative-motive condition is
    /// violated.
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
                "BahlTajPlanningHorizonSolver requires " +
                "p[t] + h[t] >= p[t+1] for every adjacent period.");
        }

        var horizon = problem.Horizon;

        var valueBuffer =
            ArrayPool<double>.Shared.Rent(horizon + 1);

        var predecessorBuffer =
            ArrayPool<int>.Shared.Rent(horizon + 1);

        var intervalCostBuffer =
            ArrayPool<double>.Shared.Rent(horizon);

        var deliveredCostBuffer =
            ArrayPool<double>.Shared.Rent(horizon);

        var cumulativeDemandBuffer =
            ArrayPool<double>.Shared.Rent(horizon);

        try
        {
            var value =
                valueBuffer.AsSpan(0, horizon + 1);

            var predecessor =
                predecessorBuffer.AsSpan(0, horizon + 1);

            var intervalCost =
                intervalCostBuffer.AsSpan(0, horizon);

            var deliveredCost =
                deliveredCostBuffer.AsSpan(0, horizon);

            var cumulativeDemand =
                cumulativeDemandBuffer.AsSpan(0, horizon);

            value.Fill(double.PositiveInfinity);
            predecessor.Fill(-1);
            intervalCost.Clear();
            deliveredCost.Clear();
            cumulativeDemand.Clear();

            value[0] = 0.0;

            var demands = problem.Demands;
            var setupCosts = problem.SetupCosts;
            var productionCosts = problem.UnitProductionCosts;
            var holdingCosts = problem.HoldingCosts;

            // Earliest setup period that still needs to be considered.
            // Wagner-Whitin's Planning Horizon Theorem makes this bound
            // monotone nondecreasing.
            var planningHorizonStart = 0;

            for (var end = 0; end < horizon; end++)
            {
                if ((end & CancellationCheckMask) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                // A setup beginning at 'end' becomes a candidate for this
                // prefix and every later prefix until planning-horizon pruning
                // makes it obsolete.
                deliveredCost[end] = productionCosts[end];
                intervalCost[end] = 0.0;
                cumulativeDemand[end] = 0.0;

                if (demands[end] == 0.0)
                {
                    // Extending an already solved prefix through a zero-demand
                    // period requires no setup. Crucially, this zero-length arc
                    // is NOT a planning-horizon certificate: a setup in an
                    // earlier period can still be optimal for future positive
                    // demand. The newly introduced candidate 'end' is retained
                    // for those future periods.
                    value[end + 1] = value[end];
                    predecessor[end + 1] = end;

                    AdvanceDeliveredCosts(
                        planningHorizonStart,
                        end,
                        holdingCosts[end],
                        deliveredCost);

                    continue;
                }

                var best = double.PositiveInfinity;
                var bestStart = -1;

                for (var start = planningHorizonStart;
                     start <= end;
                     start++)
                {
                    cumulativeDemand[start] = AddFinite(
                        cumulativeDemand[start],
                        demands[end],
                        "candidate cumulative demand");

                    intervalCost[start] = AddFinite(
                        intervalCost[start],
                        MultiplyFinite(
                            demands[end],
                            deliveredCost[start],
                            "candidate delivered demand cost"),
                        "candidate regeneration-interval cost");

                    var regenerationCost = AddFinite(
                        setupCosts[start],
                        intervalCost[start],
                        "candidate setup plus regeneration cost");

                    var candidate = AddFinite(
                        value[start],
                        regenerationCost,
                        "forward dynamic-programming candidate");

                    // On an exact tie retain the later setup period. It is also
                    // an optimal predecessor and establishes the strongest
                    // planning-horizon bound allowed by the theorem.
                    if (candidate < best ||
                        (candidate == best && start > bestStart))
                    {
                        best = candidate;
                        bestStart = start;
                    }
                }

                if (!double.IsFinite(best) ||
                    bestStart < planningHorizonStart ||
                    bestStart > end)
                {
                    throw new ArithmeticException(
                        $"No finite Bahl-Taj value was obtained for period {end}.");
                }

                value[end + 1] = best;
                predecessor[end + 1] = bestStart;

                // This is the Bahl-Taj data-dependent pruning step.
                planningHorizonStart = bestStart;

                AdvanceDeliveredCosts(
                    planningHorizonStart,
                    end,
                    holdingCosts[end],
                    deliveredCost);
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

            ArrayPool<double>.Shared.Return(
                intervalCostBuffer,
                clearArray: false);

            ArrayPool<double>.Shared.Return(
                deliveredCostBuffer,
                clearArray: false);

            ArrayPool<double>.Shared.Return(
                cumulativeDemandBuffer,
                clearArray: false);
        }
    }

    private static void AdvanceDeliveredCosts(
        int firstActiveStart,
        int end,
        double holdingCost,
        Span<double> deliveredCost)
    {
        if (holdingCost == 0.0)
        {
            return;
        }

        for (var start = firstActiveStart;
             start <= end;
             start++)
        {
            deliveredCost[start] = AddFinite(
                deliveredCost[start],
                holdingCost,
                "candidate delivered unit cost");
        }
    }

    private static double AddFinite(
        double left,
        double right,
        string operation)
    {
        var value = left + right;

        if (!double.IsFinite(value))
        {
            throw new ArithmeticException(
                $"Numerical overflow while computing {operation}.");
        }

        return value;
    }

    private static double MultiplyFinite(
        double left,
        double right,
        string operation)
    {
        var value = left * right;

        if (!double.IsFinite(value))
        {
            throw new ArithmeticException(
                $"Numerical overflow while computing {operation}.");
        }

        return value;
    }
}
