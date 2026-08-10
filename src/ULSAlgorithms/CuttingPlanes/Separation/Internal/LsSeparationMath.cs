using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.Formulations;
using ULSAlgorithms.Models;

namespace ULSAlgorithms.CuttingPlanes.Separation.Internal;

internal static class LsSeparationMath
{
    internal static double[] BuildCumulativeDemand(
        UlsProblem problem)
    {
        var cumulative =
            new double[problem.Horizon];

        double running = 0.0;

        for (int period = 0;
             period < problem.Horizon;
             period++)
        {
            running +=
                problem.Demands[period];

            if (!double.IsFinite(running))
            {
                throw new ArithmeticException(
                    "Cumulative demand overflowed.");
            }

            cumulative[period] =
                running;
        }

        return cumulative;
    }

    internal static double IntervalDemand(
        double[] cumulativeDemand,
        int first,
        int last)
    {
        if (first < 0 ||
            last < first ||
            last >= cumulativeDemand.Length)
        {
            throw new ArgumentOutOfRangeException();
        }

        return cumulativeDemand[last] -
               (first == 0
                   ? 0.0
                   : cumulativeDemand[first - 1]);
    }

    internal static double GetSemanticValue(
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
                $"The formulation has no {family} variable for period {period}.");
        }

        if (!values.TryGetValue(
                variableId,
                out double value) ||
            !double.IsFinite(value))
        {
            throw new InvalidOperationException(
                $"No finite LP value exists for {family}[{period}].");
        }

        return value;
    }

    internal static string GetVariableName(
        UlsFormulation formulation,
        IReadOnlyDictionary<int, int> semanticMap,
        int period,
        string family)
    {
        if (!semanticMap.TryGetValue(
                period,
                out int variableId))
        {
            throw new InvalidOperationException(
                $"The formulation has no {family} variable for period {period}.");
        }

        return formulation.Model
            .GetVariable(variableId)
            .Name;
    }

    internal static double Efficacy(
        double violation,
        IEnumerable<CutCoefficient> coefficients)
    {
        double squaredNorm = 0.0;

        foreach (CutCoefficient coefficient in coefficients)
        {
            squaredNorm +=
                coefficient.Coefficient *
                coefficient.Coefficient;
        }

        return squaredNorm > 0.0
            ? violation /
              Math.Sqrt(squaredNorm)
            : 0.0;
    }

    internal static bool HasWagnerWhitinCosts(
        UlsProblem problem)
    {
        for (int period = 0;
             period < problem.Horizon - 1;
             period++)
        {
            double left =
                problem.UnitProductionCosts[period] +
                problem.HoldingCosts[period];

            double right =
                problem.UnitProductionCosts[period + 1];

            double scale =
                Math.Max(
                    1.0,
                    Math.Max(
                        Math.Abs(left),
                        Math.Abs(right)));

            if (left + 1.0e-12 * scale <
                right)
            {
                return false;
            }
        }

        return true;
    }

    internal static void RequireAggregateVariables(
        UlsFormulation formulation)
    {
        if (formulation.Variables.Production.Count == 0 ||
            formulation.Variables.Setup.Count == 0)
        {
            throw new ArgumentException(
                "An (l,S) separator requires aggregate production and setup " +
                "variable mappings.",
                nameof(formulation));
        }
    }
}
