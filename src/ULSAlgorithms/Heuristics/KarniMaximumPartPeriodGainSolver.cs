using System.Buffers;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Heuristics.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Heuristics;

/// <summary>
/// Implements Karni's Maximum Part-Period Gain (MPG) heuristic.
/// </summary>
/// <remarks>
/// <para>
/// MPG starts from Lot-for-Lot and is deliberately non-forward. At each
/// iteration it selects the globally smallest current part-period cost of
/// deleting a replenishment boundary. The adjacent lots are merged while the
/// required part-periods do not exceed the Economic Part Period S/h.
/// </para>
/// <para>
/// The implementation uses a lazy-invalidated priority queue plus an
/// array-backed doubly-linked list of active replenishment lots. This preserves
/// the global greedy merge rule while avoiding repeated full-horizon scans.
/// </para>
/// <para>
/// Original source: R. Karni,
/// "Maximum Part-Period Gain (MPG)—A Lot Sizing Procedure for Unconstrained
/// and Constrained Requirements Planning Systems",
/// Production and Inventory Management 22(2), 91-98, 1981.
/// </para>
/// <para>
/// Detailed reconstruction and numerical example:
/// L. Baciarello, M. D'Avino, R. Onori and M. M. Schiraldi,
/// "Lot Sizing Heuristics Performance", 2013, DOI 10.5772/56004.
/// </para>
/// </remarks>
public sealed class KarniMaximumPartPeriodGainSolver : IUlsSolver
{
    private readonly record struct MergeCandidate(
        int RightStart,
        int Version);

    public string Name =>
        "Karni Maximum Part-Period Gain";

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

        int[] cycleBuffer =
            ArrayPool<int>.Shared.Rent(horizon);

        try
        {
            Span<int> cycleEnds =
                cycleBuffer.AsSpan(0, horizon);

            cycleEnds.Fill(-1);

            ReadOnlySpan<double> demands =
                problem.Demands;

            int first =
                ClassicHeuristicGuard.FindNextPositiveDemand(
                    demands,
                    0);

            if (first >= horizon)
            {
                return HeuristicSolutionBuilder.Build(
                    problem,
                    cycleEnds,
                    Name,
                    cancellationToken);
            }

            double holdingCost =
                horizon > 1
                    ? problem.HoldingCosts[0]
                    : 0.0;

            if (holdingCost == 0.0)
            {
                cycleEnds[first] =
                    horizon - 1;

                return HeuristicSolutionBuilder.Build(
                    problem,
                    cycleEnds,
                    Name,
                    cancellationToken);
            }

            double economicPartPeriod =
                problem.SetupCosts[0] /
                holdingCost;

            var previous =
                new int[horizon];

            var next =
                new int[horizon];

            var quantity =
                new double[horizon];

            var active =
                new bool[horizon];

            var version =
                new int[horizon];

            Array.Fill(previous, -1);
            Array.Fill(next, -1);

            int previousPositive = -1;

            for (int period = first;
                 period < horizon;
                 period++)
            {
                if ((period & 255) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (demands[period] == 0.0)
                {
                    continue;
                }

                active[period] = true;
                quantity[period] = demands[period];
                previous[period] = previousPositive;

                if (previousPositive >= 0)
                {
                    next[previousPositive] =
                        period;
                }

                previousPositive =
                    period;
            }

            var queue =
                new PriorityQueue<
                    MergeCandidate,
                    (double PartPeriods, int RightStart)>();

            for (int right = next[first];
                 right >= 0;
                 right = next[right])
            {
                Enqueue(right);
            }

            while (queue.TryDequeue(
                       out MergeCandidate candidate,
                       out _))
            {
                cancellationToken.ThrowIfCancellationRequested();

                int right =
                    candidate.RightStart;

                if (!active[right] ||
                    candidate.Version != version[right] ||
                    previous[right] < 0)
                {
                    continue;
                }

                int left =
                    previous[right];

                double partPeriods =
                    (right - left) *
                    quantity[right];

                if (!double.IsFinite(partPeriods))
                {
                    throw new ArithmeticException(
                        "Numerical overflow while evaluating an MPG merge.");
                }

                if (StrictlyGreater(
                        partPeriods,
                        economicPartPeriod))
                {
                    // Because the queue is ordered by the current part-period
                    // priority of every valid adjacent merge, no remaining
                    // valid merge can satisfy the EPP threshold.
                    break;
                }

                int rightNeighbor =
                    next[right];

                quantity[left] =
                    AddFinite(
                        quantity[left],
                        quantity[right],
                        "MPG merged lot quantity");

                version[left]++;

                active[right] = false;
                version[right]++;

                next[left] =
                    rightNeighbor;

                if (rightNeighbor >= 0)
                {
                    previous[rightNeighbor] =
                        left;

                    version[rightNeighbor]++;
                }

                Enqueue(left);
                Enqueue(rightNeighbor);
            }

            for (int start = first;
                 start >= 0;)
            {
                if (!active[start])
                {
                    throw new InvalidOperationException(
                        "Invalid MPG active-lot chain.");
                }

                int following =
                    next[start];

                cycleEnds[start] =
                    following >= 0
                        ? following - 1
                        : horizon - 1;

                start =
                    following;
            }

            return HeuristicSolutionBuilder.Build(
                problem,
                cycleEnds,
                Name,
                cancellationToken);

            void Enqueue(
                int right)
            {
                if (right < 0 ||
                    !active[right] ||
                    previous[right] < 0)
                {
                    return;
                }

                int left =
                    previous[right];

                double partPeriods =
                    (right - left) *
                    quantity[right];

                if (!double.IsFinite(partPeriods))
                {
                    throw new ArithmeticException(
                        "Numerical overflow while creating an MPG merge candidate.");
                }

                queue.Enqueue(
                    new MergeCandidate(
                        right,
                        version[right]),
                    (
                        partPeriods,
                        right
                    ));
            }
        }
        finally
        {
            ArrayPool<int>.Shared.Return(
                cycleBuffer,
                clearArray: false);
        }
    }

    private static bool StrictlyGreater(
        double left,
        double right)
    {
        double tolerance =
            1.0e-12 *
            Math.Max(
                1.0,
                Math.Max(
                    Math.Abs(left),
                    Math.Abs(right)));

        return left >
               right + tolerance;
    }

    private static double AddFinite(
        double left,
        double right,
        string operation)
    {
        double value =
            left + right;

        if (!double.IsFinite(value))
        {
            throw new ArithmeticException(
                $"Numerical overflow while computing {operation}.");
        }

        return value;
    }
}
