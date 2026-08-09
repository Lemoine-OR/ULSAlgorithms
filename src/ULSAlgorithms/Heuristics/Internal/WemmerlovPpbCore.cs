using System.Buffers;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics.Internal;

/// <summary>
/// Shared implementation of the PPB variants analyzed by Wemmerlöv (1983).
/// </summary>
internal static class WemmerlovPpbCore
{
    public static UlsSolveResult Solve(
        UlsProblem problem,
        string solverName,
        double correctionFactor,
        bool useLookAheadLookBack,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        ClassicHeuristicGuard.ThrowIfNotStationary(
            problem,
            solverName);

        if (correctionFactor < 0.0 ||
            correctionFactor > 0.5 ||
            !double.IsFinite(correctionFactor))
        {
            throw new ArgumentOutOfRangeException(
                nameof(correctionFactor));
        }

        if (useLookAheadLookBack &&
            !HasStrictlyPositiveDemand(problem))
        {
            throw new NotSupportedException(
                $"{solverName} conservatively requires strictly positive " +
                "demand in every period when Look-Ahead/Look-Back is enabled.");
        }

        var horizon = problem.Horizon;
        var buffer = ArrayPool<int>.Shared.Rent(horizon);

        try
        {
            var cycleEnds = buffer.AsSpan(0, horizon);
            cycleEnds.Fill(-1);

            var demands = problem.Demands;
            var setupCost = problem.SetupCosts[0];

            var holdingCost =
                horizon > 1
                    ? problem.HoldingCosts[0]
                    : 0.0;

            var start =
                ClassicHeuristicGuard.FindNextPositiveDemand(
                    demands,
                    0);

            while (start < horizon)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var end = SelectPpbCycleEnd(
                    demands,
                    start,
                    setupCost,
                    holdingCost,
                    correctionFactor);

                if (useLookAheadLookBack)
                {
                    end = ApplyLookAheadLookBack(
                        demands,
                        start,
                        end,
                        setupCost,
                        holdingCost,
                        correctionFactor);
                }

                cycleEnds[start] = end;

                start =
                    ClassicHeuristicGuard.FindNextPositiveDemand(
                        demands,
                        end + 1);
            }

            return HeuristicSolutionBuilder.Build(
                problem,
                cycleEnds,
                solverName,
                cancellationToken);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(
                buffer,
                clearArray: false);
        }
    }

    private static int SelectPpbCycleEnd(
        ReadOnlySpan<double> demands,
        int start,
        double setupCost,
        double holdingCost,
        double correctionFactor)
    {
        if (holdingCost == 0.0)
        {
            return demands.Length - 1;
        }

        var epp = setupCost / holdingCost;

        var adjustedPartPeriods =
            MultiplyFinite(
                correctionFactor,
                demands[start],
                "corrected part-period quantity");

        var bestEnd = start;
        var bestDifference =
            Math.Abs(epp - adjustedPartPeriods);

        for (var end = start + 1;
             end < demands.Length;
             end++)
        {
            adjustedPartPeriods = AddFinite(
                adjustedPartPeriods,
                MultiplyFinite(
                    end - start + correctionFactor,
                    demands[end],
                    "corrected part-period quantity"),
                "corrected cumulative part-period quantity");

            var difference =
                Math.Abs(epp - adjustedPartPeriods);

            if (difference < bestDifference ||
                (difference == bestDifference &&
                 end > bestEnd))
            {
                bestDifference = difference;
                bestEnd = end;
            }

            if (adjustedPartPeriods >= epp)
            {
                break;
            }
        }

        return bestEnd;
    }

    private static int ApplyLookAheadLookBack(
        ReadOnlySpan<double> demands,
        int start,
        int end,
        double setupCost,
        double holdingCost,
        double correctionFactor)
    {
        var next = end + 1;

        if (next >= demands.Length)
        {
            return end;
        }

        var coverage = end - start + 1;

        // Figure 4 / Box 1:
        // compare the incremental holding cost of adding D[next] to the
        // current lot with the cost of opening the next replenishment.
        if (next + 1 < demands.Length)
        {
            var incrementalCurrentLotCost =
                MultiplyFinite(
                    holdingCost,
                    MultiplyFinite(
                        coverage,
                        demands[next],
                        "Look-Ahead gate quantity"),
                    "Look-Ahead gate cost");

            if (incrementalCurrentLotCost <= setupCost)
            {
                // Figure 4 / Box 2:
                // current next replenishment at 'next'
                // versus moving that replenishment one period forward.
                var currentPatternCost =
                    MultiplyFinite(
                        holdingCost,
                        AddFinite(
                            MultiplyFinite(
                                correctionFactor,
                                demands[next],
                                "Look-Ahead current cost"),
                            MultiplyFinite(
                                1.0 + correctionFactor,
                                demands[next + 1],
                                "Look-Ahead current cost"),
                            "Look-Ahead current cost"),
                        "Look-Ahead current cost");

                var shiftedPatternCost =
                    MultiplyFinite(
                        holdingCost,
                        AddFinite(
                            MultiplyFinite(
                                coverage + correctionFactor,
                                demands[next],
                                "Look-Ahead shifted cost"),
                            MultiplyFinite(
                                correctionFactor,
                                demands[next + 1],
                                "Look-Ahead shifted cost"),
                            "Look-Ahead shifted cost"),
                        "Look-Ahead shifted cost");

                if (shiftedPatternCost < currentPatternCost)
                {
                    return end + 1;
                }
            }
        }

        // Figure 4 / Box 3:
        // test whether the last requirement of the tentative lot should
        // instead be included in the next replenishment.
        if (end > start)
        {
            var currentPatternCost =
                MultiplyFinite(
                    holdingCost,
                    AddFinite(
                        MultiplyFinite(
                            coverage - 1.0 + correctionFactor,
                            demands[end],
                            "Look-Back current cost"),
                        MultiplyFinite(
                            correctionFactor,
                            demands[next],
                            "Look-Back current cost"),
                        "Look-Back current cost"),
                    "Look-Back current cost");

            var shiftedPatternCost =
                MultiplyFinite(
                    holdingCost,
                    AddFinite(
                        MultiplyFinite(
                            correctionFactor,
                            demands[end],
                            "Look-Back shifted cost"),
                        MultiplyFinite(
                            1.0 + correctionFactor,
                            demands[next],
                            "Look-Back shifted cost"),
                        "Look-Back shifted cost"),
                    "Look-Back shifted cost");

            if (shiftedPatternCost < currentPatternCost)
            {
                return end - 1;
            }
        }

        return end;
    }

    private static bool HasStrictlyPositiveDemand(
        UlsProblem problem)
    {
        var demands = problem.Demands;

        for (var period = 0;
             period < demands.Length;
             period++)
        {
            if (!(demands[period] > 0.0))
            {
                return false;
            }
        }

        return true;
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
