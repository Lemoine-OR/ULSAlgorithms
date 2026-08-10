using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.Optimization.Execution;

namespace ULSAlgorithms.Results;

/// <summary>
/// ULS result enriched with the complete (l,S) cut-generation report.
/// </summary>
public sealed class CuttingPlaneUlsSolveResult :
    UlsSolveResult
{
    /// <summary>Initializes a cutting-plane ULS result.</summary>
    public CuttingPlaneUlsSolveResult(
        string solverName,
        UlsSolveStatus status,
        CutSeparationMethod separationMethod,
        CuttingPlaneExecutionReport cuttingPlaneExecution,
        LinearModelSolveResult finalModelExecution,
        UlsSolution? solution = null,
        string? message = null)
        : base(
            solverName,
            status,
            solution,
            message)
    {
        if (separationMethod ==
            CutSeparationMethod.Unknown)
        {
            throw new ArgumentOutOfRangeException(
                nameof(separationMethod));
        }

        SeparationMethod = separationMethod;

        CuttingPlaneExecution =
            cuttingPlaneExecution ??
            throw new ArgumentNullException(
                nameof(cuttingPlaneExecution));

        FinalModelExecution =
            finalModelExecution ??
            throw new ArgumentNullException(
                nameof(finalModelExecution));
    }

    /// <summary>Gets the separation method.</summary>
    public CutSeparationMethod SeparationMethod { get; }

    /// <summary>Gets complete generated/added cut traceability.</summary>
    public CuttingPlaneExecutionReport CuttingPlaneExecution { get; }

    /// <summary>Gets the final exact MILP execution.</summary>
    public LinearModelSolveResult FinalModelExecution { get; }
}
