using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.WagnerWhitin;

/// <summary>
/// Implements the Heady-Zhu family of improved Wagner-Whitin procedures using
/// the Planning Horizon Theorem and the Economic-Part-Period pruning concept.
/// </summary>
/// <remarks>
/// <para>
/// Heady and Zhu (1994) report an improved exact implementation of the
/// Wagner-Whitin algorithm based on the Planning Horizon Theorem and the
/// Economic-Part-Period concept.
/// </para>
/// <para>
/// This implementation realizes the fixed-cost specialization of that
/// algorithmic idea. It requires a constant setup cost, a constant unit
/// production cost and a constant per-unit-per-period holding cost over the
/// economically relevant periods. These assumptions make the
/// Economic-Part-Period cutoff exact and auditable.
/// </para>
/// <para>
/// For fixed setup cost <c>A</c> and holding cost <c>h</c>, the economic
/// part-period threshold is <c>A / h</c>. When extending an order one period
/// earlier would add more than <c>A</c> of incremental holding cost, that
/// candidate and all still earlier candidates are dominated by opening an
/// additional setup. The backward predecessor scan can therefore stop.
/// </para>
/// <para>
/// The solver also applies the Wagner-Whitin Planning Horizon Theorem: once a
/// positive-demand prefix has an optimal last setup period, predecessor periods
/// before that setup need not be reconsidered for later prefixes.
/// </para>
/// <para>
/// Worst-case time remains <c>O(n^2)</c>, auxiliary working memory is
/// <c>O(n)</c>, and the actual number of predecessor evaluations is strongly
/// data dependent. Under favorable demand/cost ratios the scan length remains
/// small and practical execution can be close to linear.
/// </para>
/// <para>
/// Original reference:
/// R. B. Heady and Z. Zhu,
/// "An Improved Implementation of the Wagner-Whitin Algorithm",
/// Production and Operations Management 3(1), 55-63, 1994.
/// DOI: 10.1111/j.1937-5956.1994.tb00109.x.
/// </para>
/// <para>
/// Explicit fixed-cost Economic-Part-Period implementation reference:
/// S. J. Sadjadi, M. B. Gh. Aryanezhad and H. A. Sadeghi,
/// "An Improved WAGNER-WHITIN Algorithm",
/// International Journal of Industrial Engineering &amp; Production Research
/// 20(3), 117-123, 2009.
/// The paper gives the fixed-cost cutoff <c>DPP = A / H</c> and demonstrates
/// the branch-pruning procedure on a 12-period example.
/// </para>
/// <para>
/// Planning-horizon reference:
/// H. M. Wagner and T. M. Whitin,
/// "Dynamic Version of the Economic Lot Size Model",
/// Management Science 5(1), 89-96, 1958.
/// DOI: 10.1287/mnsc.5.1.89.
/// </para>
/// <para>
/// The implementation is a modern, allocation-conscious reconstruction of the
/// published algorithmic principles. It does not claim to be a transliteration
/// of the unavailable original Heady-Zhu program listing.
/// </para>
/// </remarks>
public sealed class HeadyZhuEconomicPartPeriodSolver : IUlsSolver
{
    private const int CancellationCheckMask = 255;

