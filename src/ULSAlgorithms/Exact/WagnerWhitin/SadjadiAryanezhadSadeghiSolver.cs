using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.WagnerWhitin;

/// <summary>
/// Implements the fixed-cost improved Wagner-Whitin method of
/// Sadjadi, Aryanezhad and Sadeghi.
/// </summary>
/// <remarks>
/// <para>
/// The 2009 paper keeps the forward Wagner-Whitin recursion but avoids branches
/// once the cumulative future demand exceeds the Derived/Economic Part-Period
/// threshold <c>DPP = A / H</c>. It also uses the Planning Horizon Theorem.
/// </para>
/// <para>
/// This class is deliberately public and separate from
/// <see cref="HeadyZhuEconomicPartPeriodSolver"/> so that the 2009 publication
/// can be reproduced and benchmarked as its own method.
/// </para>
/// <para>
/// Applicability: constant setup cost, constant unit production cost, and
/// constant relevant unit holding cost. Worst-case time is <c>O(T^2)</c>,
/// auxiliary memory is <c>O(T)</c>, while the number of evaluated branches is
/// data dependent.
/// </para>
/// <para>
/// Reference:
/// S. J. Sadjadi, M. B. Gh. Aryanezhad and H. A. Sadeghi,
/// "An Improved WAGNER-WHITIN Algorithm",
/// International Journal of Industrial Engineering &amp; Production Research
/// 20(3), 117-123, 2009.
/// </para>
/// </remarks>
public sealed class SadjadiAryanezhadSadeghiSolver : IUlsSolver
{
    private const int CancellationCheckMask = 255;

    /// <inheritdoc />
    public string Name =>
        "Sadjadi-Aryanezhad-Sadeghi improved Wagner-Whitin";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <summary>
    /// Determines whether the fixed-cost assumptions of the published first
    /// model are satisfied.
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
    /// Gets the published <c>DPP = A/H</c> threshold.
    /// </summary>
    public static double GetDerivedPartPeriodThreshold(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        if (!IsApplicable(problem))
        {
            throw new NotSupportedException(
                "The Sadjadi DPP threshold requires constant setup, " +
                "production and relevant holding costs.");
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
    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsApplicable(problem))
        {
            throw new NotSupportedException(
                "SadjadiAryanezhadSadeghiSolver requires constant setup, " +
                "unit production and relevant holding costs.");
        }

        var horizon = problem.Horizon;
        var valueBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);
        var predecessorBuffer = ArrayPool<int>.Shared.Rent(horizon + 1);

        try
        {
            var value = valueBuffer.AsSpan(0, horizon + 1);
            var predecessor = predecessorBuffer.AsSpan(0, horizon + 1);

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
            var cancellationCounter = 0;

            for (var end = 0; end < horizon; end++)
            {
                if ((cancellationCounter++ & CancellationCheckMask) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (demands[end] == 0.0)
                {
                    value[end + 1] = value[end];
                    predecessor[end + 1] = end;
                    continue;
                }

                var best = double.PositiveInfinity;
                var bestStart = -1;
                var futureDemand = 0.0;
                var intervalHoldingCost = 0.0;

                for (var start = end;
                     start >= planningHorizonStart;
                     start--)
                {
                    if ((cancellationCounter++ & CancellationCheckMask) == 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (start < end)
                    {
                        var incrementalHoldingCost =
                            MultiplyFinite(
                                holdingCost,
                                futureDemand,
                                "DPP incremental holding cost");

                        // This is the paper's DPP = A/H branch-elimination
                        // criterion written without division.
                        if (incrementalHoldingCost > setupCost)
                        {
                            break;
                        }

                        intervalHoldingCost = AddFinite(
                            intervalHoldingCost,
                            incrementalHoldingCost,
                            "candidate holding cost");
                    }

                    futureDemand = AddFinite(
                        futureDemand,
                        demands[start],
                        "candidate demand");

                    var candidate = AddFinite(
                        value[start],
                        setupCost,
                        "candidate cost");

                    candidate = AddFinite(
                        candidate,
                        MultiplyFinite(
                            productionCost,
                            futureDemand,
                            "candidate production cost"),
                        "candidate cost");

                    candidate = AddFinite(
                        candidate,
                        intervalHoldingCost,
                        "candidate cost");

                    // Backward scan + strict comparison retains the latest
                    // optimal predecessor, strengthening the planning horizon.
                    if (candidate < best)
                    {
                        best = candidate;
                        bestStart = start;
                    }
                }

                if (!double.IsFinite(best) ||
                    bestStart < planningHorizonStart)
                {
                    throw new ArithmeticException(
                        $"No finite Sadjadi value was obtained for period {end}.");
                }

                value[end + 1] = best;
                predecessor[end + 1] = bestStart;
                planningHorizonStart = bestStart;
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
