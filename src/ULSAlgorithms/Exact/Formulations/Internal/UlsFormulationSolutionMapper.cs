using ULSAlgorithms.Formulations;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.Formulations.Internal;

internal static class UlsFormulationSolutionMapper
{
    internal static UlsSolution Map(
        UlsProblem problem,
        UlsFormulation formulation,
        IReadOnlyDictionary<int, double> values,
        double zeroTolerance,
        double feasibilityTolerance)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(formulation);
        ArgumentNullException.ThrowIfNull(values);

        var production =
            new double[problem.Horizon];

        var inventory =
            new double[problem.Horizon];

        var setup =
            new bool[problem.Horizon];

        switch (formulation.Kind)
        {
            case UlsFormulationKind.AggregateInventory:
                MapAggregate(
                    problem,
                    formulation,
                    values,
                    production,
                    inventory,
                    setup,
                    feasibilityTolerance);
                break;

            case UlsFormulationKind.FacilityLocation:
                MapFacilityLocation(
                    problem,
                    formulation,
                    values,
                    production,
                    inventory,
                    setup,
                    zeroTolerance,
                    feasibilityTolerance);
                break;

            case UlsFormulationKind.ShortestPath:
                MapShortestPath(
                    problem,
                    formulation,
                    values,
                    production,
                    inventory,
                    setup,
                    zeroTolerance,
                    feasibilityTolerance);
                break;

            case UlsFormulationKind.InventoryEliminated:
                MapInventoryEliminated(
                    problem,
                    formulation,
                    values,
                    production,
                    inventory,
                    setup,
                    feasibilityTolerance);
                break;

            default:
                throw new NotSupportedException(
                    $"Unsupported ULS formulation kind '{formulation.Kind}'.");
        }

        ComputeCostComponents(
            problem,
            production,
            inventory,
            setup,
            out double setupCost,
            out double productionCost,
            out double holdingCost);

