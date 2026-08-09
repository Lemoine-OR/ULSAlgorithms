using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.AggarwalPark.Internal;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.AggarwalPark;

/// <summary>
/// Implements the Aggarwal-Park recursive Monge-matrix algorithm for the
/// uncapacitated economic lot-sizing problem.
/// </summary>
/// <remarks>
/// <para>
/// Aggarwal and Park showed that dynamic programs arising in uncapacitated
/// economic lot sizing can be accelerated by exploiting Monge-array structure.
/// Their general ELS algorithm uses recursive matrix searching and runs in
/// <c>O(n log n)</c> time.
/// </para>
/// <para>
/// This implementation uses the forward transformed-cost recurrence
/// </para>
/// <code>
/// F(t) = min(j &lt; t)
///        F(j) + f[j] - r[j] D[j] + r[j] D[t],
/// </code>
/// <para>
/// where <c>D[t]</c> is cumulative demand and
/// <c>r[j] = p[j] + sum(h[k], k=j..n-2)</c>. A CDQ-style divide-and-conquer
/// over the time indices turns every cross-recursion relaxation into a
/// rectangular implicit matrix. Predecessor columns are ordered by
/// nonincreasing <c>r[j]</c>; because cumulative demand is nondecreasing, each
/// such matrix is Monge. Its row minima are found with SMAWK in linear time.
/// </para>
/// <para>
/// The total work over the divide-and-conquer recursion is
/// <c>O(n log n)</c>. Temporary storage is <c>O(n)</c>. All large temporary
/// arrays are obtained from <see cref="ArrayPool{T}"/>.
/// </para>
/// <para>
/// Primary reference:
/// A. Aggarwal and J. K. Park,
/// "Improved Algorithms for Economic Lot Size Problems",
/// Operations Research 41(3), 549-571, 1993.
/// DOI: 10.1287/opre.41.3.549.
/// </para>
/// <para>
/// A contemporary independent exposition explicitly identifies the
/// Aggarwal-Park ELS implementation as a recursive matrix-searching algorithm
/// and describes their divide-and-conquer treatment of the general,
/// nonmonotone-cost case:
/// S. van Hoesel, A. Wagelmans and B. Moerman,
/// "Using Geometric Techniques to Improve Dynamic Programming Algorithms for
/// the Economic Lot-Sizing Problem and Extensions",
/// European Journal of Operational Research 75(2), 312-331, 1994.
/// DOI: 10.1016/0377-2217(94)90077-9.
/// </para>
/// <para>
/// This is a modern array-based realization of the published matrix-search
/// approach, not a delegation to the Wagelmans convex-envelope solver or the
/// Federgruen-Tzur predecessor-tree solver.
/// </para>
/// </remarks>
public sealed class AggarwalParkSolver : IUlsSolver
{
    /// <inheritdoc />
    public string Name =>
        "Aggarwal-Park Monge matrix search O(n log n)";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <inheritdoc />
    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        var horizon = problem.Horizon;

        var prefixBuffer =
            ArrayPool<double>.Shared.Rent(horizon + 1);

        var transformedCostBuffer =
            ArrayPool<double>.Shared.Rent(horizon);

        var valueBuffer =
            ArrayPool<double>.Shared.Rent(horizon + 1);

        var predecessorBuffer =
            ArrayPool<int>.Shared.Rent(horizon + 1);

        var interceptBuffer =
            ArrayPool<double>.Shared.Rent(horizon);

        var orderBuffer =
            ArrayPool<int>.Shared.Rent(horizon);

        var scratchBuffer =
            ArrayPool<int>.Shared.Rent(horizon);

        var argMinBuffer =
            ArrayPool<int>.Shared.Rent(horizon + 1);

        var matrixWorkspaceBuffer =
            ArrayPool<int>.Shared.Rent(
                checked((2 * (horizon + 1)) + 8));

        var sortKeyBuffer =
            ArrayPool<double>.Shared.Rent(horizon);

