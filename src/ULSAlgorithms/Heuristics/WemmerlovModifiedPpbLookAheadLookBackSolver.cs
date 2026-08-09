using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements Wemmerlöv's corrected PPB (v = 0.5) combined with the modified
/// Look-Ahead/Look-Back tests.
/// </summary>
/// <remarks>
/// This corresponds to the PPB-v/LALB combination explicitly evaluated in
/// Wemmerlöv's 1983 study.
/// <para>
/// Reference:
/// U. Wemmerlöv,
/// "The Part-Period Balancing Algorithm and Its Look Ahead-Look Back Feature:
/// A Theoretical and Experimental Analysis of a Single Stage Lot-Sizing
/// Procedure",
/// Journal of Operations Management 4(1), 23-39, 1983.
/// DOI: 10.1016/0272-6963(83)90023-2.
/// </para>
/// </remarks>
public sealed class WemmerlovModifiedPpbLookAheadLookBackSolver : IUlsSolver
{
    public const double CorrectionFactor = 0.5;

    public string Name =>
        "Wemmerlov corrected PPB (v=0.5) with Look-Ahead/Look-Back";

    public UlsSolverKind Kind => UlsSolverKind.Heuristic;

    public static bool IsApplicable(UlsProblem problem) =>
        WemmerlovPpbLookAheadLookBackSolver.IsApplicable(problem);

    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        return WemmerlovPpbCore.Solve(
            problem,
            Name,
            CorrectionFactor,
            useLookAheadLookBack: true,
            cancellationToken);
    }
}
