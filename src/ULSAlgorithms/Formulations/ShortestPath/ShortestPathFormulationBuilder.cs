using ULSAlgorithms.Formulations.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Formulations.ShortestPath;

/// <summary>
/// Builds an acyclic regeneration-interval shortest-path formulation of ULS.
/// </summary>
/// <remarks>
/// <para>
/// A replenishment arc (t,j+1) represents one setup in period t serving all
/// demand from t through j. Zero-demand periods may also be crossed by a
/// zero-cost skip arc (t,t+1), which preserves correctness when demands contain
/// zeros.
/// </para>
/// <para>
/// Arc variables are continuous in [0,1]. The node-arc incidence matrix is a
/// network matrix, so an extreme optimal flow is integral.
/// </para>
/// <para>
/// This regeneration formulation requires the Wagner-Whitin/no-speculative-
/// motive condition p[t] + h[t] &gt;= p[t+1]. The network interpretation follows
/// Zangwill (1969), "A Backlogging Model and a Multi-Echelon Model of a Dynamic
/// Economic Lot Size Production System—A Network Approach", Management Science
/// 15(9), 506-527. Evans (1985), DOI 10.1016/0272-6963(85)90009-9, gives the
/// classical efficient Wagner-Whitin recursion.
/// </para>
/// </remarks>
public sealed class ShortestPathFormulationBuilder :
    IUlsFormulationBuilder
{
    /// <inheritdoc />
    public string Name =>
        "Regeneration-interval shortest-path formulation";

    /// <inheritdoc />
    public UlsFormulationKind Kind =>
        UlsFormulationKind.ShortestPath;

    /// <inheritdoc />
    public bool IsApplicable(
        UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        return UlsFormulationMath
            .IsNoSpeculativeMotive(problem);
    }

    /// <inheritdoc />
    public UlsFormulation Build(
        UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        if (!IsApplicable(problem))
        {
            throw new NotSupportedException(
                "ShortestPathFormulationBuilder requires " +
                "p[t] + h[t] >= p[t+1] for every adjacent period.");
        }

        int horizon =
            problem.Horizon;

        var model =
            new LinearModelBuilder();

        var arcs =
            new Dictionary<(int From, int To), int>();

        var outgoing =
            new List<(int To, int Variable)>[horizon + 1];

        var incoming =
            new List<(int From, int Variable)>[horizon + 1];

        for (int node = 0;
             node <= horizon;
             node++)
        {
            outgoing[node] = [];
            incoming[node] = [];
        }

        for (int start = 0;
             start < horizon;
             start++)
        {
            if (problem.Demands[start] == 0.0)
            {
                AddArc(
                    start,
                    start + 1,
                    0.0,
                    $"skip[{start},{start + 1}]");
            }

            double segmentDemand = 0.0;

            for (int end = start;
                 end < horizon;
                 end++)
            {
                segmentDemand =
                    UlsFormulationMath.AddFinite(
                        segmentDemand,
                        problem.Demands[end],
                        "shortest-path segment demand");

                if (segmentDemand == 0.0)
                {
                    continue;
                }

                AddArc(
                    start,
                    end + 1,
                    UlsFormulationMath.RegenerationArcCost(
                        problem,
                        start,
                        end),
                    $"z[{start},{end + 1}]");
            }
        }

        for (int node = 0;
             node <= horizon;
             node++)
        {
            var flow =
                new List<LinearTerm>(
                    outgoing[node].Count +
                    incoming[node].Count);

            foreach ((int _, int variable) in outgoing[node])
            {
                flow.Add(
                    new LinearTerm(
                        variable,
                        1.0));
            }

            foreach ((int _, int variable) in incoming[node])
            {
                flow.Add(
                    new LinearTerm(
                        variable,
                        -1.0));
            }

            double rhs =
                node == 0
                    ? 1.0
                    : node == horizon
                        ? -1.0
                        : 0.0;

            model.AddConstraint(
                $"flow[{node}]",
                flow,
                LinearConstraintSense.Equal,
                rhs);
        }

        return new UlsFormulation(
            Kind,
            "Regeneration network formulation; Zangwill (1969), Management " +
            "Science 15(9), 506-527; Evans (1985), DOI " +
            "10.1016/0272-6963(85)90009-9; classical formulation taxonomy in " +
            "Brahimi et al. (2006), DOI 10.1016/j.ejor.2004.01.054.",
            model.Build(
                "ULS-Shortest-Path"),
            new UlsFormulationVariableMap(
                arcs: arcs));

        void AddArc(
            int from,
            int to,
            double cost,
            string name)
        {
            int variable =
                model.AddVariable(
                    name,
                    LinearVariableType.Continuous,
                    0.0,
                    1.0);

            arcs.Add(
                (from, to),
                variable);

            outgoing[from].Add(
                (to, variable));

            incoming[to].Add(
                (from, variable));

            model.AddObjectiveTerm(
                variable,
                cost);
        }
    }
}
