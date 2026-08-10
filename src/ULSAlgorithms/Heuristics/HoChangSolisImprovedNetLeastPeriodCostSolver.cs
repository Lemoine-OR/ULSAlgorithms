using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements the improved Ho-Chang-Solis nLPC(i) heuristic.
/// </summary>
/// <remarks>
/// <para>
/// nLPC(i) uses the same net average period cost as nLPC, but adds the
/// paper's improved stopping rule. Besides stopping on a strict increase, it
/// also stops when the current and previous net average period costs are both
/// equal to the setup cost.
/// </para>
/// <para>
/// The equality condition is evaluated with a scale-aware numerical tolerance;
/// this is the floating-point counterpart of the exact equality in the
/// published rule.
/// </para>
/// <para>
/// Reference: J. C. Ho, Y.-L. Chang and A. O. Solis,
/// "Two modifications of the least cost per period heuristic for dynamic
/// lot-sizing", Journal of the Operational Research Society 57(8),
/// 1005-1013, 2006, DOI 10.1057/palgrave.jors.2602076.
/// </para>
/// </remarks>
public sealed class HoChangSolisImprovedNetLeastPeriodCostSolver : IUlsSolver
{
    public string Name =>
        "Ho-Chang-Solis improved net Least Period Cost";

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
                useImprovedTieBreak: true,
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
