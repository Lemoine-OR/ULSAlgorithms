using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.CuttingPlanes.Separation.Internal;
using ULSAlgorithms.Formulations;
using ULSAlgorithms.Models;

namespace ULSAlgorithms.CuttingPlanes.Separation;

/// <summary>
/// Separates the O(T^2) Wagner-Whitin specialization of the classical ULS
/// (l,S) inequalities.
/// </summary>
/// <remarks>
/// <para>
/// Under Wagner-Whitin costs, the relevant prefix-S inequalities can be written
/// equivalently as
///
/// I[k-1] + sum(j=k..l) d[j,l] y[j] >= d[k,l],
///
/// for 0 &lt;= k &lt;= l, with zero initial inventory.
/// </para>
/// <para>
/// In the canonical (l,S) representation this corresponds to
/// S = {0,...,k-1}. All (k,l) candidates are evaluated in O(T^2) time using
/// cumulative production and backward weighted-setup sums.
/// </para>
/// <para>
/// Scientific source: Y. Pochet, L.A. Wolsey,
/// "Polyhedra for lot-sizing with Wagner-Whitin costs",
/// Mathematical Programming 67 (1994), 297-323,
/// DOI 10.1007/BF01582225.
/// </para>
/// </remarks>
public sealed class WagnerWhitinLsCutSeparator :
    ILsCutSeparator
{
    /// <inheritdoc />
    public string Name =>
        "Wagner-Whitin (l,S) separator";

    /// <inheritdoc />
    public CutSeparationMethod Method =>
        CutSeparationMethod.WagnerWhitin;

    /// <inheritdoc />
    public bool IsApplicable(
        UlsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        return LsSeparationMath.HasWagnerWhitinCosts(
            problem);
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

        if (!IsApplicable(problem))
        {
            throw new NotSupportedException(
                "The Wagner-Whitin separator requires " +
                "p[t] + h[t] >= p[t+1] for every adjacent period.");
        }

        LsSeparationMath.RequireAggregateVariables(
            formulation);

        double[] cumulativeDemand =
            LsSeparationMath.BuildCumulativeDemand(
                problem);

        var x =
            new double[problem.Horizon];

        var y =
            new double[problem.Horizon];

        for (int period = 0;
             period < problem.Horizon;
             period++)
        {
            x[period] =
                LsSeparationMath.GetSemanticValue(
                    formulation.Variables.Production,
                    period,
                    variableValues,
                    "production");

            y[period] =
                LsSeparationMath.GetSemanticValue(
                    formulation.Variables.Setup,
                    period,
                    variableValues,
                    "setup");
        }

        var prefixProduction =
            new double[problem.Horizon + 1];

        for (int period = 0;
             period < problem.Horizon;
             period++)
        {
            prefixProduction[period + 1] =
                prefixProduction[period] +
                x[period];
        }

        var cuts =
            new List<LsSeparatedCut>(
                problem.Horizon *
                (problem.Horizon + 1) /
                2);

        var suffixWeightedSetup =
            new double[problem.Horizon + 1];

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

            suffixWeightedSetup[l + 1] =
                0.0;

            for (int period = l;
                 period >= 0;
                 period--)
            {
                double demandToL =
                    LsSeparationMath.IntervalDemand(
                        cumulativeDemand,
                        period,
                        l);

                suffixWeightedSetup[period] =
                    suffixWeightedSetup[period + 1] +
                    demandToL *
                    y[period];
            }

            for (int k = 0;
                 k <= l;
                 k++)
            {
                double lhs =
                    prefixProduction[k] +
                    suffixWeightedSetup[k];

                double violation =
                    rhs - lhs;

                var s =
                    Enumerable
                        .Range(
                            0,
                            k)
                        .ToArray();

                var coefficients =
                    new List<CutCoefficient>(
                        l + 1);

                for (int period = 0;
                     period < k;
                     period++)
                {
                    coefficients.Add(
                        new CutCoefficient(
                            LsSeparationMath.GetVariableName(
                                formulation,
                                formulation.Variables.Production,
                                period,
                                "production"),
                            1.0));
                }

                for (int period = k;
                     period <= l;
                     period++)
                {
                    double demandToL =
                        LsSeparationMath.IntervalDemand(
                            cumulativeDemand,
                            period,
                            l);

                    if (demandToL == 0.0)
                    {
                        continue;
                    }

                    coefficients.Add(
                        new CutCoefficient(
                            LsSeparationMath.GetVariableName(
                                formulation,
                                formulation.Variables.Setup,
                                period,
                                "setup"),
                            demandToL));
                }

                if (coefficients.Count == 0)
                {
                    continue;
                }

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
        }

        return cuts;
    }
}
