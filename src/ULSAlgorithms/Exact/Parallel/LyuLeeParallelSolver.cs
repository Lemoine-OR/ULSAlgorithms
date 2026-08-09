using System.Buffers;
using System.Threading.Tasks;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin.Internal;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Exact.Parallel;

/// <summary>
/// Parallel exact dynamic lot-sizing solver inspired by Lyu and Lee's
/// lower-triangular parallel Wagner-Whitin computation.
/// </summary>
/// <remarks>
/// <para>
/// For every horizon endpoint the predecessor cells of the lower-triangular
/// dynamic-programming matrix are independent once earlier DP values have been
/// finalized. This implementation partitions those predecessor cells across
/// worker threads, performs a local minimum reduction, then lets the calling
/// thread select the global minimum.
/// </para>
/// <para>
/// Arc costs are evaluated in O(1) using cumulative demand and cumulative
/// demand-weighted holding-cost prefixes. The full triangular matrix is not
/// materialized, which preserves O(T) auxiliary memory.
/// </para>
/// <para>
/// Sequential work is O(T^2). With <c>p</c> effective workers, the candidate
/// evaluation work is ideally O(T^2/p), subject to synchronization, scheduling
/// and finite-horizon overheads.
/// </para>
/// <para>
/// Reference:
/// J.-J. Lyu and M.-C. Lee,
/// "A parallel algorithm for the dynamic lot-sizing problem",
/// Computers &amp; Industrial Engineering 41(2), 127-134, 2001.
/// DOI: 10.1016/S0360-8352(01)00047-X.
/// </para>
/// <para>
/// The accessible publisher metadata describes the parallel algorithm and its
/// O(n^2/p) processor-time objective but not the full source listing. This
/// class is therefore documented as a modern shared-memory realization of the
/// paper's lower-triangular parallel DP architecture, not as a transliteration
/// of the authors' original PVM implementation.
/// </para>
/// </remarks>
public sealed class LyuLeeParallelSolver : IUlsSolver
{
    /// <summary>
    /// Initializes a solver using all available processors and a parallel
    /// threshold of 128 predecessor candidates.
    /// </summary>
    public LyuLeeParallelSolver()
        : this(-1, 128)
    {
    }

