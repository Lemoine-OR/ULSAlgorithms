using ULSAlgorithms.Formulations.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Formulations.Aggregate;

/// <summary>
/// Builds the classical aggregate ULS mixed-integer formulation with production,
/// setup and end-of-period inventory variables.
/// </summary>
/// <remarks>
/// <para>
/// The formulation uses inventory-balance equalities and the tight period-wise
/// uncapacitated big-M bound x[t] &lt;= D[t..T-1] y[t].
/// </para>
/// <para>
/// Literature context: the classical ULS model originates with Wagner and
/// Whitin (1958). The aggregate/disaggregate/shortest-path/inventory-eliminated
/// taxonomy is summarized by Brahimi, Dauzère-Pérès, Najid and Nordli (2006),
/// "Single item lot sizing problems", EJOR 168(1), 1-16,
/// DOI 10.1016/j.ejor.2004.01.054.
/// </para>
/// </remarks>
public sealed class AggregateInventoryFormulationBuilder :
    IUlsFormulationBuilder
{
    /// <inheritdoc />
    public string Name =>
        "Aggregate inventory-balance formulation";

    /// <inheritdoc />
    public UlsFormulationKind Kind =>
        UlsFormulationKind.AggregateInventory;

    /// <inheritdoc />
    public bool IsApplicable(
        UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);
        return true;
    }

    /// <inheritdoc />
    public UlsFormulation Build(
        UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        int horizon =
            problem.Horizon;

        double[] suffixDemand =
            UlsFormulationMath.BuildSuffixDemand(problem);

        var model =
            new LinearModelBuilder();

        var production =
            new Dictionary<int, int>();
        var setup =
            new Dictionary<int, int>();
        var inventory =
            new Dictionary<int, int>();

        for (int period = 0;
             period < horizon;
             period++)
        {
            int x =
                model.AddVariable(
                    $"x[{period}]",
                    LinearVariableType.Continuous,
                    0.0,
                    suffixDemand[period]);

            int y =
                model.AddVariable(
                    $"y[{period}]",
                    LinearVariableType.Binary,
                    0.0,
                    1.0);

            double inventoryUpperBound =
                period == horizon - 1
                    ? 0.0
                    : suffixDemand[period + 1];

            int i =
                model.AddVariable(
                    $"I[{period}]",
                    LinearVariableType.Continuous,
                    0.0,
                    inventoryUpperBound);

            production.Add(period, x);
            setup.Add(period, y);
            inventory.Add(period, i);

            model.AddObjectiveTerm(
                x,
                problem.UnitProductionCosts[period]);

            model.AddObjectiveTerm(
                y,
                problem.SetupCosts[period]);

            model.AddObjectiveTerm(
                i,
                problem.HoldingCosts[period]);
        }

        for (int period = 0;
             period < horizon;
             period++)
        {
            var balance =
                new List<LinearTerm>();

            if (period > 0)
            {
                balance.Add(
                    new LinearTerm(
                        inventory[period - 1],
                        1.0));
            }

            balance.Add(
                new LinearTerm(
                    production[period],
                    1.0));

            balance.Add(
                new LinearTerm(
                    inventory[period],
                    -1.0));

            model.AddConstraint(
                $"balance[{period}]",
                balance,
                LinearConstraintSense.Equal,
                problem.Demands[period]);

            model.AddConstraint(
                $"setup-link[{period}]",
                [
                    new LinearTerm(
                        production[period],
                        1.0),
                    new LinearTerm(
                        setup[period],
                        -suffixDemand[period])
                ],
                LinearConstraintSense.LessOrEqual,
                0.0);
        }

        return new UlsFormulation(
            Kind,
            "Classical aggregate ULS formulation; Wagner & Whitin (1958), " +
            "DOI 10.1287/mnsc.5.1.89; formulation taxonomy in Brahimi et al. " +
            "(2006), DOI 10.1016/j.ejor.2004.01.054.",
            model.Build(
                "ULS-Aggregate-Inventory"),
            new UlsFormulationVariableMap(
                production,
                setup,
                inventory));
    }
}
