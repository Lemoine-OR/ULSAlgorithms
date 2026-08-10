using ULSAlgorithms.Catalog;
using ULSAlgorithms.Models;

var configuration =
    UlsSolverConfiguration.ParseJson(
        """
        {
          "schemaVersion": 1,
          "solverId": "adaptive-exact",
          "options": {}
        }
        """);

var problem =
    new UlsProblem(
        demands:
            [10.0, 20.0, 5.0, 15.0],
        setupCosts:
            [80.0, 80.0, 80.0, 80.0],
        unitProductionCosts:
            [0.0, 0.0, 0.0, 0.0],
        holdingCosts:
            [2.0, 2.0, 2.0, 0.0]);

var result =
    configuration
        .CreateSolver()
        .Solve(problem);

var objectiveValue = result.ObjectiveValue;

if (result.Solution is null ||
    !objectiveValue.HasValue ||
    !double.IsFinite(objectiveValue.Value))
{
    throw new InvalidOperationException(
        "Portable adaptive exact smoke solve did not produce a finite solution.");
}

Console.WriteLine(
    $"ULSAlgorithms portability smoke passed. Objective = {objectiveValue.Value:R}");