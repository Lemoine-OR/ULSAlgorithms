using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.Internal;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.SaydamMcKnew;

/// <summary>
/// High-throughput full Wagner-Whitin implementation in the spirit of
/// Saydam and McKnew's fast microcomputer program.
/// </summary>
/// <remarks>
/// <para>
/// Saydam and McKnew (1987) describe a very fast implementation of the full
/// original Wagner-Whitin algorithm. They explicitly contrast their approach
/// with Evans' low-storage implementation: Evans is preferred when storage is
/// scarce, whereas their implementation emphasizes execution speed.
/// </para>
/// <para>
/// This modern C# reconstruction follows that design trade-off by materializing
/// the triangular regeneration-cost table in a single contiguous pooled array.
/// The DP phase then reads precomputed arc costs with no repeated regeneration
/// arithmetic.
/// </para>
/// <para>
/// Time complexity is O(T²); working memory is O(T²). The flattened triangular
/// layout avoids per-row objects and improves locality relative to a jagged
/// matrix.
/// </para>
/// <para>
/// Reference:
/// C. Saydam and M. McKnew,
/// "A Fast Microcomputer Program for Ordering Using the Wagner-Whitin
/// Algorithm",
/// Production and Inventory Management Journal 28(4), 15-19, 1987.
/// </para>
/// <para>
/// The article's author-uploaded full-text record states that the logic is a
/// full implementation of the original algorithm and that Evans' approach is
/// preferable when array storage is in severe shortage.
/// </para>
/// </remarks>
public sealed class SaydamMcKnewFastWagnerWhitinSolver : IUlsSolver
{
    public string Name =>
        "Saydam-McKnew fast Wagner-Whitin";

    public UlsSolverKind Kind =>
        UlsSolverKind.Exact;

    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        var horizon = problem.Horizon;

        var triangularLength =
            checked(
                horizon *
                (horizon + 1) /
                2);

        var arcCostBuffer =
            ArrayPool<double>.Shared.Rent(
                triangularLength);

        var valueBuffer =
            ArrayPool<double>.Shared.Rent(
                horizon + 1);

        var predecessorBuffer =
            ArrayPool<int>.Shared.Rent(
                horizon + 1);

        var demandPrefixBuffer =
            ArrayPool<double>.Shared.Rent(
                horizon + 1);

        try
        {
            var arcCosts =
                arcCostBuffer.AsSpan(
                    0,
                    triangularLength);

            var value =
                valueBuffer.AsSpan(
                    0,
                    horizon + 1);

            var predecessor =
                predecessorBuffer.AsSpan(
                    0,
                    horizon + 1);

            var demandPrefix =
                demandPrefixBuffer.AsSpan(
                    0,
                    horizon + 1);

            value.Fill(
                double.PositiveInfinity);

            predecessor.Fill(-1);
            demandPrefix.Clear();

            value[0] = 0.0;

            var demands =
                problem.Demands;

            for (var period = 0;
                 period < horizon;
                 period++)
            {
                demandPrefix[period + 1] =
                    demandPrefix[period] +
                    demands[period];

                if (!double.IsFinite(
                        demandPrefix[period + 1]))
                {
                    throw new ArithmeticException(
                        "Numerical overflow in cumulative demand.");
                }
            }

            MaterializeTriangularCosts(
                problem,
                arcCosts,
                cancellationToken);

            for (var endExclusive = 1;
                 endExclusive <= horizon;
                 endExclusive++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var end =
                    endExclusive - 1;

                var best =
                    double.PositiveInfinity;

                var bestStart = -1;

                if (demands[end] == 0.0)
                {
                    best =
                        value[endExclusive - 1];

                    bestStart =
                        endExclusive - 1;
                }

                for (var start = 0;
                     start <= end;
                     start++)
                {
                    var intervalDemand =
                        demandPrefix[end + 1] -
                        demandPrefix[start];

                    if (intervalDemand == 0.0)
                    {
                        continue;
                    }

                    var candidate =
                        value[start] +
                        arcCosts[
                            TriangularIndex(
                                start,
                                end,
                                horizon)];

                    if (!double.IsFinite(candidate))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow in Saydam-McKnew DP.");
                    }

                    if (candidate < best ||
                        (candidate == best &&
                         start > bestStart))
                    {
                        best = candidate;
                        bestStart = start;
                    }
                }

                if (!double.IsFinite(best) ||
                    bestStart < 0)
                {
                    throw new ArithmeticException(
                        $"No finite Saydam-McKnew value for prefix {endExclusive}.");
                }

                value[endExclusive] = best;
                predecessor[endExclusive] =
                    bestStart;
            }

            return ZeroInventoryOrderSolutionBuilder.Build(
                problem,
                predecessor,
                Name,
                cancellationToken);
        }
        finally
        {
            ArrayPool<double>.Shared.Return(
                arcCostBuffer,
                clearArray: false);

            ArrayPool<double>.Shared.Return(
                valueBuffer,
                clearArray: false);

            ArrayPool<int>.Shared.Return(
                predecessorBuffer,
                clearArray: false);

            ArrayPool<double>.Shared.Return(
                demandPrefixBuffer,
                clearArray: false);
        }
    }

    private static void MaterializeTriangularCosts(
        UlsProblem problem,
        Span<double> arcCosts,
        CancellationToken cancellationToken)
    {
        var horizon = problem.Horizon;
        var demands = problem.Demands;
        var setupCosts = problem.SetupCosts;
        var productionCosts =
            problem.UnitProductionCosts;
        var holdingCosts =
            problem.HoldingCosts;

        for (var start = 0;
             start < horizon;
             start++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var quantity = 0.0;
            var intervalCost = 0.0;
            var deliveredUnitCost =
                productionCosts[start];

            for (var end = start;
                 end < horizon;
                 end++)
            {
                quantity += demands[end];

                if (!double.IsFinite(quantity))
                {
                    throw new ArithmeticException(
                        "Numerical overflow in interval demand.");
                }

                intervalCost +=
                    demands[end] *
                    deliveredUnitCost;

                if (!double.IsFinite(intervalCost))
                {
                    throw new ArithmeticException(
                        "Numerical overflow in interval cost.");
                }

                var cost =
                    quantity == 0.0
                        ? 0.0
                        : setupCosts[start] +
                          intervalCost;

                if (!double.IsFinite(cost))
                {
                    throw new ArithmeticException(
                        "Numerical overflow in regeneration cost.");
                }

                arcCosts[
                    TriangularIndex(
                        start,
                        end,
                        horizon)] = cost;

                if (end < horizon - 1)
                {
                    deliveredUnitCost +=
                        holdingCosts[end];

                    if (!double.IsFinite(deliveredUnitCost))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow in delivered unit cost.");
                    }
                }
            }
        }
    }

    private static int TriangularIndex(
        int start,
        int end,
        int horizon)
    {
        // Number of cells in rows 0..start-1:
        // start*horizon - start*(start-1)/2.
        var rowOffset =
            checked(
                start * horizon -
                start * (start - 1) / 2);

        return checked(
            rowOffset +
            end -
            start);
    }
}