        try
        {
            var prefixDemand =
                prefixBuffer.AsSpan(0, horizon + 1);

            var transformedCost =
                transformedCostBuffer.AsSpan(0, horizon);

            var value =
                valueBuffer.AsSpan(0, horizon + 1);

            var predecessor =
                predecessorBuffer.AsSpan(0, horizon + 1);

            var intercept =
                interceptBuffer.AsSpan(0, horizon);

            var order =
                orderBuffer.AsSpan(0, horizon);

            var scratch =
                scratchBuffer.AsSpan(0, horizon);

            var argMin =
                argMinBuffer.AsSpan(0, horizon + 1);

            var matrixWorkspace =
                matrixWorkspaceBuffer.AsSpan(
                    0,
                    checked((2 * (horizon + 1)) + 8));

            var sortKeys =
                sortKeyBuffer.AsSpan(0, horizon);

            BuildPrefixDemand(
                problem.Demands,
                prefixDemand);

            BuildTransformedProductionCosts(
                problem.UnitProductionCosts,
                problem.HoldingCosts,
                transformedCost);

            value.Fill(double.PositiveInfinity);
            predecessor.Fill(-1);
            intercept.Clear();
            argMin.Fill(-1);

            value[0] = 0.0;

            for (var period = 0;
                 period < horizon;
                 period++)
            {
                order[period] = period;

                // Array.Sort is ascending. Negating the key produces
                // nonincreasing transformed marginal costs.
                sortKeys[period] =
                    -transformedCost[period];
            }

            Array.Sort(
                sortKeyBuffer,
                orderBuffer,
                0,
                horizon);

            CanonicalizeEqualSlopeGroups(
                sortKeyBuffer,
                orderBuffer,
                horizon);

            SolveRange(
                left: 0,
                right: horizon,
                orderStart: 0,
                orderCount: horizon,
                problem,
                prefixDemand,
                transformedCost,
                value,
                predecessor,
                intercept,
                order,
                scratch,
                argMin,
                matrixWorkspace,
                cancellationToken);

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
                prefixBuffer,
                clearArray: false);

            ArrayPool<double>.Shared.Return(
                transformedCostBuffer,
                clearArray: false);

            ArrayPool<double>.Shared.Return(
                valueBuffer,
                clearArray: false);

            ArrayPool<int>.Shared.Return(
                predecessorBuffer,
                clearArray: false);

            ArrayPool<double>.Shared.Return(
                interceptBuffer,
                clearArray: false);

            ArrayPool<int>.Shared.Return(
                orderBuffer,
                clearArray: false);

            ArrayPool<int>.Shared.Return(
                scratchBuffer,
                clearArray: false);

            ArrayPool<int>.Shared.Return(
                argMinBuffer,
                clearArray: false);

            ArrayPool<int>.Shared.Return(
                matrixWorkspaceBuffer,
                clearArray: false);

