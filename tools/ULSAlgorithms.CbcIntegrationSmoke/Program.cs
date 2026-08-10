using ULSAlgorithms.Catalog;
using ULSAlgorithms.Models;
using ULSAlgorithms.Optimization;
using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Results;

const double ExpectedObjective = 680.0;
const double RelativeTolerance = 1.0e-8;

var problem =
    new UlsProblem(
        demands:
            [20.0, 30.0, 25.0, 40.0],
        setupCosts:
            [200.0, 200.0, 200.0, 200.0],
        unitProductionCosts:
            [0.0, 0.0, 0.0, 0.0],
        holdingCosts:
            [4.0, 4.0, 4.0, 0.0]);

var oracle =
    UlsSolverFactory
        .Create("adaptive-exact")
        .Solve(problem);

var oracleObjective =
    RequireOptimalFiniteObjective(
        "adaptive-exact",
        oracle);

AssertClose(
    "adaptive-exact published smoke objective",
    ExpectedObjective,
    oracleObjective);

var solverBackedIds =
    new[]
    {
        "aggregate-inventory-formulation",
        "facility-location-formulation",
        "shortest-path-formulation",
        "inventory-eliminated-formulation",
        "general-ls-cutting-plane",
        "wagner-whitin-ls-cutting-plane"
    };

foreach (var solverId in solverBackedIds)
{
    var solver =
        UlsSolverFactory.Create(
            solverId,
            new UlsSolverCreationOptions
            {
                OptimizationExecution =
                    new LinearModelSolveOptions
                    {
                        Solver =
                            SolverKind.CoinOrCbc,
                        AllowFallbackWhenExplicit =
                            false
                    }
            });

    var result =
        solver.Solve(problem);

    var objective =
        RequireOptimalFiniteObjective(
            solverId,
            result);

    AssertClose(
        solverId,
        oracleObjective,
        objective);

    RequireCoinOrCbcProvenance(
        solverId,
        result);

    Console.WriteLine(
        $"{solverId}: Optimal, objective = {objective:R}, engine = COIN-OR CBC");
}

Console.WriteLine(
    $"CBC end-to-end qualification passed for {solverBackedIds.Length} solver-backed strategies.");

static double RequireOptimalFiniteObjective(
    string solverId,
    UlsSolveResult result)
{
    if (result.Status != UlsSolveStatus.Optimal)
    {
        throw new InvalidOperationException(
            $"Solver '{solverId}' returned status '{result.Status}' instead of Optimal. " +
            $"Message: {result.Message}");
    }

    if (result.Solution is null)
    {
        throw new InvalidOperationException(
            $"Solver '{solverId}' reported Optimal without a ULS solution.");
    }

    var objective =
        result.ObjectiveValue;

    if (!objective.HasValue ||
        !double.IsFinite(objective.Value))
    {
        throw new InvalidOperationException(
            $"Solver '{solverId}' did not return a finite objective value.");
    }

    return objective.Value;
}

static void RequireCoinOrCbcProvenance(
    string solverId,
    UlsSolveResult result)
{
    switch (result)
    {
        case SolverBackedUlsSolveResult formulation:
            if (formulation.OptimizationSolver?.SelectedSolver !=
                SolverKind.CoinOrCbc)
            {
                throw new InvalidOperationException(
                    $"Solver '{solverId}' did not record COIN-OR CBC as the selected engine.");
            }

            break;

        case CuttingPlaneUlsSolveResult cuttingPlane:
            if (cuttingPlane.FinalModelExecution.Solver?.SelectedSolver !=
                    SolverKind.CoinOrCbc ||
                cuttingPlane.CuttingPlaneExecution.Solver.SelectedSolver !=
                    SolverKind.CoinOrCbc)
            {
                throw new InvalidOperationException(
                    $"Solver '{solverId}' did not record COIN-OR CBC for both root/final execution provenance.");
            }

            break;

        default:
            throw new InvalidOperationException(
                $"Solver '{solverId}' returned unexpected result type '{result.GetType().FullName}'.");
    }
}

static void AssertClose(
    string name,
    double expected,
    double actual)
{
    var tolerance =
        RelativeTolerance *
        Math.Max(
            1.0,
            Math.Abs(expected));

    if (Math.Abs(expected - actual) > tolerance)
    {
        throw new InvalidOperationException(
            $"{name} objective mismatch. Expected {expected:R}, got {actual:R}, tolerance {tolerance:R}.");
    }
}
