using ULSAlgorithms.Formulations.FacilityLocation;
using ULSAlgorithms.Optimization.Execution;

namespace ULSAlgorithms.Exact.Formulations;

/// <summary>
/// Exact solver-backed ULS strategy using the disaggregated
/// facility-location formulation.
/// </summary>
public sealed class FacilityLocationFormulationSolver :
    SolverBackedUlsFormulationSolverBase
{
    /// <summary>Initializes with automatic optimization-engine selection.</summary>
    public FacilityLocationFormulationSolver(
        LinearModelSolveOptions? executionOptions = null)
        : base(
            "Facility-location formulation",
            new FacilityLocationFormulationBuilder(),
            executionOptions: executionOptions)
    {
    }

    /// <summary>Initializes with an injected portable model solver.</summary>
    public FacilityLocationFormulationSolver(
        LinearModelSolver modelSolver,
        LinearModelSolveOptions? executionOptions = null)
        : base(
            "Facility-location formulation",
            new FacilityLocationFormulationBuilder(),
            modelSolver,
            executionOptions)
    {
    }
}
