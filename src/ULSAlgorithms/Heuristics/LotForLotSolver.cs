using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements the classical Lot-for-Lot (L4L/LFL) policy.
/// </summary>
/// <remarks>
/// Every positive demand is replenished in its own period. The method is a
/// standard MRP baseline and is feasible for the complete general
/// <see cref="UlsProblem"/> cost model.
/// Time complexity is O(T); auxiliary working memory is O(T).
/// </remarks>
public sealed class LotForLotSolver : IUlsSolver
{
    public string Name => "Lot-for-Lot";

    public UlsSolverKind Kind => UlsSolverKind.Heuristic;

    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        var horizon = problem.Horizon;
        var buffer = ArrayPool<int>.Shared.Rent(horizon);

        try
        {
            var cycleEnds = buffer.AsSpan(0, horizon);
            cycleEnds.Fill(-1);

            var demands = problem.Demands;

            for (var period = 0; period < horizon; period++)
            {
                if (demands[period] > 0.0)
                {
                    cycleEnds[period] = period;
                }
            }

            return HeuristicSolutionBuilder.Build(
                problem,
                cycleEnds,
                Name,
                cancellationToken);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(buffer, clearArray: false);
        }
    }
}
