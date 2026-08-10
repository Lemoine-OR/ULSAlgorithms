using ULSAlgorithms.Formulations.ShortestPath;
using ULSAlgorithms.Optimization.Execution;

namespace ULSAlgorithms.Exact.Formulations;

/// <summary>
/// Exact solver-backed ULS strategy using the regeneration shortest-path
/// formulation.
/// </summary>
/// <remarks>
/// Applicability is restricted to the no-speculative-motive / Wagner-Whitin
/// cost condition required by the underlying formulation builder.
/// </remarks>
public sealed class ShortestPathFormulationSolver :
    SolverBackedUlsFormulationSolverBase
{
    /// <summary>Initializes with automatic optimization-engine selection.</summary>
    public ShortestPathFormulationSolver(
        LinearModelSolveOptions? executionOptions = null)
        : base(
            "Shortest-path formulation",
            new ShortestPathFormulationBuilder(),
            executionOptions: executionOptions)
    {
    }

    /// <summary>Initializes with an injected portable model solver.</summary>
    public ShortestPathFormulationSolver(
        LinearModelSolver modelSolver,
        LinearModelSolveOptions? executionOptions = null)
        : base(
            "Shortest-path formulation",
            new ShortestPathFormulationBuilder(),
            modelSolver,
            executionOptions)
    {
    }
}
