using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements Wemmerlöv's corrected Part-Period Balancing rule using the
/// practical limiting correction factor v = 0.5.
/// </summary>
/// <remarks>
/// <para>
/// The original PPB balance expression is modified by replacing the ordinary
/// part-period weight <c>j-1</c> with <c>j-1+v</c>. Wemmerlöv derives the
/// correction and reports that the constant limiting value <c>v=0.5</c> can
/// be used in practice with only a small penalty relative to item-specific
/// values.
/// </para>
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
public sealed class WemmerlovModifiedPartPeriodBalancingSolver : IUlsSolver
{
    public const double CorrectionFactor = 0.5;

    public string Name =>
        "Wemmerlov corrected PPB (v=0.5)";

    public UlsSolverKind Kind => UlsSolverKind.Heuristic;

    public static bool IsApplicable(UlsProblem problem) =>
        ClassicHeuristicGuard.HasStationaryRelevantCosts(problem);

    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        return WemmerlovPpbCore.Solve(
            problem,
            Name,
            CorrectionFactor,
            useLookAheadLookBack: false,
            cancellationToken);
    }
}
