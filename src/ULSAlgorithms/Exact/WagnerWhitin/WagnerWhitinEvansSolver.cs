using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.WagnerWhitin;

/// <summary>
/// Implements the low-storage Wagner-Whitin dynamic program described by Evans.
/// </summary>
/// <remarks>
/// <para>
/// Evans observed that the classical Wagner-Whitin problem is a shortest-path
/// computation on an acyclic network and that regeneration-interval costs do not
/// need to be stored in a complete matrix. This implementation updates candidate
/// interval costs incrementally as the planning horizon is extended.
/// </para>
/// <para>
/// Time complexity: <c>O(n^2)</c>.
/// Auxiliary working memory: <c>O(n)</c>.
/// </para>
/// <para>
/// Reference:
/// J. R. Evans,
/// "An Efficient Implementation of the Wagner-Whitin Algorithm for Dynamic
/// Lot-Sizing", Journal of Operations Management 5(2), 229-235, 1985.
/// DOI: 10.1016/0272-6963(85)90009-9.
/// </para>
/// <para>
/// The code is an equivalent modern C# implementation of Evans' low-core-storage
/// recurrence, not a transliteration of the original FORTRAN listing.
/// </para>
/// </remarks>
public sealed class WagnerWhitinEvansSolver : IUlsSolver
{
    /// <inheritdoc />
    public string Name => "Wagner-Whitin Evans 1985";

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

        var valueBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);
        var predecessorBuffer = ArrayPool<int>.Shared.Rent(horizon + 1);
        var intervalCostBuffer = ArrayPool<double>.Shared.Rent(horizon);
        var deliveredCostBuffer = ArrayPool<double>.Shared.Rent(horizon);
        var cumulativeDemandBuffer = ArrayPool<double>.Shared.Rent(horizon);

        try
        {
            var value = valueBuffer.AsSpan(0, horizon + 1);
            var predecessor = predecessorBuffer.AsSpan(0, horizon + 1);
            var intervalCost = intervalCostBuffer.AsSpan(0, horizon);
            var deliveredCost = deliveredCostBuffer.AsSpan(0, horizon);
            var cumulativeDemand = cumulativeDemandBuffer.AsSpan(0, horizon);

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

            for (var end = 0; end < horizon; end++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                deliveredCost[end] = productionCosts[end];
                intervalCost[end] = 0.0;
                cumulativeDemand[end] = 0.0;

                var best = double.PositiveInfinity;
                var bestStart = -1;

                for (var start = 0; start <= end; start++)
                {
                    cumulativeDemand[start] += demands[end];
                    intervalCost[start] += demands[end] * deliveredCost[start];

                    if (!double.IsFinite(intervalCost[start]))
                    {
                        throw new ArithmeticException(
                            "Numerical overflow while updating an Evans regeneration interval.");
                    }

                    var regenerationCost =
                        cumulativeDemand[start] > 0.0
                            ? setupCosts[start] + intervalCost[start]
                            : 0.0;

                    var candidate = value[start] + regenerationCost;

                    if (candidate < best)
                    {
                        best = candidate;
                        bestStart = start;
                    }
                }

                if (!double.IsFinite(best) || bestStart < 0)
                {
                    throw new ArithmeticException(
                        $"No finite Evans dynamic-programming value was obtained for period {end}.");
                }

                value[end + 1] = best;
                predecessor[end + 1] = bestStart;

                if (end < horizon - 1)
                {
                    for (var start = 0; start <= end; start++)
                    {
                        deliveredCost[start] += holdingCosts[end];

                        if (!double.IsFinite(deliveredCost[start]))
                        {
                            throw new ArithmeticException(
                                "Numerical overflow while updating Evans delivered unit costs.");
                        }
                    }
                }
            }

            return ZeroInventoryOrderSolutionBuilder.Build(
                problem,
                predecessor,
                Name,
                cancellationToken);
        }
        finally
        {
            ArrayPool<double>.Shared.Return(valueBuffer, clearArray: false);
            ArrayPool<int>.Shared.Return(predecessorBuffer, clearArray: false);
            ArrayPool<double>.Shared.Return(intervalCostBuffer, clearArray: false);
            ArrayPool<double>.Shared.Return(deliveredCostBuffer, clearArray: false);
            ArrayPool<double>.Shared.Return(cumulativeDemandBuffer, clearArray: false);
        }
    }
}
