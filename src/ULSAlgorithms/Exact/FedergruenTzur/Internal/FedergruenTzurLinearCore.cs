using System.Buffers;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.FedergruenTzur.Internal;

/// <summary>
/// Shared allocation-conscious forward recurrence for the two Federgruen-Tzur
/// linear-time specializations.
/// </summary>
internal static class FedergruenTzurLinearCore
{
    private const int CancellationCheckMask = 255;

    public static UlsSolveResult SolveNoSpeculativeMotive(
        UlsProblem problem,
        string solverName,
        CancellationToken cancellationToken)
    {
        return Solve(
            problem,
            solverName,
            FedergruenTzurLinearMode.NoSpeculativeMotive,
            cancellationToken);
    }

    public static UlsSolveResult SolveNondecreasingSetupCosts(
        UlsProblem problem,
        string solverName,
        CancellationToken cancellationToken)
    {
        return Solve(
            problem,
            solverName,
            FedergruenTzurLinearMode.NondecreasingSetupCosts,
            cancellationToken);
    }

    private static UlsSolveResult Solve(
        UlsProblem problem,
        string solverName,
        FedergruenTzurLinearMode mode,
        CancellationToken cancellationToken)
    {
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
                new FedergruenTzurLinearCandidateDeque(horizon);

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

                // Federgruen-Tzur:
                // C(i) = c_i - H(i-1).
                var transformedVariableCost =
                    productionCosts[period] -
                    cumulativeHoldingBefore;

                EnsureFinite(
                    transformedVariableCost,
                    "transformed variable production cost");

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

                var shouldInsert = candidates.IsEmpty;

                if (!shouldInsert)
                {
                    shouldInsert =
                        mode == FedergruenTzurLinearMode.NoSpeculativeMotive ||
                        transformedVariableCost < candidates.LastSlope;
                }

                if (shouldInsert)
                {
                    candidates.AddMonotone(
                        period,
                        transformedVariableCost,
                        intercept);
                }

                var bestPeriod =
                    candidates.GetBestAndDiscardPast(
                        cumulativeDemand);

                var bestLineValue = AddFinite(
                    candidates.BestIntercept,
                    MultiplyFinite(
                        candidates.BestSlope,
                        cumulativeDemand,
                        "candidate line evaluation"),
                    "candidate line evaluation");

                var orderValue = AddFinite(
                    firstPeriodHoldingCost,
                    bestLineValue,
                    "forward dynamic-programming value");

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
                solverName,
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

    private enum FedergruenTzurLinearMode
    {
        NoSpeculativeMotive,
        NondecreasingSetupCosts
    }
}
