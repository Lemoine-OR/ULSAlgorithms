using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.CuttingPlanes.Separation;
using ULSAlgorithms.Optimization.Execution;

namespace ULSAlgorithms.Exact.CuttingPlanes;

/// <summary>
/// Exact ULS cut-and-solve strategy using the O(T^2) Wagner-Whitin
/// specialization of the classical (l,S) inequalities.
/// </summary>
public sealed class WagnerWhitinLsCuttingPlaneSolver :
    LsCuttingPlaneSolverBase
{
    /// <summary>Initializes with automatic optimization-engine selection.</summary>
    public WagnerWhitinLsCuttingPlaneSolver(
        LinearModelSolveOptions? executionOptions = null,
        LsCuttingPlaneOptions? cuttingPlaneOptions = null)
        : base(
            "Wagner-Whitin (l,S) cutting-plane solver",
            new WagnerWhitinLsCutSeparator(),
            executionOptions: executionOptions,
            cuttingPlaneOptions: cuttingPlaneOptions)
    {
    }

    /// <summary>Initializes with an injected portable model solver.</summary>
    public WagnerWhitinLsCuttingPlaneSolver(
        LinearModelSolver modelSolver,
        LinearModelSolveOptions? executionOptions = null,
        LsCuttingPlaneOptions? cuttingPlaneOptions = null)
        : base(
            "Wagner-Whitin (l,S) cutting-plane solver",
            new WagnerWhitinLsCutSeparator(),
            modelSolver,
            executionOptions,
            cuttingPlaneOptions)
    {
    }
}
