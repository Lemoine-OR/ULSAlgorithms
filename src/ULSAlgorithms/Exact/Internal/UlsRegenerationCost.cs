using ULSAlgorithms.Models;

namespace ULSAlgorithms.Exact.Internal;

/// <summary>
/// O(1) regeneration-interval cost evaluator for the uncapacitated
/// zero-inventory-ordering structure.
/// </summary>
internal sealed class UlsRegenerationCost
{
    private readonly UlsProblem _problem;
    private readonly double[] _demandPrefix;
    private readonly double[] _holdingPrefix;
    private readonly double[] _weightedDemandPrefix;

    public UlsRegenerationCost(UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        _problem = problem;

        var horizon = problem.Horizon;

        _demandPrefix = new double[horizon + 1];
        _holdingPrefix = new double[horizon + 1];
        _weightedDemandPrefix = new double[horizon + 1];

        var demands = problem.Demands;
        var holdingCosts = problem.HoldingCosts;

        for (var period = 0; period < horizon; period++)
        {
            _demandPrefix[period + 1] = AddFinite(
                _demandPrefix[period],
                demands[period],
                "cumulative demand");

            _weightedDemandPrefix[period + 1] = AddFinite(
                _weightedDemandPrefix[period],
                MultiplyFinite(
                    demands[period],
                    _holdingPrefix[period],
                    "demand-weighted holding prefix"),
                "demand-weighted holding prefix");

            _holdingPrefix[period + 1] = AddFinite(
                _holdingPrefix[period],
                holdingCosts[period],
                "cumulative holding cost");
        }
    }

    /// <summary>
    /// Gets the cost of one replenishment in <paramref name="start"/>
    /// satisfying demand through <paramref name="endInclusive"/>.
    /// </summary>
    public double GetCost(
        int start,
        int endInclusive)
    {
        if ((uint)start >= (uint)_problem.Horizon ||
            endInclusive < start ||
            endInclusive >= _problem.Horizon)
        {
            throw new ArgumentOutOfRangeException();
        }

        var endExclusive = endInclusive + 1;

        var segmentDemand =
            _demandPrefix[endExclusive] -
            _demandPrefix[start];

        if (segmentDemand == 0.0)
        {
            return 0.0;
        }

        var transformedUnitCost =
            _problem.UnitProductionCosts[start] -
            _holdingPrefix[start];

        var variableCost = AddFinite(
            MultiplyFinite(
                transformedUnitCost,
                segmentDemand,
                "regeneration variable cost"),
            _weightedDemandPrefix[endExclusive] -
            _weightedDemandPrefix[start],
            "regeneration variable cost");

        return AddFinite(
            _problem.SetupCosts[start],
            variableCost,
            "regeneration interval cost");
    }

    public double GetDemand(
        int start,
        int endInclusive)
    {
        if ((uint)start >= (uint)_problem.Horizon ||
            endInclusive < start ||
            endInclusive >= _problem.Horizon)
        {
            throw new ArgumentOutOfRangeException();
        }

        return
            _demandPrefix[endInclusive + 1] -
            _demandPrefix[start];
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
