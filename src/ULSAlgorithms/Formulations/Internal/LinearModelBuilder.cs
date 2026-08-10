using ULSAlgorithms.Optimization.Modeling;

namespace ULSAlgorithms.Formulations.Internal;

internal sealed class LinearModelBuilder
{
    private readonly List<LinearVariable> _variables = [];
    private readonly List<LinearConstraint> _constraints = [];
    private readonly List<LinearTerm> _objectiveTerms = [];

    internal int AddVariable(
        string name,
        LinearVariableType type,
        double lowerBound,
        double upperBound)
    {
        int id =
            _variables.Count;

        _variables.Add(
            new LinearVariable(
                id,
                name,
                type,
                lowerBound,
                upperBound));

        return id;
    }

    internal void AddConstraint(
        string name,
        IEnumerable<LinearTerm> terms,
        LinearConstraintSense sense,
        double rightHandSide)
    {
        _constraints.Add(
            new LinearConstraint(
                name,
                terms,
                sense,
                rightHandSide));
    }

    internal void AddObjectiveTerm(
        int variableId,
        double coefficient)
    {
        if (coefficient == 0.0)
        {
            return;
        }

        _objectiveTerms.Add(
            new LinearTerm(
                variableId,
                coefficient));
    }

    internal LinearModel Build(
        string name,
        double objectiveConstant = 0.0)
    {
        LinearTerm[] combinedObjective =
            _objectiveTerms
                .GroupBy(
                    static term =>
                        term.VariableId)
                .Select(
                    group =>
                        new LinearTerm(
                            group.Key,
                            group.Sum(
                                static term =>
                                    term.Coefficient)))
                .Where(
                    static term =>
                        term.Coefficient != 0.0)
                .OrderBy(
                    static term =>
                        term.VariableId)
                .ToArray();

        return new LinearModel(
            name,
            _variables,
            _constraints,
            new LinearObjective(
                combinedObjective,
                objectiveConstant));
    }
}
