using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.CuttingPlanes.Separation.Internal;
using ULSAlgorithms.Formulations;
using ULSAlgorithms.Models;

namespace ULSAlgorithms.CuttingPlanes.Separation;

/// <summary>
/// Exact combinatorial separation of the classical general ULS (l,S)
/// inequalities.
/// </summary>
/// <remarks>
/// <para>
/// For each prefix ending in l, the classical inequality is written as
///
/// sum(j in S) x[j] + sum(j in L\S) d[j,l] y[j] >= d[0,l].
///
/// For fixed l the most violated member is obtained independently for every
/// period j by choosing the smaller of x[j] and d[j,l] y[j]. Therefore one
/// candidate per l gives exact separation over the exponential S-family.
/// </para>
/// <para>
/// Time complexity: O(T^2). Additional working memory: O(T), excluding the
/// returned cuts.
/// </para>
/// <para>
/// Scientific source: I. Barany, T.J. Van Roy, L.A. Wolsey,
/// "Uncapacitated lot-sizing: the convex hull of solutions",
/// Mathematical Programming Study 22 (1984), 32-43,
/// DOI 10.1007/BFb0121006.
/// </para>
/// </remarks>
public sealed class GeneralLsCutSeparator :
    ILsCutSeparator
{
    /// <inheritdoc />
    public string Name =>
        "General exact (l,S) separator";

    /// <inheritdoc />
    public CutSeparationMethod Method =>
        CutSeparationMethod.General;

    /// <inheritdoc />
    public bool IsApplicable(
        UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);
        return true;
    }

    /// <inheritdoc />
    public IReadOnlyList<LsSeparatedCut> Separate(
        UlsProblem problem,
        UlsFormulation formulation,
        IReadOnlyDictionary<int, double> variableValues)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(formulation);
        ArgumentNullException.ThrowIfNull(variableValues);

        LsSeparationMath.RequireAggregateVariables(
            formulation);

        double[] cumulativeDemand =
            LsSeparationMath.BuildCumulativeDemand(
                problem);

        var cuts =
            new List<LsSeparatedCut>(
                problem.Horizon);

        for (int l = 0;
             l < problem.Horizon;
             l++)
        {
            double rhs =
                cumulativeDemand[l];

            if (rhs == 0.0)
            {
                continue;
            }

            var s =
                new List<int>(
                    l + 1);

            var coefficients =
                new List<CutCoefficient>(
                    l + 1);

            double lhs = 0.0;

            for (int period = 0;
                 period <= l;
                 period++)
            {
                double x =
                    LsSeparationMath.GetSemanticValue(
                        formulation.Variables.Production,
                        period,
                        variableValues,
                        "production");

                double y =
                    LsSeparationMath.GetSemanticValue(
                        formulation.Variables.Setup,
                        period,
                        variableValues,
                        "setup");

                double demandToL =
                    LsSeparationMath.IntervalDemand(
                        cumulativeDemand,
                        period,
                        l);

                double setupContribution =
                    demandToL * y;

                if (x <= setupContribution)
                {
                    s.Add(period);

                    coefficients.Add(
                        new CutCoefficient(
                            LsSeparationMath.GetVariableName(
                                formulation,
                                formulation.Variables.Production,
                                period,
                                "production"),
                            1.0));

                    lhs += x;
                }
                else
                {
                    if (demandToL != 0.0)
                    {
                        coefficients.Add(
                            new CutCoefficient(
                                LsSeparationMath.GetVariableName(
                                    formulation,
                                    formulation.Variables.Setup,
                                    period,
                                    "setup"),
                                demandToL));
                    }

                    lhs +=
                        setupContribution;
                }
            }

            if (coefficients.Count == 0)
            {
                continue;
            }

            double violation =
                rhs - lhs;

            var definition =
                new LsCutDefinition(
                    l,
                    s,
                    coefficients,
                    LinearConstraintSense.GreaterOrEqual,
                    rhs);

            cuts.Add(
                new LsSeparatedCut(
                    definition,
                    violation,
                    LsSeparationMath.Efficacy(
                        violation,
                        coefficients)));
        }

        return cuts;
    }
}
