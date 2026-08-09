using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.ChowdhuryBakiAzab;

/// <summary>
/// Implements the linear-time Wagner-Whitin algorithm of
/// Chowdhury, Baki and Azab.
/// </summary>
/// <remarks>
/// <para>
/// The algorithm works backwards on the Wagner-Whitin shortest-path network.
/// Instead of constructing the triangular advantage matrices introduced in the
/// paper, it maintains only the active diagonals, scheduled deletion events,
/// and the stack summaries required by Algorithm 1.
/// </para>
/// <para>
/// Time complexity: <c>O(T)</c>.
/// Auxiliary working memory: <c>O(T)</c>.
/// </para>
/// <para>
/// The implementation follows Algorithm 1 and Theorems 1-5 of the detailed
/// primary exposition in N. T. Chowdhury's doctoral dissertation, Chapter 2,
/// which corresponds to the published article:
/// N. T. Chowdhury, M. F. Baki and A. Azab,
/// "Dynamic Economic Lot-Sizing Problem: A new O(T) Algorithm for the
/// Wagner-Whitin Model",
/// Computers &amp; Industrial Engineering 117, 6-18, 2018.
/// DOI: 10.1016/j.cie.2018.01.010.
/// </para>
/// <para>
/// The paper assumes stationary unit holding cost <c>h</c>, time-varying setup
/// costs <c>f[t]</c>, and the Wagner-Whitin cost structure. Constant unit
/// production cost may be present because it adds the same amount to every
/// feasible policy.
/// </para>
/// <para>
/// To preserve the published arithmetic exactly, this public implementation
/// conservatively requires strictly positive demands and strictly positive
/// stationary holding cost when the horizon contains more than one period.
/// </para>
/// </remarks>
public sealed class ChowdhuryBakiAzabSolver : IUlsSolver
{
    private const int CancellationCheckMask = 255;

    /// <inheritdoc />
    public string Name => "Chowdhury-Baki-Azab O(T)";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <summary>
    /// Determines whether the problem satisfies the published algorithm's
    /// stationary-cost domain used by this implementation.
    /// </summary>
    public static bool IsApplicable(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var horizon = problem.Horizon;
        var demands = problem.Demands;
        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        for (var period = 0; period < horizon; period++)
        {
            if (!(demands[period] > 0.0))
            {
                return false;
            }
        }

        var productionCost = productionCosts[0];

        for (var period = 1; period < horizon; period++)
        {
            if (productionCosts[period] != productionCost)
            {
                return false;
            }
        }

        if (horizon <= 1)
        {
            return true;
        }

        var holdingCost = holdingCosts[0];

        if (!(holdingCost > 0.0))
        {
            return false;
        }

        for (var period = 1; period < horizon - 1; period++)
        {
            if (holdingCosts[period] != holdingCost)
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">
    /// Thrown when demands are not strictly positive, relevant holding costs
    /// are not stationary and positive, or unit production costs are not
    /// constant.
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
                "ChowdhuryBakiAzabSolver requires strictly positive demands, " +
                "constant unit production costs, and a strictly positive " +
                "stationary relevant holding cost.");
        }

        var horizon = problem.Horizon;

        if (horizon == 1)
        {
            var predecessor = new int[2];
            predecessor[0] = -1;
            predecessor[1] = 0;

            return ZeroInventoryOrderSolutionBuilder.Build(
                problem,
                predecessor,
                Name,
                cancellationToken);
        }

        var eventCapacity = checked((2 * horizon) + 8);