    /// <inheritdoc />
    public string Name =>
        "Heady-Zhu economic-part-period Wagner-Whitin";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <summary>
    /// Determines whether the fixed-cost assumptions required by this
    /// implementation hold.
    /// </summary>
    public static bool IsApplicable(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var horizon = problem.Horizon;
        var setupCosts = problem.SetupCosts;
        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        var setupCost = setupCosts[0];
        var productionCost = productionCosts[0];

        for (var period = 1; period < horizon; period++)
        {
            if (setupCosts[period] != setupCost ||
                productionCosts[period] != productionCost)
            {
                return false;
            }
        }

        // The final holding-cost entry is irrelevant when terminal inventory
        // is zero, so only transitions 0..horizon-2 need be constant.
        if (horizon > 1)
        {
            var holdingCost = holdingCosts[0];

            for (var period = 1; period < horizon - 1; period++)
            {
                if (holdingCosts[period] != holdingCost)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Returns the Economic-Part-Period threshold <c>A / h</c>.
    /// </summary>
    /// <remarks>
    /// Returns positive infinity when the relevant holding cost is zero.
    /// </remarks>
    public static double GetEconomicPartPeriodThreshold(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        if (!IsApplicable(problem))
        {
            throw new NotSupportedException(
                "The Economic-Part-Period threshold requires constant " +
                "setup, production and relevant holding costs.");
        }

        var holdingCost =
            problem.Horizon > 1
                ? problem.HoldingCosts[0]
                : 0.0;

        return holdingCost == 0.0
            ? double.PositiveInfinity
            : problem.SetupCosts[0] / holdingCost;
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">
    /// Thrown when setup costs, production costs, or economically relevant
    /// holding costs are not constant.
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
                "HeadyZhuEconomicPartPeriodSolver requires constant setup " +
                "costs, constant unit production costs and constant relevant " +
                "holding costs.");
        }

        var horizon = problem.Horizon;

        var valueBuffer =
            ArrayPool<double>.Shared.Rent(horizon + 1);

        var predecessorBuffer =
            ArrayPool<int>.Shared.Rent(horizon + 1);

        try
        {
            var value =
                valueBuffer.AsSpan(0, horizon + 1);

            var predecessor =
                predecessorBuffer.AsSpan(0, horizon + 1);

            value.Fill(double.PositiveInfinity);
            predecessor.Fill(-1);

            value[0] = 0.0;

            var demands = problem.Demands;
            var setupCost = problem.SetupCosts[0];
            var productionCost = problem.UnitProductionCosts[0];

            var holdingCost =
                horizon > 1
                    ? problem.HoldingCosts[0]
                    : 0.0;

            var planningHorizonStart = 0;

            for (var end = 0; end < horizon; end++)
            {
                if ((end & CancellationCheckMask) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (demands[end] == 0.0)
                {
                    // No setup is needed to extend a solved prefix through a
                    // zero-demand period. Do not advance the planning-horizon
                    // bound: this transition does not certify a new setup.
                    value[end + 1] = value[end];
                    predecessor[end + 1] = end;
                    continue;
                }

                var best = double.PositiveInfinity;
                var bestStart = -1;

                // Demand covered by the current candidate setup period through
                // 'end'. Before the first candidate it is empty.
                var segmentDemand = 0.0;

                // Holding cost of the current candidate interval.
                var intervalHoldingCost = 0.0;

                for (var start = end;
                     start >= planningHorizonStart;
                     start--)
                {
                    if (((end - start) & CancellationCheckMask) == 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (start < end)
                    {
                        // Moving the candidate setup one period earlier makes
                        // every unit already in the segment wait one additional
                        // period. This is the incremental part-period cost.
                        var incrementalHoldingCost = MultiplyFinite(
                            holdingCost,
                            segmentDemand,
                            "economic part-period incremental holding cost");

                        // Economic-Part-Period dominance:
                        // if one more period of carrying the already covered
                        // future demand costs more than opening an extra setup,
                        // this candidate is dominated by splitting at start+1.
                        // Earlier starts only increase the carried future
                        // demand, so the entire remaining scan can stop.
                        if (incrementalHoldingCost > setupCost)
                        {
                            break;
                        }

                        intervalHoldingCost = AddFinite(
                            intervalHoldingCost,
                            incrementalHoldingCost,
                            "candidate interval holding cost");
                    }

                    segmentDemand = AddFinite(
                        segmentDemand,
                        demands[start],
                        "candidate interval demand");

                    var variableProductionCost = MultiplyFinite(
                        productionCost,
                        segmentDemand,
                        "candidate production cost");

                    var candidate = AddFinite(
                        value[start],
                        setupCost,
                        "forward dynamic-programming candidate");

                    candidate = AddFinite(
                        candidate,
                        variableProductionCost,
                        "forward dynamic-programming candidate");

                    candidate = AddFinite(
                        candidate,
                        intervalHoldingCost,
                        "forward dynamic-programming candidate");

                    // Scanning from latest to earliest means strict comparison
                    // retains the latest optimal predecessor on a tie. This
                    // gives the strongest valid planning-horizon bound.
                    if (candidate < best)
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
                        $"No finite Heady-Zhu value was obtained for period {end}.");
                }

                value[end + 1] = best;
                predecessor[end + 1] = bestStart;

                // Wagner-Whitin Planning Horizon Theorem.
                planningHorizonStart = bestStart;
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
