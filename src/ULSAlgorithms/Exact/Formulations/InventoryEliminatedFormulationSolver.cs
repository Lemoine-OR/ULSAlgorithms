using ULSAlgorithms.Formulations.InventoryEliminated;
using ULSAlgorithms.Optimization.Execution;

namespace ULSAlgorithms.Exact.Formulations;

/// <summary>
/// Exact solver-backed ULS strategy using the aggregate formulation in which
/// inventory variables are algebraically eliminated.
/// </summary>
public sealed class InventoryEliminatedFormulationSolver :
    SolverBackedUlsFormulationSolverBase
{
    /// <summary>Initializes with automatic optimization-engine selection.</summary>
    public InventoryEliminatedFormulationSolver(
        LinearModelSolveOptions? executionOptions = null)
        : base(
            "Inventory-eliminated formulation",
            new InventoryEliminatedFormulationBuilder(),
            executionOptions: executionOptions)
    {
    }

    /// <summary>Initializes with an injected portable model solver.</summary>
    public InventoryEliminatedFormulationSolver(
        LinearModelSolver modelSolver,
        LinearModelSolveOptions? executionOptions = null)
        : base(
            "Inventory-eliminated formulation",
            new InventoryEliminatedFormulationBuilder(),
            modelSolver,
            executionOptions)
    {
    }
}
