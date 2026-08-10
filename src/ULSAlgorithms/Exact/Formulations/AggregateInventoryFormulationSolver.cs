using ULSAlgorithms.Formulations.Aggregate;
using ULSAlgorithms.Optimization.Execution;

namespace ULSAlgorithms.Exact.Formulations;

/// <summary>
/// Exact solver-backed ULS strategy using the classical aggregate
/// production/setup/inventory formulation.
/// </summary>
public sealed class AggregateInventoryFormulationSolver :
    SolverBackedUlsFormulationSolverBase
{
    /// <summary>Initializes with automatic optimization-engine selection.</summary>
    public AggregateInventoryFormulationSolver(
        LinearModelSolveOptions? executionOptions = null)
        : base(
            "Aggregate inventory formulation",
            new AggregateInventoryFormulationBuilder(),
            executionOptions: executionOptions)
    {
    }

    /// <summary>Initializes with an injected portable model solver.</summary>
    public AggregateInventoryFormulationSolver(
        LinearModelSolver modelSolver,
        LinearModelSolveOptions? executionOptions = null)
        : base(
            "Aggregate inventory formulation",
            new AggregateInventoryFormulationBuilder(),
            modelSolver,
            executionOptions)
    {
    }
}
