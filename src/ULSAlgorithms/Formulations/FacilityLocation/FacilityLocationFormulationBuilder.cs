using ULSAlgorithms.Formulations.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Formulations.FacilityLocation;

/// <summary>
/// Builds the classical disaggregated/facility-location formulation of ULS.
/// </summary>
/// <remarks>
/// <para>
/// q[t,k] is the amount of demand in period k supplied by a setup in period t,
/// with t &lt;= k. Demand is assigned exactly once and q[t,k] &lt;= d[k] y[t].
/// The delivered unit cost contains production cost in t plus holding from t
/// through k-1.
/// </para>
/// <para>
/// The facility-location connection for economic lot sizing is classically
/// associated with Krarup and Bilde (1977), "Plant location, Set Covering and
/// Economic Lot Size", DOI 10.1007/978-3-0348-5936-3_10. The formulation
/// taxonomy is also reviewed by Brahimi et al. (2006),
/// DOI 10.1016/j.ejor.2004.01.054.
/// </para>
/// </remarks>
public sealed class FacilityLocationFormulationBuilder :
    IUlsFormulationBuilder
{
    /// <inheritdoc />
    public string Name =>
        "Disaggregated facility-location formulation";

    /// <inheritdoc />
    public UlsFormulationKind Kind =>
        UlsFormulationKind.FacilityLocation;

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

        var model =
            new LinearModelBuilder();

        var setup =
            new Dictionary<int, int>();

        var q =
            new Dictionary<(int First, int Second), int>();

        for (int period = 0;
             period < horizon;
             period++)
        {
            int y =
                model.AddVariable(
                    $"y[{period}]",
                    LinearVariableType.Binary,
                    0.0,
                    1.0);

            setup.Add(period, y);

            model.AddObjectiveTerm(
                y,
                problem.SetupCosts[period]);
        }

        for (int demandPeriod = 0;
             demandPeriod < horizon;
             demandPeriod++)
        {
            double demand =
                problem.Demands[demandPeriod];

            if (demand == 0.0)
            {
                continue;
            }

            var assignment =
                new List<LinearTerm>(
                    demandPeriod + 1);

            for (int productionPeriod = 0;
                 productionPeriod <= demandPeriod;
                 productionPeriod++)
            {
                int variable =
                    model.AddVariable(
                        $"q[{productionPeriod},{demandPeriod}]",
                        LinearVariableType.Continuous,
                        0.0,
                        demand);

                q.Add(
                    (productionPeriod, demandPeriod),
                    variable);

                assignment.Add(
                    new LinearTerm(
                        variable,
                        1.0));

                model.AddConstraint(
                    $"facility-link[{productionPeriod},{demandPeriod}]",
                    [
                        new LinearTerm(
                            variable,
                            1.0),
                        new LinearTerm(
                            setup[productionPeriod],
                            -demand)
                    ],
                    LinearConstraintSense.LessOrEqual,
                    0.0);

                model.AddObjectiveTerm(
                    variable,
                    UlsFormulationMath.DeliveredUnitCost(
                        problem,
                        productionPeriod,
                        demandPeriod));
            }

            model.AddConstraint(
                $"demand[{demandPeriod}]",
                assignment,
                LinearConstraintSense.Equal,
                demand);
        }

        return new UlsFormulation(
            Kind,
            "Disaggregated/facility-location ULS formulation; Krarup & Bilde " +
            "(1977), DOI 10.1007/978-3-0348-5936-3_10; taxonomy in Brahimi " +
            "et al. (2006), DOI 10.1016/j.ejor.2004.01.054.",
            model.Build(
                "ULS-Facility-Location"),
            new UlsFormulationVariableMap(
                setup: setup,
                disaggregated: q));
    }
}