        var gBuffer = ArrayPool<double>.Shared.Rent(horizon + 2);
        var aBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);
        var bBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);
        var prefixDemandBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);
        var prefixWeightedDemandBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);

        var activePreviousBuffer = ArrayPool<int>.Shared.Rent(horizon + 1);
        var activeNextBuffer = ArrayPool<int>.Shared.Rent(horizon + 1);
        var bestSuccessorBuffer = ArrayPool<int>.Shared.Rent(horizon + 1);
        var listHeadBuffer = ArrayPool<int>.Shared.Rent(horizon + 1);
        var eventPeriodBuffer = ArrayPool<int>.Shared.Rent(eventCapacity);
        var eventNextBuffer = ArrayPool<int>.Shared.Rent(eventCapacity);
        var predecessorBuffer = ArrayPool<int>.Shared.Rent(horizon + 1);

        try
        {
            var g = gBuffer.AsSpan(0, horizon + 2);
            var a = aBuffer.AsSpan(0, horizon + 1);
            var b = bBuffer.AsSpan(0, horizon + 1);
            var prefixDemand = prefixDemandBuffer.AsSpan(0, horizon + 1);
            var prefixWeightedDemand =
                prefixWeightedDemandBuffer.AsSpan(0, horizon + 1);

            var activePrevious =
                activePreviousBuffer.AsSpan(0, horizon + 1);
            var activeNext =
                activeNextBuffer.AsSpan(0, horizon + 1);
            var bestSuccessor =
                bestSuccessorBuffer.AsSpan(0, horizon + 1);
            var listHead =
                listHeadBuffer.AsSpan(0, horizon + 1);
            var eventPeriod =
                eventPeriodBuffer.AsSpan(0, eventCapacity);
            var eventNext =
                eventNextBuffer.AsSpan(0, eventCapacity);
            var predecessor =
                predecessorBuffer.AsSpan(0, horizon + 1);

            g.Clear();
            a.Clear();
            b.Clear();
            prefixDemand.Clear();
            prefixWeightedDemand.Clear();
            activePrevious.Clear();
            activeNext.Clear();
            bestSuccessor.Clear();
            listHead.Fill(-1);
            predecessor.Fill(-1);

            var demands = problem.Demands;
            var setupCosts = problem.SetupCosts;
            var holdingCost = problem.HoldingCosts[0];

            for (var period = 1; period <= horizon; period++)
            {
                var demand = demands[period - 1];

                prefixDemand[period] = AddFinite(
                    prefixDemand[period - 1],
                    demand,
                    "cumulative demand");

                prefixWeightedDemand[period] = AddFinite(
                    prefixWeightedDemand[period - 1],
                    MultiplyFinite(period, demand, "weighted demand"),
                    "weighted cumulative demand");
            }

            activePrevious[1] = 0;
            activeNext[0] = 1;

            for (var diagonal = 2; diagonal <= horizon; diagonal++)
            {
                activePrevious[diagonal] = diagonal - 1;
            }

            for (var diagonal = 1; diagonal < horizon; diagonal++)
            {
                activeNext[diagonal] = diagonal + 1;
            }

            g[horizon + 1] = 0.0;
            a[horizon] = 0.0;
            g[horizon] = setupCosts[horizon - 1];
            bestSuccessor[horizon] = horizon + 1;

            var bestDiagonal = horizon - 1;
            var eventCount = 0;
            var cancellationCounter = 0;

            for (var k = horizon - 1; k >= 1; k--)
            {
                if ((cancellationCounter++ & CancellationCheckMask) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                a[k] =
                    g[k + 1] -
                    g[k + 2] -
                    MultiplyFinite(
                        holdingCost,
                        demands[k],
                        "Algorithm 1 advantage");

                EnsureFinite(a[k], "Algorithm 1 advantage");

                b[k] = MultiplyFinite(
                    holdingCost,
                    demands[k],
                    "Algorithm 1 slope");

                var u = ClampedCeilingRatio(
                    a[k],
                    b[k],
                    horizon);

                if (u <= k - 1)
                {
                    Schedule(
                        k - u,
                        k,
                        listHead,
                        eventPeriod,
                        eventNext,
                        ref eventCount);
                }

                var eventIndex = listHead[k];

                while (eventIndex >= 0)
                {
                    if ((cancellationCounter++ & CancellationCheckMask) == 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    var p = eventPeriod[eventIndex];

                    if (p <= bestDiagonal &&
                        activeNext[activePrevious[p]] == p)
                    {
                        var delta =
                            a[p] -
                            MultiplyFinite(
                                p - k,
                                b[p],
                                "stack advantage shift");

                        var aggregatedSlope = b[p];
                        var stackHead = p;

                        while (delta <= 0.0 &&
                               p <= bestDiagonal)
                        {
                            if ((cancellationCounter++ &
                                 CancellationCheckMask) == 0)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            if (p < bestDiagonal)
                            {
                                activeNext[activePrevious[p]] =
                                    activeNext[p];

                                activePrevious[activeNext[p]] =
                                    activePrevious[p];

                                p = activeNext[p];

                                delta = AddFinite(
                                    delta,
                                    a[p] -
                                    MultiplyFinite(
                                        p - k,
                                        b[p],
                                        "stack advantage shift"),
                                    "stack advantage aggregation");

                                aggregatedSlope = AddFinite(
                                    aggregatedSlope,
                                    b[p],
                                    "stack slope aggregation");

                                if (delta > 0.0)
                                {
                                    a[p] = AddFinite(
                                        delta,
                                        MultiplyFinite(
                                            p - k,
                                            aggregatedSlope,
                                            "stack compressed intercept"),
                                        "stack compressed intercept");

                                    b[p] = aggregatedSlope;

                                    u = ClampedCeilingRatio(
                                        delta,
                                        b[p],
                                        horizon);

                                    if (u <= k - 1)
                                    {
                                        Schedule(
                                            k - u,
                                            p,
                                            listHead,
                                            eventPeriod,
                                            eventNext,
                                            ref eventCount);
                                    }
                                }
                            }
                            else
                            {
                                bestDiagonal =
                                    activePrevious[stackHead];
                            }
                        }
                    }

                    eventIndex = eventNext[eventIndex];
                }

                bestSuccessor[k] = bestDiagonal + 2;

                var holdingArcCost = ComputeHoldingArcCost(
                    k,
                    bestSuccessor[k],
                    holdingCost,
                    prefixDemand,
                    prefixWeightedDemand);

                g[k] = AddFinite(
                    setupCosts[k - 1],
                    holdingArcCost,
                    "backward shortest-path cost");

                g[k] = AddFinite(
                    g[k],
                    g[bestSuccessor[k]],
                    "backward shortest-path cost");
            }

            var node = 1;

            while (node <= horizon)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var next = bestSuccessor[node];

                if (next <= node ||
                    next > horizon + 1)
                {
                    throw new InvalidOperationException(
                        "The Chowdhury-Baki-Azab successor chain is inconsistent.");
                }

                predecessor[next - 1] = node - 1;
                node = next;
            }

            return ZeroInventoryOrderSolutionBuilder.Build(
                problem,
                predecessor,
                Name,
                cancellationToken);
        }
        finally
        {
            ArrayPool<double>.Shared.Return(gBuffer, clearArray: false);
            ArrayPool<double>.Shared.Return(aBuffer, clearArray: false);
            ArrayPool<double>.Shared.Return(bBuffer, clearArray: false);
            ArrayPool<double>.Shared.Return(prefixDemandBuffer, clearArray: false);
            ArrayPool<double>.Shared.Return(
                prefixWeightedDemandBuffer,
                clearArray: false);

            ArrayPool<int>.Shared.Return(activePreviousBuffer, clearArray: false);
            ArrayPool<int>.Shared.Return(activeNextBuffer, clearArray: false);
            ArrayPool<int>.Shared.Return(bestSuccessorBuffer, clearArray: false);
            ArrayPool<int>.Shared.Return(listHeadBuffer, clearArray: false);
            ArrayPool<int>.Shared.Return(eventPeriodBuffer, clearArray: false);
            ArrayPool<int>.Shared.Return(eventNextBuffer, clearArray: false);
            ArrayPool<int>.Shared.Return(predecessorBuffer, clearArray: false);
        }
    }

    private static void Schedule(
        int list,
        int period,
        Span<int> listHead,
        Span<int> eventPeriod,
        Span<int> eventNext,
        ref int eventCount)
    {
        if ((uint)list >= (uint)listHead.Length)
        {
            throw new InvalidOperationException(
                "Algorithm 1 attempted to schedule an invalid list index.");
        }

        if (eventCount >= eventPeriod.Length)
        {
            throw new InvalidOperationException(
                "Algorithm 1 exceeded the proven O(T) event bound.");
        }

        eventPeriod[eventCount] = period;
        eventNext[eventCount] = listHead[list];
        listHead[list] = eventCount;
        eventCount++;
    }

    private static int ClampedCeilingRatio(
        double numerator,
        double denominator,
        int upperBound)
    {
        if (!(denominator > 0.0) ||
            !double.IsFinite(denominator))
        {
            throw new ArithmeticException(
                "Algorithm 1 requires a finite positive b(k).");
        }

        var ratio = numerator / denominator;

        if (!double.IsFinite(ratio))
        {
            throw new ArithmeticException(
                "Algorithm 1 produced a non-finite advantage ratio.");
        }

        if (ratio <= 0.0)
        {
            return 0;
        }

        if (ratio >= upperBound)
        {
            return upperBound;
        }

        return (int)Math.Ceiling(ratio);
    }

    private static double ComputeHoldingArcCost(
        int startNode,
        int successorNode,
        double holdingCost,
        ReadOnlySpan<double> prefixDemand,
        ReadOnlySpan<double> prefixWeightedDemand)
    {
        var lastDemandPeriod = successorNode - 1;

        if (lastDemandPeriod <= startNode)
        {
            return 0.0;
        }

        var futureDemand =
            prefixDemand[lastDemandPeriod] -
            prefixDemand[startNode];

        var weightedFutureDemand =
            prefixWeightedDemand[lastDemandPeriod] -
            prefixWeightedDemand[startNode];

        var partPeriods =
            weightedFutureDemand -
            MultiplyFinite(
                startNode,
                futureDemand,
                "part-period quantity");

        return MultiplyFinite(
            holdingCost,
            partPeriods,
            "regeneration-interval holding cost");
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
