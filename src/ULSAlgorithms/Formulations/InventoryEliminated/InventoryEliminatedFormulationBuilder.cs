using ULSAlgorithms.Formulations.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Formulations.InventoryEliminated;

/// <summary>
/// Builds an exact aggregate ULS formulation after algebraically eliminating
/// end-of-period inventory variables.
/// </summary>
/// <remarks>
/// <para>
/// Inventory is reconstructed from cumulative production:
/// I[t] = sum(i=0..t) x[i] - sum(i=0..t) d[i].
/// Nonnegative inventory becomes a cumulative-production inequality, and final
/// zero inventory becomes equality of total production and total demand.
/// </para>
/// <para>
/// This is the fourth classical formulation family summarized by Brahimi,
/// Dauzère-Pérès, Najid and Nordli (2006), "Single item lot sizing problems",
/// EJOR 168(1), 1-16, DOI 10.1016/j.ejor.2004.01.054.
/// </para>
/// </remarks>
public sealed class InventoryEliminatedFormulationBuilder :
    IUlsFormulationBuilder
{
    /// <inheritdoc />
    public string Name =>
        "Inventory-eliminated aggregate formulation";

    /// <inheritdoc />
    public UlsFormulationKind Kind =>
        UlsFormulationKind.InventoryEliminated;

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

        double[] cumulativeDemand =
            UlsFormulationMath.BuildCumulativeDemand(problem);

        var model =
            new LinearModelBuilder();

        var production =
            new Dictionary<int, int>();
        var setup =
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

            production.Add(period, x);
            setup.Add(period, y);

            model.AddObjectiveTerm(
                y,
                problem.SetupCosts[period]);

            double xCoefficient =
                problem.UnitProductionCosts[period];

            for (int inventoryPeriod = period;
                 inventoryPeriod < horizon - 1;
                 inventoryPeriod++)
            {
                xCoefficient =
                    UlsFormulationMath.AddFinite(
                        xCoefficient,
                        problem.HoldingCosts[inventoryPeriod],
                        "inventory-eliminated production coefficient");
            }

            model.AddObjectiveTerm(
                x,
                xCoefficient);

            model.AddConstraint(
                $"setup-link[{period}]",
                [
                    new LinearTerm(
                        x,
                        1.0),
                    new LinearTerm(
                        y,
                        -suffixDemand[period])
                ],
                LinearConstraintSense.LessOrEqual,
                0.0);
        }

        for (int period = 0;
             period < horizon - 1;
             period++)
        {
            var cumulativeProduction =
                new List<LinearTerm>(
                    period + 1);

            for (int source = 0;
                 source <= period;
                 source++)
            {
                cumulativeProduction.Add(
                    new LinearTerm(
                        production[source],
                        1.0));
            }

            model.AddConstraint(
                $"cumulative-demand[{period}]",
                cumulativeProduction,
                LinearConstraintSense.GreaterOrEqual,
                cumulativeDemand[period]);
        }

        model.AddConstraint(
            "total-demand",
            Enumerable
                .Range(
                    0,
                    horizon)
                .Select(
                    period =>
                        new LinearTerm(
                            production[period],
                            1.0)),
            LinearConstraintSense.Equal,
            problem.TotalDemand);

        double objectiveConstant = 0.0;

        for (int period = 0;
             period < horizon - 1;
             period++)
        {
            objectiveConstant =
                UlsFormulationMath.AddFinite(
                    objectiveConstant,
                    -UlsFormulationMath.MultiplyFinite(
                        problem.HoldingCosts[period],
                        cumulativeDemand[period],
                        "inventory-eliminated objective constant"),
                    "inventory-eliminated objective constant");
        }

        return new UlsFormulation(
            Kind,
            "Inventory-eliminated classical ULS formulation; taxonomy and " +
            "algebraic form summarized in Brahimi et al. (2006), DOI " +
            "10.1016/j.ejor.2004.01.054.",
            model.Build(
                "ULS-Inventory-Eliminated",
                objectiveConstant),
            new UlsFormulationVariableMap(
                production,
                setup));
    }
}
