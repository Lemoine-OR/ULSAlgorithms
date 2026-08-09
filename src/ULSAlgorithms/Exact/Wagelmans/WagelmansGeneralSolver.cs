using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.Wagelmans;

/// <summary>
/// Solves the general uncapacitated economic lot-sizing problem in
/// <c>O(n log n)</c> time using the backward geometric algorithm of
/// Wagelmans, van Hoesel and Kolen.
/// </summary>
/// <remarks>
/// <para>
/// The solver implements the backward dynamic-programming formulation of
/// Wagelmans, van Hoesel and Kolen (1992). Cumulative demand coordinates are
/// monotone, so candidate continuation states can be inserted into a lower
/// convex envelope with a simple array-backed stack. General transformed
/// production costs are not necessarily monotone; each query is therefore
/// located by binary search, yielding <c>O(n log n)</c> total time and
/// <c>O(n)</c> auxiliary memory.
/// </para>
/// <para>
/// The implementation uses the standard zero-holding-cost transformation
/// <c>r[t] = p[t] + sum(h[j], j=t..n-2)</c>. The last holding-cost coefficient
/// is omitted because terminal inventory is fixed to zero; omitting this common
/// additive shift does not change any minimizing production periods.
/// </para>
/// <para>
/// Original algorithmic source:
/// A. Wagelmans, S. van Hoesel, A. Kolen,
/// "Economic Lot Sizing: An O(n log n) Algorithm That Runs in Linear Time
/// in the Wagner-Whitin Case", Operations Research 40(S1), S145-S156, 1992.
/// DOI: 10.1287/opre.40.1.S145.
/// </para>
/// <para>
/// Implementation/data-structure source:
/// S. van Hoesel, A. Wagelmans, B. Moerman,
/// "Using Geometric Techniques to Improve Dynamic Programming Algorithms for
/// the Economic Lot-Sizing Problem and Extensions",
/// European Journal of Operational Research 75(2), 312-331, 1994.
/// DOI: 10.1016/0377-2217(94)90077-9.
/// </para>
/// <para>
/// The 1994 computational study reports that the backward geometric algorithm
/// is especially effective and emphasizes that only a stack plus binary search
/// is required. This C# implementation follows that backward formulation with
/// contiguous pooled arrays and no LINQ in the hot path.
/// </para>
/// <para>
/// The papers allow more general signed cost coefficients. The current
/// <see cref="UlsProblem"/> contract deliberately restricts input demand and
/// costs to finite non-negative values, so this implementation exposes the
/// corresponding non-negative-cost subset of the published model.
/// </para>
/// </remarks>
public sealed class WagelmansGeneralSolver : IUlsSolver
{
    private const int CancellationCheckMask = 255;

    /// <inheritdoc />
    public string Name => "Wagelmans general O(n log n)";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <inheritdoc />
    /// <exception cref="ArithmeticException">
    /// Thrown when a cumulative demand, transformed cost, line intersection,
    /// or dynamic-programming value is not representable as a finite
    /// <see cref="double"/>.
    /// </exception>
    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        var horizon = problem.Horizon;