        return UlsSolution.FromOwnedBuffers(
            production,
            inventory,
            setup,
            setupCost,
            productionCost,
            holdingCost);
    }

    private static void MapAggregate(
        UlsProblem problem,
        UlsFormulation formulation,
        IReadOnlyDictionary<int, double> values,
        double[] production,
        double[] inventory,
        bool[] setup,
        double feasibilityTolerance)
    {
        for (int period = 0;
             period < problem.Horizon;
             period++)
        {
            production[period] =
                CleanNonnegative(
                    GetValue(
                        formulation.Variables.Production,
                        period,
                        values,
                        "production"),
                    feasibilityTolerance);

            inventory[period] =
                CleanNonnegative(
                    GetValue(
                        formulation.Variables.Inventory,
                        period,
                        values,
                        "inventory"),
                    feasibilityTolerance);

            setup[period] =
                GetBinaryDecision(
                    formulation.Variables.Setup,
                    period,
                    values,
                    "setup");
        }
    }

    private static void MapFacilityLocation(
        UlsProblem problem,
        UlsFormulation formulation,
        IReadOnlyDictionary<int, double> values,
        double[] production,
        double[] inventory,
        bool[] setup,
        double zeroTolerance,
        double feasibilityTolerance)
    {
        foreach (KeyValuePair<(int First, int Second), int> entry in
                 formulation.Variables.Disaggregated)
        {
            double quantity =
                CleanNonnegative(
                    GetValue(
                        values,
                        entry.Value,
                        $"q[{entry.Key.First},{entry.Key.Second}]"),
                    feasibilityTolerance);

            production[entry.Key.First] +=
                quantity;
        }

        for (int period = 0;
             period < problem.Horizon;
             period++)
        {
            if (Math.Abs(production[period]) <= zeroTolerance)
            {
                production[period] = 0.0;
            }

            setup[period] =
                GetBinaryDecision(
                    formulation.Variables.Setup,
                    period,
                    values,
                    "setup");
        }

        ReconstructInventory(
            problem,
            production,
            inventory,
            zeroTolerance,
            feasibilityTolerance);
    }

    private static void MapInventoryEliminated(
        UlsProblem problem,
        UlsFormulation formulation,
        IReadOnlyDictionary<int, double> values,
        double[] production,
        double[] inventory,
        bool[] setup,
        double feasibilityTolerance)
    {
        for (int period = 0;
             period < problem.Horizon;
             period++)
        {
            production[period] =
                CleanNonnegative(
                    GetValue(
                        formulation.Variables.Production,
                        period,
                        values,
                        "production"),
                    feasibilityTolerance);

            setup[period] =
                GetBinaryDecision(
                    formulation.Variables.Setup,
                    period,
                    values,
                    "setup");
        }

        ReconstructInventory(
            problem,
            production,
            inventory,
            zeroTolerance: feasibilityTolerance,
            feasibilityTolerance);
    }

    private static void MapShortestPath(
        UlsProblem problem,
        UlsFormulation formulation,
        IReadOnlyDictionary<int, double> values,
        double[] production,
        double[] inventory,
        bool[] setup,
        double zeroTolerance,
        double feasibilityTolerance)
    {
        int node = 0;
        int steps = 0;

        while (node < problem.Horizon)
        {
            if (++steps > problem.Horizon + 1)
            {
                throw new InvalidOperationException(
                    "The shortest-path solution contains an invalid cycle.");
            }

            var outgoing =
                formulation.Variables.Arcs
                    .Where(
                        entry =>
                            entry.Key.From == node)
                    .Select(
                        entry =>
                            new
                            {
                                entry.Key.To,
                                Value =
                                    GetValue(
                                        values,
                                        entry.Value,
                                        $"arc[{entry.Key.From},{entry.Key.To}]")
                            })
                    .Where(
                        candidate =>
                            candidate.Value > zeroTolerance)
                    .OrderByDescending(
                        candidate =>
                            candidate.Value)
                    .ThenBy(
                        candidate =>
                            candidate.To)
                    .ToArray();

            if (outgoing.Length == 0)
            {
                throw new InvalidOperationException(
                    $"No positive outgoing shortest-path arc exists from node {node}.");
            }

            int next =
                outgoing[0].To;

            if (next <= node ||
                next > problem.Horizon)
            {
                throw new InvalidOperationException(
                    $"Invalid shortest-path arc ({node},{next}).");
            }

            double segmentDemand = 0.0;

            for (int demandPeriod = node;
                 demandPeriod < next;
                 demandPeriod++)
            {
                segmentDemand +=
                    problem.Demands[demandPeriod];
            }

            if (segmentDemand > zeroTolerance)
            {
                production[node] +=
                    segmentDemand;

                setup[node] = true;
            }

            node = next;
        }

        ReconstructInventory(
            problem,
            production,
            inventory,
            zeroTolerance,
            feasibilityTolerance);
    }

    private static void ReconstructInventory(
        UlsProblem problem,
        double[] production,
        double[] inventory,
        double zeroTolerance,
        double feasibilityTolerance)
    {
        double previous = 0.0;

        for (int period = 0;
             period < problem.Horizon;
             period++)
        {
            double value =
                previous +
                production[period] -
                problem.Demands[period];

            if (Math.Abs(value) <= zeroTolerance)
            {
                value = 0.0;
            }

            inventory[period] =
                CleanNonnegative(
                    value,
                    feasibilityTolerance);

            previous =
                inventory[period];
        }
    }

    private static void ComputeCostComponents(
        UlsProblem problem,
        double[] production,
        double[] inventory,
        bool[] setup,
        out double setupCost,
        out double productionCost,
        out double holdingCost)
    {
        setupCost = 0.0;
        productionCost = 0.0;
        holdingCost = 0.0;

        for (int period = 0;
             period < problem.Horizon;
             period++)
        {
            if (setup[period])
            {
                setupCost +=
                    problem.SetupCosts[period];
            }

            productionCost +=
                production[period] *
                problem.UnitProductionCosts[period];

            holdingCost +=
                inventory[period] *
                problem.HoldingCosts[period];
        }

        if (!double.IsFinite(setupCost) ||
            !double.IsFinite(productionCost) ||
            !double.IsFinite(holdingCost))
        {
            throw new ArithmeticException(
                "A reconstructed ULS cost component is not finite.");
        }
    }

    private static double GetValue(
        IReadOnlyDictionary<int, int> semanticMap,
        int period,
        IReadOnlyDictionary<int, double> values,
        string family)
    {
        if (!semanticMap.TryGetValue(
                period,
                out int variableId))
        {
            throw new InvalidOperationException(
                $"The formulation contains no {family} variable for period {period}.");
        }

        return GetValue(
            values,
            variableId,
            $"{family}[{period}]");
    }

    private static double GetValue(
        IReadOnlyDictionary<int, double> values,
        int variableId,
        string description)
    {
        if (!values.TryGetValue(
                variableId,
                out double value) ||
            !double.IsFinite(value))
        {
            throw new InvalidOperationException(
                $"No finite solver value exists for {description} " +
                $"(variable id {variableId}).");
        }

        return value;
    }

    private static bool GetBinaryDecision(
        IReadOnlyDictionary<int, int> semanticMap,
        int period,
        IReadOnlyDictionary<int, double> values,
        string family)
    {
        double value =
            GetValue(
                semanticMap,
                period,
                values,
                family);

        if (value == 0.0)
        {
            return false;
        }

        if (value == 1.0)
        {
            return true;
        }

        throw new InvalidOperationException(
            $"{family}[{period}] was not normalized to 0 or 1: {value:G17}.");
    }

    private static double CleanNonnegative(
        double value,
        double feasibilityTolerance)
    {
        if (!double.IsFinite(value))
        {
            throw new InvalidOperationException(
                "A reconstructed ULS decision is not finite.");
        }

        if (value >= 0.0)
        {
            return value;
        }

        if (Math.Abs(value) <= feasibilityTolerance)
        {
            return 0.0;
        }

        throw new InvalidOperationException(
            $"A reconstructed ULS decision is materially negative: {value:G17}.");
    }
}