    /// <summary>
    /// Initializes a solver with explicit parallelism controls.
    /// </summary>
    /// <param name="maxDegreeOfParallelism">
    /// Maximum number of workers, or -1 to use the runtime default processor
    /// count.
    /// </param>
    /// <param name="parallelThreshold">
    /// Minimum predecessor count before parallel evaluation is attempted.
    /// </param>
    public LyuLeeParallelSolver(
        int maxDegreeOfParallelism,
        int parallelThreshold = 128)
    {
        if (maxDegreeOfParallelism == 0 ||
            maxDegreeOfParallelism < -1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxDegreeOfParallelism));
        }

        if (parallelThreshold < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parallelThreshold));
        }

        MaxDegreeOfParallelism = maxDegreeOfParallelism;
        ParallelThreshold = parallelThreshold;
    }

    /// <inheritdoc />
    public string Name => "Lyu-Lee parallel dynamic lot-sizing";

    /// <inheritdoc />
    public UlsSolverKind Kind => UlsSolverKind.Exact;

    /// <summary>
    /// Gets the configured maximum degree of parallelism.
    /// </summary>
    public int MaxDegreeOfParallelism { get; }

    /// <summary>
    /// Gets the minimum predecessor count for parallel evaluation.
    /// </summary>
    public int ParallelThreshold { get; }

    /// <inheritdoc />
    public UlsSolveResult Solve(
        UlsProblem problem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        cancellationToken.ThrowIfCancellationRequested();

        var horizon = problem.Horizon;

        var effectiveWorkerLimit =
            MaxDegreeOfParallelism == -1
                ? Math.Max(1, Environment.ProcessorCount)
                : MaxDegreeOfParallelism;

        var valueBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);
        var predecessorBuffer = ArrayPool<int>.Shared.Rent(horizon + 1);
        var demandPrefixBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);
        var holdingPrefixBuffer = ArrayPool<double>.Shared.Rent(horizon + 1);
        var weightedDemandPrefixBuffer =
            ArrayPool<double>.Shared.Rent(horizon + 1);

        var workerBestValueBuffer =
            ArrayPool<double>.Shared.Rent(effectiveWorkerLimit);
        var workerBestPredecessorBuffer =
            ArrayPool<int>.Shared.Rent(effectiveWorkerLimit);

        try
        {
            var value = valueBuffer;
            var predecessor = predecessorBuffer;
            var demandPrefix = demandPrefixBuffer;
            var holdingPrefix = holdingPrefixBuffer;
            var weightedDemandPrefix = weightedDemandPrefixBuffer;

            Array.Fill(
                value,
                double.PositiveInfinity,
                0,
                horizon + 1);

            Array.Fill(
                predecessor,
                -1,
                0,
                horizon + 1);

            demandPrefix[0] = 0.0;
            holdingPrefix[0] = 0.0;
            weightedDemandPrefix[0] = 0.0;
            value[0] = 0.0;

            var demands = problem.Demands;
            var holdingCosts = problem.HoldingCosts;

            for (var period = 0; period < horizon; period++)
            {
                demandPrefix[period + 1] = AddFinite(
                    demandPrefix[period],
                    demands[period],
                    "cumulative demand");

                weightedDemandPrefix[period + 1] = AddFinite(
                    weightedDemandPrefix[period],
                    MultiplyFinite(
                        demands[period],
                        holdingPrefix[period],
                        "demand-weighted holding prefix"),
                    "demand-weighted holding prefix");

                holdingPrefix[period + 1] = AddFinite(
                    holdingPrefix[period],
                    holdingCosts[period],
                    "cumulative holding cost");
            }

            for (var end = 1; end <= horizon; end++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double best;
                int bestStart;

                if (end < ParallelThreshold ||
                    effectiveWorkerLimit <= 1)
                {
                    (best, bestStart) = FindBestSerial(
                        end,
                        problem,
                        value,
                        demandPrefix,
                        holdingPrefix,
                        weightedDemandPrefix,
                        cancellationToken);
                }
                else
                {
                    var workerCount =
                        Math.Min(effectiveWorkerLimit, end);

                    var options = new ParallelOptions
                    {
                        CancellationToken = cancellationToken,
                        MaxDegreeOfParallelism = workerCount
                    };

                    global::System.Threading.Tasks.Parallel.For(
                        0,
                        workerCount,
                        options,
                        worker =>
                        {
                            var first =
                                (end * worker) / workerCount;

                            var lastExclusive =
                                (end * (worker + 1)) / workerCount;

                            var localBest = double.PositiveInfinity;
                            var localBestStart = -1;

                            for (var start = first;
                                 start < lastExclusive;
                                 start++)
                            {
                                if ((start & 255) == 0)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                }

                                var candidate = EvaluateCandidate(
                                    start,
                                    end,
                                    problem,
                                    value,
                                    demandPrefix,
                                    holdingPrefix,
                                    weightedDemandPrefix);

                                if (candidate < localBest ||
                                    (candidate == localBest &&
                                     (localBestStart < 0 ||
                                      start < localBestStart)))
                                {
                                    localBest = candidate;
                                    localBestStart = start;
                                }
                            }

                            workerBestValueBuffer[worker] = localBest;
                            workerBestPredecessorBuffer[worker] =
                                localBestStart;
                        });

                    best = double.PositiveInfinity;
                    bestStart = -1;

                    for (var worker = 0;
                         worker < workerCount;
                         worker++)
                    {
                        var candidate =
                            workerBestValueBuffer[worker];

                        var candidateStart =
                            workerBestPredecessorBuffer[worker];

                        if (candidate < best ||
                            (candidate == best &&
                             candidateStart >= 0 &&
                             (bestStart < 0 ||
                              candidateStart < bestStart)))
                        {
                            best = candidate;
                            bestStart = candidateStart;
                        }
                    }
                }

                if (!double.IsFinite(best) ||
                    bestStart < 0)
                {
                    throw new ArithmeticException(
                        $"No finite Lyu-Lee value was obtained for state {end}.");
                }

                value[end] = best;
                predecessor[end] = bestStart;
            }

            return ZeroInventoryOrderSolutionBuilder.Build(
                problem,
                predecessor.AsSpan(0, horizon + 1),
                Name,
                cancellationToken);
        }
        finally
        {
            ArrayPool<double>.Shared.Return(valueBuffer, clearArray: false);
            ArrayPool<int>.Shared.Return(predecessorBuffer, clearArray: false);
            ArrayPool<double>.Shared.Return(
                demandPrefixBuffer,
                clearArray: false);
            ArrayPool<double>.Shared.Return(
                holdingPrefixBuffer,
                clearArray: false);
            ArrayPool<double>.Shared.Return(
                weightedDemandPrefixBuffer,
                clearArray: false);
            ArrayPool<double>.Shared.Return(
                workerBestValueBuffer,
                clearArray: false);
            ArrayPool<int>.Shared.Return(
                workerBestPredecessorBuffer,
                clearArray: false);
        }
    }

    private static (double Best, int BestStart) FindBestSerial(
        int end,
        UlsProblem problem,
        double[] value,
        double[] demandPrefix,
        double[] holdingPrefix,
        double[] weightedDemandPrefix,
        CancellationToken cancellationToken)
    {
        var best = double.PositiveInfinity;
        var bestStart = -1;

        for (var start = 0; start < end; start++)
        {
            if ((start & 255) == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            var candidate = EvaluateCandidate(
                start,
                end,
                problem,
                value,
                demandPrefix,
                holdingPrefix,
                weightedDemandPrefix);

            if (candidate < best ||
                (candidate == best &&
                 (bestStart < 0 ||
                  start < bestStart)))
            {
                best = candidate;
                bestStart = start;
            }
        }

        return (best, bestStart);
    }

    private static double EvaluateCandidate(
        int start,
        int end,
        UlsProblem problem,
        double[] value,
        double[] demandPrefix,
        double[] holdingPrefix,
        double[] weightedDemandPrefix)
    {
        var segmentDemand =
            demandPrefix[end] -
            demandPrefix[start];

        if (segmentDemand == 0.0)
        {
            return value[start];
        }

        var transformedUnitCost =
            problem.UnitProductionCosts[start] -
            holdingPrefix[start];

        var variableCost = AddFinite(
            MultiplyFinite(
                transformedUnitCost,
                segmentDemand,
                "regeneration-interval variable cost"),
            weightedDemandPrefix[end] -
            weightedDemandPrefix[start],
            "regeneration-interval variable cost");

        var arcCost = AddFinite(
            problem.SetupCosts[start],
            variableCost,
            "regeneration-interval cost");

        return AddFinite(
            value[start],
            arcCost,
            "dynamic-programming candidate");
    }

    private static double AddFinite(
        double left,
        double right,
        string operation)
    {
        var result = left + right;

        if (!double.IsFinite(result))
        {
            throw new ArithmeticException(
                $"Numerical overflow while computing {operation}.");
        }

        return result;
    }

    private static double MultiplyFinite(
        double left,
        double right,
        string operation)
    {
        var result = left * right;

        if (!double.IsFinite(result))
        {
            throw new ArithmeticException(
                $"Numerical overflow while computing {operation}.");
        }

        return result;
    }
}
