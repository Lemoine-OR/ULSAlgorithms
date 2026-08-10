using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.CuttingPlanes.Separation;
using ULSAlgorithms.Optimization.Execution;

namespace ULSAlgorithms.Exact.CuttingPlanes;

/// <summary>
/// Exact ULS cut-and-solve strategy using exact general separation of the
/// classical (l,S) convex-hull inequalities.
/// </summary>
public sealed class GeneralLsCuttingPlaneSolver :
    LsCuttingPlaneSolverBase
{
    /// <summary>Initializes with automatic optimization-engine selection.</summary>
    public GeneralLsCuttingPlaneSolver(
        LinearModelSolveOptions? executionOptions = null,
        LsCuttingPlaneOptions? cuttingPlaneOptions = null)
        : base(
            "General (l,S) cutting-plane solver",
            new GeneralLsCutSeparator(),
            executionOptions: executionOptions,
            cuttingPlaneOptions: cuttingPlaneOptions)
    {
    }

    /// <summary>Initializes with an injected portable model solver.</summary>
    public GeneralLsCuttingPlaneSolver(
        LinearModelSolver modelSolver,
        LinearModelSolveOptions? executionOptions = null,
        LsCuttingPlaneOptions? cuttingPlaneOptions = null)
        : base(
            "General (l,S) cutting-plane solver",
            new GeneralLsCutSeparator(),
            modelSolver,
            executionOptions,
            cuttingPlaneOptions)
    {
    }
}