        var prefixBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);
        var transformedCostBuffer = ArrayPool<double>.Shared.Rent(horizon);
        var valueBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);
        var successorBuffer = ArrayPool<int>.Shared.Rent(horizon);
        var hullBuffer = ArrayPool<HullLine>.Shared.Rent(horizon + 1);

        try
        {
            var prefixDemand = prefixBuffer.AsSpan(0, horizon + 1);
            var transformedCost = transformedCostBuffer.AsSpan(0, horizon);
            var value = valueBuffer.AsSpan(0, horizon + 1);
            var successor = successorBuffer.AsSpan(0, horizon);
            var hull = hullBuffer.AsSpan(0, horizon + 1);

            BuildPrefixDemand(problem.Demands, prefixDemand);
            BuildTransformedProductionCosts(
                problem.UnitProductionCosts,
                problem.HoldingCosts,
                transformedCost);

            value[horizon] = 0.0;
            successor.Fill(-1);

            var hullCount = 1;
            hull[0] = new HullLine(
                slope: prefixDemand[horizon],
                intercept: 0.0,
                startX: double.NegativeInfinity,
                successorPeriod: horizon);

            var setupCosts = problem.SetupCosts;
            var demands = problem.Demands;

            for (var period = horizon - 1; period >= 0; period--)
            {
                if ((period & CancellationCheckMask) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                var queryX = transformedCost[period];
                var bestLine = Query(hull, hullCount, queryX);

                var coveredDemand =
                    prefixDemand[bestLine.SuccessorPeriod] -
                    prefixDemand[period];

                var setupValue = AddFinite(
                    setupCosts[period],
                    MultiplyFinite(
                        queryX,
                        coveredDemand,
                        "transformed variable production cost"),
                    "setup plus transformed variable production cost");

                setupValue = AddFinite(
                    setupValue,
                    bestLine.Intercept,
                    "backward dynamic-programming value");

                if (demands[period] == 0.0 &&
                    value[period + 1] <= setupValue)
                {
                    value[period] = value[period + 1];
                    successor[period] = -1;
                }
                else
                {
                    value[period] = setupValue;
                    successor[period] = bestLine.SuccessorPeriod;
                }

                hullCount = AddLine(
                    hull,
                    hullCount,
                    new HullLine(
                        slope: prefixDemand[period],
                        intercept: value[period],
                        startX: double.NegativeInfinity,
                        successorPeriod: period));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return BuildResult(
                problem,
                prefixDemand,
                successor,
                cancellationToken);
        }
        finally
        {
            ArrayPool<double>.Shared.Return(prefixBuffer, clearArray: false);
            ArrayPool<double>.Shared.Return(transformedCostBuffer, clearArray: false);
            ArrayPool<double>.Shared.Return(valueBuffer, clearArray: false);
            ArrayPool<int>.Shared.Return(successorBuffer, clearArray: false);
            ArrayPool<HullLine>.Shared.Return(hullBuffer, clearArray: false);
        }
    }

    private static void BuildPrefixDemand(
        ReadOnlySpan<double> demands,
        Span<double> prefixDemand)
    {
        prefixDemand[0] = 0.0;

        for (var period = 0; period < demands.Length; period++)
        {
            prefixDemand[period + 1] = AddFinite(
                prefixDemand[period],
                demands[period],
                "cumulative demand");
        }
    }

    private static void BuildTransformedProductionCosts(
        ReadOnlySpan<double> productionCosts,
        ReadOnlySpan<double> holdingCosts,
        Span<double> transformedCosts)
    {
        var relevantHoldingSuffix = 0.0;

        for (var period = productionCosts.Length - 1;
             period >= 0;
             period--)
        {
            if (period < productionCosts.Length - 1)
            {
                relevantHoldingSuffix = AddFinite(
                    relevantHoldingSuffix,
                    holdingCosts[period],
                    "holding-cost suffix");
            }

            transformedCosts[period] = AddFinite(
                productionCosts[period],
                relevantHoldingSuffix,
                "transformed production cost");
        }
    }

    private static HullLine Query(
        ReadOnlySpan<HullLine> hull,
        int hullCount,
        double x)
    {
        var low = 0;
        var high = hullCount - 1;

        while (low < high)
        {
            var middle = low + ((high - low + 1) >> 1);

            if (hull[middle].StartX <= x)
            {
                low = middle;
            }
            else
            {
                high = middle - 1;
            }
        }

        return hull[low];
    }

    private static int AddLine(
        Span<HullLine> hull,
        int hullCount,
        HullLine newLine)
    {
        while (hullCount > 0)
        {
            var last = hull[hullCount - 1];

            if (newLine.Slope == last.Slope)
            {
                if (newLine.Intercept >= last.Intercept)
                {
                    return hullCount;
                }

                hullCount--;
                continue;
            }

            if (last.Slope <= newLine.Slope)
            {
                throw new InvalidOperationException(
                    "WagelmansGeneralSolver received nonmonotone cumulative-demand slopes.");
            }

            var startX = IntersectionX(last, newLine);

            if (hullCount == 1 || startX > last.StartX)
            {
                break;
            }

            hullCount--;
        }

        var activationX =
            hullCount == 0
                ? double.NegativeInfinity
                : IntersectionX(hull[hullCount - 1], newLine);

        hull[hullCount] = new HullLine(
            newLine.Slope,
            newLine.Intercept,
            activationX,
            newLine.SuccessorPeriod);

        return hullCount + 1;
    }

    private static double IntersectionX(
        HullLine left,
        HullLine right)
    {
        var denominator = left.Slope - right.Slope;

        if (!(denominator > 0.0) || !double.IsFinite(denominator))
        {
            throw new ArithmeticException(
                "A Wagelmans convex-envelope intersection has an invalid denominator.");
        }

        var numerator = right.Intercept - left.Intercept;
        var intersection = numerator / denominator;

        if (!double.IsFinite(intersection))
        {
            throw new ArithmeticException(
                "A Wagelmans convex-envelope intersection is not finite.");
        }

        return intersection;
    }

    private static UlsSolveResult BuildResult(
        UlsProblem problem,
        ReadOnlySpan<double> prefixDemand,
        ReadOnlySpan<int> successor,
        CancellationToken cancellationToken)
    {
        var horizon = problem.Horizon;
        var production = new double[horizon];
        var inventory = new double[horizon];
        var setup = new bool[horizon];

        var period = 0;

        while (period < horizon)
        {
            if ((period & CancellationCheckMask) == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            var next = successor[period];

            if (next < 0)
            {
                period++;
                continue;
            }

            if (next <= period || next > horizon)
            {
                throw new InvalidOperationException(
                    "The Wagelmans successor chain is inconsistent.");
            }

            production[period] =
                prefixDemand[next] - prefixDemand[period];

            setup[period] = production[period] > 0.0;
            period = next;
        }

        var setupCost = 0.0;
        var productionCost = 0.0;
        var holdingCost = 0.0;
        var runningInventory = 0.0;

        var demands = problem.Demands;
        var setupCosts = problem.SetupCosts;
        var productionCosts = problem.UnitProductionCosts;
        var holdingCosts = problem.HoldingCosts;

        for (period = 0; period < horizon; period++)
        {
            if ((period & CancellationCheckMask) == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (setup[period])
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
                    production[period],
                    "solution production cost"),
                "solution production cost");

            runningInventory = AddFinite(
                runningInventory,
                production[period],
                "inventory balance");

            runningInventory -= demands[period];

            var tolerance = 1e-10 * Math.Max(
                1.0,
                Math.Max(Math.Abs(runningInventory), Math.Abs(demands[period])));

            if (runningInventory < -tolerance)
            {
                throw new InvalidOperationException(
                    $"Reconstructed solution has negative inventory at period {period}.");
            }

            if (runningInventory < 0.0)
            {
                runningInventory = 0.0;
            }

            inventory[period] = runningInventory;

            holdingCost = AddFinite(
                holdingCost,
                MultiplyFinite(
                    holdingCosts[period],
                    inventory[period],
                    "solution holding cost"),
                "solution holding cost");
        }

        var solution = UlsSolution.FromOwnedBuffers(
            production,
            inventory,
            setup,
            setupCost,
            productionCost,
            holdingCost);

        return new UlsSolveResult(
            "Wagelmans general O(n log n)",
            UlsSolveStatus.Optimal,
            solution);
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
            int successorPeriod)
        {
            Slope = slope;
            Intercept = intercept;
            StartX = startX;
            SuccessorPeriod = successorPeriod;
        }

        public double Slope { get; }

        public double Intercept { get; }

        public double StartX { get; }

        public int SuccessorPeriod { get; }
    }
}
