using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.Internal;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.JacobsKhumawala;

/// <summary>
/// Exact single-level lot-sizing procedure expressed as the simplified
/// branch-and-bound/subproblem scheme of Jacobs and Khumawala.
/// </summary>
/// <remarks>
/// <para>
/// Jacobs and Khumawala (1987) present a simple branch-and-bound procedure for
/// single-item, single-level lot sizing. Their abstract describes the method as
/// computationally equivalent to Wagner-Whitin, but easier to apply through a
/// graphical branch-and-bound representation. Efficiency is obtained by
/// dividing the problem into subproblems and proving that some subproblems
/// cannot lead to an optimum.
/// </para>
/// <para>
/// This modern reconstruction represents each boundary period as a subproblem.
/// Branches are regeneration intervals. For every boundary, only the cheapest
/// label is retained; any more expensive branch reaching the same subproblem is
/// dominated and is discarded. A feasible Lot-for-Lot incumbent supplies an
/// additional global upper bound.
/// </para>
/// <para>
/// Because boundaries are processed in topological order, every retained label
/// is final when expanded. The resulting algorithm is exact, runs in O(T²)
/// time, and uses O(T) auxiliary working memory.
/// </para>
/// <para>
/// Reference:
/// F. R. Jacobs and B. M. Khumawala,
/// "A Simplified Procedure for Optimal Single-Level Lot Sizing",
/// Production and Inventory Management 28(3), 39-43, 1987.
/// </para>
/// <para>
/// This is a C# reconstruction of the published branch/subproblem architecture,
/// not a transliteration of the original graphical worksheet.
/// </para>
/// </remarks>
public sealed class JacobsKhumawalaBranchAndBoundSolver : IUlsSolver
{
    public string Name =>
        "Jacobs-Khumawala simplified branch-and-bound";

    public UlsSolverKind Kind =>
        UlsSolverKind.Exact;

    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        var horizon =
            problem.Horizon;

        var bestLabelBuffer =
            ArrayPool<double>.Shared.Rent(
                horizon + 1);

        var predecessorBuffer =
            ArrayPool<int>.Shared.Rent(
                horizon + 1);

        try
        {
            var bestLabel =
                bestLabelBuffer.AsSpan(
                    0,
                    horizon + 1);

            var predecessor =
                predecessorBuffer.AsSpan(
                    0,
                    horizon + 1);

            bestLabel.Fill(
                double.PositiveInfinity);

            predecessor.Fill(-1);

            bestLabel[0] = 0.0;

            var arc =
                new UlsRegenerationCost(problem);

            var incumbent =
                ComputeLotForLotUpperBound(
                    problem);

            for (var start = 0;
                 start < horizon;
                 start++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var startLabel =
                    bestLabel[start];

                if (!double.IsFinite(startLabel) ||
                    startLabel > incumbent)
                {
                    continue;
                }

                // A zero-demand state may be transferred to the next
                // subproblem without opening an order.
                if (problem.Demands[start] == 0.0 &&
                    startLabel <
                    bestLabel[start + 1])
                {
                    bestLabel[start + 1] =
                        startLabel;

                    predecessor[start + 1] =
                        start;
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
                        startLabel +
                        arc.GetCost(
                            start,
                            end);

                    if (!double.IsFinite(candidate))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow in Jacobs-Khumawala branch cost.");
                    }

                    // Incumbent fathoming.
                    if (candidate > incumbent)
                    {
                        continue;
                    }

                    var boundary =
                        end + 1;

                    // Subproblem dominance:
                    // only the cheapest branch reaching the same boundary
                    // can belong to an optimal continuation.
                    if (candidate <
                            bestLabel[boundary] ||
                        (candidate ==
                            bestLabel[boundary] &&
                         start >
                            predecessor[boundary]))
                    {
                        bestLabel[boundary] =
                            candidate;

                        predecessor[boundary] =
                            start;

                        if (boundary == horizon &&
                            candidate < incumbent)
                        {
                            incumbent =
                                candidate;
                        }
                    }
                }
            }

            if (!double.IsFinite(
                    bestLabel[horizon]))
            {
                throw new ArithmeticException(
                    "Jacobs-Khumawala failed to obtain a finite incumbent.");
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
                bestLabelBuffer,
                clearArray: false);

            ArrayPool<int>.Shared.Return(
                predecessorBuffer,
                clearArray: false);
        }
    }

    private static double ComputeLotForLotUpperBound(
        UlsProblem problem)
    {
        var demands = problem.Demands;
        var setupCosts = problem.SetupCosts;
        var productionCosts =
            problem.UnitProductionCosts;

        var cost = 0.0;

        for (var period = 0;
             period < problem.Horizon;
             period++)
        {
            if (demands[period] == 0.0)
            {
                continue;
            }

            cost +=
                setupCosts[period] +
                demands[period] *
                productionCosts[period];

            if (!double.IsFinite(cost))
            {
                throw new ArithmeticException(
                    "Numerical overflow in Lot-for-Lot upper bound.");
            }
        }

        return cost;
    }
}