            ArrayPool<double>.Shared.Return(
                sortKeyBuffer,
                clearArray: false);
        }
    }

    private static void SolveRange(
        int left,
        int right,
        int orderStart,
        int orderCount,
        UlsProblem problem,
        ReadOnlySpan<double> prefixDemand,
        ReadOnlySpan<double> transformedCost,
        Span<double> value,
        Span<int> predecessor,
        Span<double> intercept,
        Span<int> order,
        Span<int> scratch,
        Span<int> argMin,
        Span<int> matrixWorkspace,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (left == right)
        {
            FinalizeState(
                left,
                problem,
                prefixDemand,
                transformedCost,
                value,
                predecessor,
                intercept);

            return;
        }

        var middle =
            left + ((right - left) >> 1);

        var leftOrderCount = 0;

        for (var position = 0;
             position < orderCount;
             position++)
        {
            if (order[orderStart + position] <= middle)
            {
                leftOrderCount++;
            }
        }

        var nextLeft = orderStart;
        var nextRight = orderStart + leftOrderCount;

        for (var position = 0;
             position < orderCount;
             position++)
        {
            var period =
                order[orderStart + position];

            if (period <= middle)
            {
                scratch[nextLeft++] = period;
            }
            else
            {
                scratch[nextRight++] = period;
            }
        }

        scratch
            .Slice(orderStart, orderCount)
            .CopyTo(
                order.Slice(orderStart, orderCount));

        var rightOrderStart =
            orderStart + leftOrderCount;

        var rightOrderCount =
            orderCount - leftOrderCount;

        SolveRange(
            left,
            middle,
            orderStart,
            leftOrderCount,
            problem,
            prefixDemand,
            transformedCost,
            value,
            predecessor,
            intercept,
            order,
            scratch,
            argMin,
            matrixWorkspace,
            cancellationToken);

        RelaxCrossMatrix(
            targetStart: middle + 1,
            targetEnd: right,
            predecessorColumns:
                order.Slice(
                    orderStart,
                    leftOrderCount),
            prefixDemand,
            transformedCost,
            intercept,
            value,
            predecessor,
            argMin,
            matrixWorkspace);

        SolveRange(
            middle + 1,
            right,
            rightOrderStart,
            rightOrderCount,
            problem,
            prefixDemand,
            transformedCost,
            value,
            predecessor,
            intercept,
            order,
            scratch,
            argMin,
            matrixWorkspace,
            cancellationToken);

        MergeSlopeOrderedChildren(
            orderStart,
            leftOrderCount,
            rightOrderCount,
            transformedCost,
            order,
            scratch);
    }

    private static void RelaxCrossMatrix(
        int targetStart,
        int targetEnd,
        ReadOnlySpan<int> predecessorColumns,
        ReadOnlySpan<double> prefixDemand,
        ReadOnlySpan<double> transformedCost,
        ReadOnlySpan<double> intercept,
        Span<double> value,
        Span<int> predecessor,
        Span<int> argMin,
        Span<int> matrixWorkspace)
    {
        if (predecessorColumns.IsEmpty ||
            targetStart > targetEnd)
        {
            return;
        }

        var rowCount =
            targetEnd - targetStart + 1;

        AggarwalParkMatrixSearch.FindRowMinima(
            targetStart,
            rowCount,
            predecessorColumns,
            prefixDemand,
            transformedCost,
            intercept,
            argMin,
            matrixWorkspace);

        for (var target = targetStart;
             target <= targetEnd;
             target++)
        {
            var candidatePredecessor =
                argMin[target];

            if (candidatePredecessor < 0)
            {
                throw new InvalidOperationException(
                    "Aggarwal-Park matrix search did not return a predecessor.");
            }

            var candidateValue =
                AggarwalParkMatrixSearch.Evaluate(
                    target,
                    candidatePredecessor,
                    prefixDemand,
                    transformedCost,
                    intercept);

            if (candidateValue < value[target])
            {
                value[target] = candidateValue;
                predecessor[target] =
                    candidatePredecessor;
            }
        }
    }

    private static void FinalizeState(
        int state,
        UlsProblem problem,
        ReadOnlySpan<double> prefixDemand,
        ReadOnlySpan<double> transformedCost,
        Span<double> value,
        Span<int> predecessor,
        Span<double> intercept)
    {
        if (state > 0 &&
            problem.Demands[state - 1] == 0.0 &&
            value[state - 1] <= value[state])
        {
            value[state] =
                value[state - 1];

            predecessor[state] =
                state - 1;
        }

        if (!double.IsFinite(value[state]))
        {
            throw new ArithmeticException(
                $"No finite Aggarwal-Park dynamic-programming value was obtained for state {state}.");
        }

        if (state >= problem.Horizon)
        {
            return;
        }

        var transformedDemandCost =
            transformedCost[state] *
            prefixDemand[state];

        var candidateIntercept =
            value[state] +
            problem.SetupCosts[state] -
            transformedDemandCost;

        if (!double.IsFinite(transformedDemandCost) ||
            !double.IsFinite(candidateIntercept))
        {
            throw new ArithmeticException(
                "Numerical overflow while constructing an Aggarwal-Park predecessor line.");
        }

        intercept[state] =
            candidateIntercept;
    }

    private static void BuildPrefixDemand(
        ReadOnlySpan<double> demands,
        Span<double> prefixDemand)
    {
        prefixDemand[0] = 0.0;

        for (var period = 0;
             period < demands.Length;
             period++)
        {
            var next =
                prefixDemand[period] +
                demands[period];

            if (!double.IsFinite(next))
            {
                throw new ArithmeticException(
                    "Numerical overflow while computing cumulative demand.");
            }

            prefixDemand[period + 1] = next;
        }
    }

    private static void BuildTransformedProductionCosts(
        ReadOnlySpan<double> productionCosts,
        ReadOnlySpan<double> holdingCosts,
        Span<double> transformedCost)
    {
        var holdingSuffix = 0.0;

        for (var period = productionCosts.Length - 1;
             period >= 0;
             period--)
        {
            if (period < productionCosts.Length - 1)
            {
                holdingSuffix +=
                    holdingCosts[period];

                if (!double.IsFinite(holdingSuffix))
                {
                    throw new ArithmeticException(
                        "Numerical overflow while computing holding-cost suffix.");
                }
            }

            var value =
                productionCosts[period] +
                holdingSuffix;

            if (!double.IsFinite(value))
            {
                throw new ArithmeticException(
                    "Numerical overflow while computing transformed production cost.");
            }

            transformedCost[period] = value;
        }
    }

    private static void CanonicalizeEqualSlopeGroups(
        double[] sortedKeys,
        int[] order,
        int length)
    {
        var start = 0;

        while (start < length)
        {
            var end = start + 1;

            while (end < length &&
                   sortedKeys[end] == sortedKeys[start])
            {
                end++;
            }

            if (end - start > 1)
            {
                Array.Sort(
                    order,
                    start,
                    end - start);
            }

            start = end;
        }
    }

    private static void MergeSlopeOrderedChildren(
        int orderStart,
        int leftCount,
        int rightCount,
        ReadOnlySpan<double> transformedCost,
        Span<int> order,
        Span<int> scratch)
    {
        if (leftCount == 0 ||
            rightCount == 0)
        {
            return;
        }

        var leftPosition = orderStart;
        var leftEnd = orderStart + leftCount;

        var rightPosition = leftEnd;
        var rightEnd = rightPosition + rightCount;

        var destination = orderStart;

        while (leftPosition < leftEnd &&
               rightPosition < rightEnd)
        {
            var leftPeriod =
                order[leftPosition];

            var rightPeriod =
                order[rightPosition];

            if (ComesBefore(
                    leftPeriod,
                    rightPeriod,
                    transformedCost))
            {
                scratch[destination++] =
                    leftPeriod;

                leftPosition++;
            }
            else
            {
                scratch[destination++] =
                    rightPeriod;

                rightPosition++;
            }
        }

        while (leftPosition < leftEnd)
        {
            scratch[destination++] =
                order[leftPosition++];
        }

        while (rightPosition < rightEnd)
        {
            scratch[destination++] =
                order[rightPosition++];
        }

        scratch
            .Slice(
                orderStart,
                leftCount + rightCount)
            .CopyTo(
                order.Slice(
                    orderStart,
                    leftCount + rightCount));
    }

    private static bool ComesBefore(
        int leftPeriod,
        int rightPeriod,
        ReadOnlySpan<double> transformedCost)
    {
        var leftSlope =
            transformedCost[leftPeriod];

        var rightSlope =
            transformedCost[rightPeriod];

        if (leftSlope > rightSlope)
        {
            return true;
        }

        if (leftSlope < rightSlope)
        {
            return false;
        }

        return leftPeriod < rightPeriod;
    }
}
