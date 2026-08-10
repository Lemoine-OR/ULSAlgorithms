using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.Optimization.Modeling;
using ModelConstraintSense =
    ULSAlgorithms.Optimization.Modeling.LinearConstraintSense;
using CutConstraintSense =
    ULSAlgorithms.CuttingPlanes.LinearConstraintSense;

namespace ULSAlgorithms.CuttingPlanes.Internal;

internal static class LsCutModelBuilder
{
    internal static LinearModel CreateLpRelaxation(
        LinearModel source)
    {
        ArgumentNullException.ThrowIfNull(source);

        LinearVariable[] variables =
            source.Variables
                .Select(
                    static variable =>
                        new LinearVariable(
                            variable.Id,
                            variable.Name,
                            variable.Type ==
                            LinearVariableType.Binary
                                ? LinearVariableType.Continuous
                                : variable.Type,
                            variable.LowerBound,
                            variable.UpperBound))
                .ToArray();

        return new LinearModel(
            source.Name + "-LP",
            variables,
            source.Constraints,
            source.Objective);
    }

    internal static LinearModel AddCuts(
        LinearModel source,
        IEnumerable<(string Name, LsCutDefinition Cut)> cuts)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(cuts);

        var nameToVariable =
            source.Variables.ToDictionary(
                static variable =>
                    variable.Name,
                StringComparer.Ordinal);

        var constraints =
            source.Constraints.ToList();

        foreach ((string name, LsCutDefinition cut) in cuts)
        {
            var terms =
                new List<LinearTerm>(
                    cut.Coefficients.Count);

            foreach (CutCoefficient coefficient in
                     cut.Coefficients)
            {
                if (!nameToVariable.TryGetValue(
                        coefficient.VariableName,
                        out LinearVariable? variable))
                {
                    throw new InvalidOperationException(
                        $"Cut '{name}' references unknown variable " +
                        $"'{coefficient.VariableName}'.");
                }

                terms.Add(
                    new LinearTerm(
                        variable.Id,
                        coefficient.Coefficient));
            }

            constraints.Add(
                new LinearConstraint(
                    name,
                    terms,
                    TranslateSense(
                        cut.Sense),
                    cut.RightHandSide));
        }

        return new LinearModel(
            source.Name,
            source.Variables,
            constraints,
            source.Objective);
    }

    private static ModelConstraintSense TranslateSense(
        CutConstraintSense sense)
    {
        return sense switch
        {
            CutConstraintSense.LessOrEqual =>
                ModelConstraintSense.LessOrEqual,

            CutConstraintSense.Equal =>
                ModelConstraintSense.Equal,

            CutConstraintSense.GreaterOrEqual =>
                ModelConstraintSense.GreaterOrEqual,

            _ =>
                throw new NotSupportedException(
                    $"Unsupported cut sense '{sense}'.")
        };
    }
}
