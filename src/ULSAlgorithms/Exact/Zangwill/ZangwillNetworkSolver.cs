using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.Internal;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.Zangwill;

/// <summary>
/// Exact ULS solver using Zangwill's acyclic network representation.
/// </summary>
/// <remarks>
/// <para>
/// A node represents a zero-inventory boundary between periods. An arc from
/// node <c>i</c> to node <c>j</c> represents one replenishment in period
/// <c>i</c> covering periods <c>i..j-1</c>. The ULS problem is therefore a
/// shortest-path problem in a directed acyclic network.
/// </para>
/// <para>
/// This implementation performs the shortest-path recursion backwards from
/// node T to node 0 and evaluates every arc in O(1) using cumulative arrays.
/// Time complexity is O(T²); auxiliary working memory is O(T).
/// </para>
/// <para>
/// Reference:
/// W. I. Zangwill,
/// "A Backlogging Model and a Multi-Echelon Model of a Dynamic Economic Lot
/// Size Production System—A Network Approach",
/// Management Science 15(9), 506-527, 1969.
/// </para>
/// <para>
/// The present class solves the no-backlogging single-echelon specialization
/// represented by <see cref="UlsProblem"/>.
/// </para>
/// </remarks>
public sealed class ZangwillNetworkSolver : IUlsSolver
{
    public string Name =>
        "Zangwill acyclic network";

    public UlsSolverKind Kind =>
        UlsSolverKind.Exact;

    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        var horizon = problem.Horizon;

        var costToGoBuffer =
            ArrayPool<double>.Shared.Rent(
                horizon + 1);

        var successorBuffer =
            ArrayPool<int>.Shared.Rent(
                horizon + 1);

        var predecessorBuffer =
            ArrayPool<int>.Shared.Rent(
                horizon + 1);

        try
        {
            var costToGo =
                costToGoBuffer.AsSpan(
                    0,
                    horizon + 1);

            var successor =
                successorBuffer.AsSpan(
                    0,
                    horizon + 1);

            var predecessor =
                predecessorBuffer.AsSpan(
                    0,
                    horizon + 1);

            costToGo.Fill(
                double.PositiveInfinity);

            successor.Fill(-1);
            predecessor.Fill(-1);

            costToGo[horizon] = 0.0;
            successor[horizon] = horizon;

            var arc =
                new UlsRegenerationCost(problem);

            var demands =
                problem.Demands;

            for (var start = horizon - 1;
                 start >= 0;
                 start--)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var best =
                    double.PositiveInfinity;

                var bestNext = -1;

                // Zero-demand periods may simply be crossed with no setup.
                if (demands[start] == 0.0)
                {
                    best =
                        costToGo[start + 1];

                    bestNext =
                        start + 1;
                }

                for (var end = start;
                     end < horizon;
                     end++)
                {
                    if (arc.GetDemand(
                            start,
                            end) == 0.0)
                    {
                        continue;
                    }

                    var candidate =
                        arc.GetCost(
                            start,
                            end) +
                        costToGo[end + 1];

                    if (!double.IsFinite(candidate))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow in Zangwill shortest path.");
                    }

                    if (candidate < best ||
                        (candidate == best &&
                         end + 1 < bestNext))
                    {
                        best = candidate;
                        bestNext = end + 1;
                    }
                }

                if (!double.IsFinite(best) ||
                    bestNext <= start)
                {
                    throw new ArithmeticException(
                        $"No finite Zangwill path from node {start}.");
                }

                costToGo[start] = best;
                successor[start] = bestNext;
            }

            var node = 0;

            while (node < horizon)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var next =
                    successor[node];

                if (next <= node ||
                    next > horizon)
                {
                    throw new InvalidOperationException(
                        "Invalid Zangwill successor chain.");
                }

                if (arc.GetDemand(
                        node,
                        next - 1) > 0.0)
                {
                    predecessor[next] =
                        node;
                }
                else
                {
                    predecessor[next] =
                        next - 1;
                }

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
            ArrayPool<double>.Shared.Return(
                costToGoBuffer,
                clearArray: false);

            ArrayPool<int>.Shared.Return(
                successorBuffer,
                clearArray: false);

            ArrayPool<int>.Shared.Return(
                predecessorBuffer,
                clearArray: false);
        }
    }
}
