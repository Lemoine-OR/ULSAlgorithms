using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.WagnerWhitin;

/// <summary>
/// Solves ULS instances with Wagner-Whitin costs in linear time.
/// </summary>
/// <remarks>
/// <para>
/// This implementation is an exact solver. It does not use the classical
/// <c>O(n^2)</c> Wagner-Whitin dynamic-programming scan. Instead, it implements
/// the monotone lower-convex-envelope specialization described by
/// Wagelmans, van Hoesel and Kolen (1992), which runs in <c>O(n)</c> time for
/// the Wagner-Whitin/no-speculative-motive case and uses <c>O(n)</c> memory.
/// </para>
/// <para>
/// The supported condition is
/// <c>p[t] + h[t] &gt;= p[t+1]</c> for every adjacent pair of periods.
/// Equivalently, the transformed marginal production costs are nonincreasing
/// over time. The classical case with constant unit production costs and
/// nonnegative holding costs is a special case.
/// </para>
/// <para>
/// Algorithmic source:
/// A. Wagelmans, S. van Hoesel, A. Kolen,
/// "Economic Lot Sizing: An O(n log n) Algorithm That Runs in Linear Time
/// in the Wagner-Whitin Case", Operations Research 40(S1), S145-S156, 1992,
/// DOI: 10.1287/opre.40.1.S145.
/// </para>
/// <para>
/// Historical references:
/// H. M. Wagner and T. M. Whitin,
/// "Dynamic Version of the Economic Lot Size Model", Management Science
/// 5(1), 89-96, 1958, DOI: 10.1287/mnsc.5.1.89;
/// J. R. Evans,
/// "An Efficient Implementation of the Wagner-Whitin Algorithm for Dynamic
/// Lot-Sizing", Journal of Operations Management 5(2), 229-235, 1985,
/// DOI: 10.1016/0272-6963(85)90009-9.
/// </para>
/// <para>
/// The C# implementation uses an equivalent monotone convex-hull form of the
/// backward recurrence. Internal work arrays are rented from
/// <see cref="ArrayPool{T}"/> to reduce garbage-collector pressure when the
/// solver is repeatedly used as a subproblem.
/// </para>
/// </remarks>
public sealed class WagnerWhitinSolver : IUlsSolver
{
    private const int CancellationCheckMask = 255;

