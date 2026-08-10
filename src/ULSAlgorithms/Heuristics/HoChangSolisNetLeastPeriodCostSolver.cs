using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements the Ho-Chang-Solis net Least Period Cost (nLPC) heuristic.
/// </summary>
/// <remarks>
/// <para>
/// For a lot beginning in period i and ending in period j, Ho, Chang and Solis
/// define the net average period cost as the setup-plus-holding cost divided by
/// the number of non-zero-demand periods in [i,j]. Zero-demand periods are
/// explicitly skipped by the stopping test.
/// </para>
/// <para>
/// The lot is extended while the net average period cost does not increase.
/// This implementation evaluates the same published stopping rule
/// incrementally, so each calendar period is scanned only a constant number of
/// times.
/// </para>
/// <para>
/// Reference: J. C. Ho, Y.-L. Chang and A. O. Solis,
/// "Two modifications of the least cost per period heuristic for dynamic
/// lot-sizing", Journal of the Operational Research Society 57(8),
/// 1005-1013, 2006, DOI 10.1057/palgrave.jors.2602076.
/// </para>
/// </remarks>
public sealed class HoChangSolisNetLeastPeriodCostSolver : IUlsSolver
{
    public string Name =>
        "Ho-Chang-Solis net Least Period Cost";

    public UlsSolverKind Kind =>
        UlsSolverKind.Heuristic;

    public static bool IsApplicable(
        UlsProblem problem) =>
        ClassicHeuristicGuard.HasStationaryRelevantCosts(problem);

    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        ClassicHeuristicGuard.ThrowIfNotStationary(
            problem,
            Name);

        int horizon = problem.Horizon;
        int[] buffer =
            ArrayPool<int>.Shared.Rent(horizon);

        try
        {
            Span<int> cycleEnds =
                buffer.AsSpan(0, horizon);

            HoChangSolisNetLeastPeriodCostCore.BuildCycleEnds(
                problem,
                cycleEnds,
                useImprovedTieBreak: false,
                cancellationToken);

            return HeuristicSolutionBuilder.Build(
                problem,
                cycleEnds,
                Name,
                cancellationToken);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(
                buffer,
                clearArray: false);
        }
    }
}