    /// <inheritdoc />
    public string Name => "Wagner-Whitin (Wagelmans linear-time)";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <summary>
    /// Determines whether the problem satisfies the no-speculative-motive
    /// condition required by the linear-time specialization.
    /// </summary>
    /// <param name="problem">The problem to inspect.</param>
    /// <returns>
    /// <see langword="true"/> when
    /// <c>p[t] + h[t] &gt;= p[t+1]</c> for all adjacent periods;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool IsApplicable(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        for (var period = 0; period < problem.Horizon - 1; period++)
        {
            var currentDeliveredNextPeriod =
                productionCosts[period] + holdingCosts[period];

            if (!double.IsFinite(currentDeliveredNextPeriod) ||
                currentDeliveredNextPeriod < productionCosts[period + 1])
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">
    /// Thrown when the problem violates the Wagner-Whitin/no-speculative-motive
    /// cost condition required by this linear-time solver.
    /// </exception>
    /// <exception cref="ArithmeticException">
    /// Thrown when a cumulative demand or intermediate cost cannot be
    /// represented as a finite <see cref="double"/>.
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
                "WagnerWhitinSolver requires p[t] + h[t] >= p[t+1] " +
                "for every adjacent pair of periods.");
        }

        var horizon = problem.Horizon;
        var demands = problem.Demands;
        var setupCosts = problem.SetupCosts;
        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        var suffixBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);
        var nextBuffer = ArrayPool<int>.Shared.Rent(horizon);
        var hullBuffer = ArrayPool<HullLine>.Shared.Rent(horizon + 1);

        try
        {
            var suffixDemand = suffixBuffer.AsSpan(0, horizon + 1);
            var next = nextBuffer.AsSpan(0, horizon);
            var hull = hullBuffer.AsSpan(0, horizon + 1);

            BuildSuffixDemand(demands, suffixDemand);

            var head = 0;
            var tail = 0;
            hull[0] = new HullLine(
                slope: 0.0,
                intercept: 0.0,
                startX: double.NegativeInfinity,
                period: horizon);

            var nextValue = 0.0;
            var accumulatedRelevantHoldingCost = 0.0;

            for (var period = horizon - 1; period >= 0; period--)
            {
                if ((period & CancellationCheckMask) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                // The terminal holding-cost coefficient is irrelevant because
                // terminal inventory is fixed at zero. Omitting it is a common
                // additive shift of the transformed marginal costs.
                if (period < horizon - 1)
                {
                    accumulatedRelevantHoldingCost = AddFinite(
                        accumulatedRelevantHoldingCost,
                        holdingCosts[period],
                        "transformed holding-cost suffix");
                }

                var transformedMarginalCost = AddFinite(
                    productionCosts[period],
                    accumulatedRelevantHoldingCost,
                    "transformed marginal production cost");

                AdvanceQueryHead(
                    hull,
                    ref head,
                    tail,
                    transformedMarginalCost);

                var bestLine = hull[head];
                var coveredDemand =
                    suffixDemand[period] - suffixDemand[bestLine.Period];

                var setupValue = AddFinite(
                    setupCosts[period],
                    MultiplyFinite(
                        transformedMarginalCost,
                        coveredDemand,
                        "transformed variable production cost"),
                    "setup plus transformed variable cost");

                setupValue = AddFinite(
                    setupValue,
                    bestLine.Intercept,
                    "dynamic-programming value");

                double value;

                if (demands[period] == 0.0 && nextValue <= setupValue)
                {
                    value = nextValue;
                    next[period] = -1;
                }
                else
                {
                    value = setupValue;
                    next[period] = bestLine.Period;
                }

                nextValue = value;

                AddLine(
                    hull,
                    ref head,
                    ref tail,
                    new HullLine(
                        slope: -suffixDemand[period],
                        intercept: value,
                        startX: double.NegativeInfinity,
                        period: period));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return BuildResult(
                problem,
                suffixDemand,
                next,
                cancellationToken);
        }
        finally
        {
            ArrayPool<double>.Shared.Return(suffixBuffer, clearArray: false);
            ArrayPool<int>.Shared.Return(nextBuffer, clearArray: false);
            ArrayPool<HullLine>.Shared.Return(hullBuffer, clearArray: false);
        }
    }

    private UlsSolveResult BuildResult(
        UlsProblem problem,
        ReadOnlySpan<double> suffixDemand,
        ReadOnlySpan<int> next,
        CancellationToken cancellationToken)
    {
        var horizon = problem.Horizon;
        var productionQuantities = new double[horizon];
        var endingInventories = new double[horizon];
        var setupDecisions = new bool[horizon];

        var period = 0;

        while (period < horizon)
        {
            if ((period & CancellationCheckMask) == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            var successor = next[period];

            if (successor < 0)
            {
                period++;
                continue;
            }

            if (successor <= period || successor > horizon)
            {
                throw new InvalidOperationException(
                    "The Wagner-Whitin predecessor chain is inconsistent.");
            }

            productionQuantities[period] =
                suffixDemand[period] - suffixDemand[successor];
            setupDecisions[period] = true;

            for (var inventoryPeriod = period;
                 inventoryPeriod < successor;
                 inventoryPeriod++)
            {
                endingInventories[inventoryPeriod] =
                    suffixDemand[inventoryPeriod + 1] -
                    suffixDemand[successor];
            }

            period = successor;
        }

        var setupCost = 0.0;
        var productionCost = 0.0;
        var holdingCost = 0.0;

        var setupCosts = problem.SetupCosts;
        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        for (period = 0; period < horizon; period++)
        {
            if ((period & CancellationCheckMask) == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (setupDecisions[period])
            {
                setupCost = AddFinite(
                    setupCost,
                    setupCosts[period],
                    "solution setup cost");
            }

            productionCost = AddFinite(
                productionCost,
                MultiplyFinite(
                    productionCosts[period],
                    productionQuantities[period],
                    "solution production cost"),
                "solution production cost");

            holdingCost = AddFinite(
                holdingCost,
                MultiplyFinite(
                    holdingCosts[period],
                    endingInventories[period],
                    "solution holding cost"),
                "solution holding cost");
        }

        var solution = UlsSolution.FromOwnedBuffers(
            productionQuantities,
            endingInventories,
            setupDecisions,
            setupCost,
            productionCost,
            holdingCost);

        return new UlsSolveResult(
            Name,
            UlsSolveStatus.Optimal,
            solution);
    }

    private static void BuildSuffixDemand(
        ReadOnlySpan<double> demands,
        Span<double> suffixDemand)
    {
        suffixDemand[demands.Length] = 0.0;

        for (var period = demands.Length - 1; period >= 0; period--)
        {
            suffixDemand[period] = AddFinite(
                suffixDemand[period + 1],
                demands[period],
                "cumulative demand");
        }
    }

    private static void AdvanceQueryHead(
        ReadOnlySpan<HullLine> hull,
        ref int head,
        int tail,
        double x)
    {
        while (head < tail && hull[head + 1].StartX <= x)
        {
            head++;
        }
    }

    private static void AddLine(
        Span<HullLine> hull,
        ref int head,
        ref int tail,
        HullLine newLine)
    {
        while (tail >= head)
        {
            var last = hull[tail];

            if (newLine.Slope == last.Slope)
            {
                if (newLine.Intercept >= last.Intercept)
                {
                    return;
                }

                tail--;
                continue;
            }

            if (last.Slope <= newLine.Slope)
            {
                throw new InvalidOperationException(
                    "The Wagner-Whitin convex hull received nonmonotone slopes.");
            }

            var startX = IntersectionX(last, newLine);

            if (tail == head || startX > last.StartX)
            {
                break;
            }

            tail--;
        }

        if (tail < head)
        {
            tail = head;
            hull[tail] = new HullLine(
                newLine.Slope,
                newLine.Intercept,
                double.NegativeInfinity,
                newLine.Period);
            return;
        }

        var intersection = IntersectionX(hull[tail], newLine);

        tail++;
        hull[tail] = new HullLine(
            newLine.Slope,
            newLine.Intercept,
            intersection,
            newLine.Period);
    }

    private static double IntersectionX(
        HullLine left,
        HullLine right)
    {
        var denominator = left.Slope - right.Slope;

        if (!(denominator > 0.0) || !double.IsFinite(denominator))
        {
            throw new ArithmeticException(
                "A convex-hull line intersection has an invalid denominator.");
        }

        var numerator = right.Intercept - left.Intercept;
        var intersection = numerator / denominator;

        if (!double.IsFinite(intersection))
        {
            throw new ArithmeticException(
                "A convex-hull line intersection is not finite.");
        }

        return intersection;
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

    private readonly struct HullLine
    {
        public HullLine(
            double slope,
            double intercept,
            double startX,
            int period)
        {
            Slope = slope;
            Intercept = intercept;
            StartX = startX;
            Period = period;
        }

        public double Slope { get; }

        public double Intercept { get; }

        public double StartX { get; }

        public int Period { get; }
    }
}
