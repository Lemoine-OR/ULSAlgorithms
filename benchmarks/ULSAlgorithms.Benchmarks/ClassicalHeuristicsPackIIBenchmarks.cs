using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Scaling benchmark for Classical Heuristics Pack II.
/// </summary>
[MemoryDiagnoser]
public class ClassicalHeuristicsPackIIBenchmarks
{
    private UlsProblem _problem = null!;

    private FreelandColleySolver _freeland = null!;
    private PattersonLaForgeIncrementalPartPeriodSolver _ippa = null!;
    private WemmerlovModifiedPartPeriodBalancingSolver _modifiedPpb = null!;
    private WemmerlovPpbLookAheadLookBackSolver _lalb = null!;
    private WemmerlovModifiedPpbLookAheadLookBackSolver _modifiedLalb = null!;

    [Params(100, 1_000, 10_000, 100_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random =
            new Random(
                1983 +
                Horizon);

        var demands =
            new double[Horizon];

        for (var period = 0;
             period < Horizon;
             period++)
        {
            demands[period] =
                random.Next(1, 101);
        }

        _problem = new UlsProblem(
            demands,
            Enumerable.Repeat(
                100.0,
                Horizon).ToArray(),
            Enumerable.Repeat(
                5.0,
                Horizon).ToArray(),
            Enumerable.Repeat(
                1.0,
                Horizon).ToArray());

        _freeland =
            new FreelandColleySolver();

        _ippa =
            new PattersonLaForgeIncrementalPartPeriodSolver();

        _modifiedPpb =
            new WemmerlovModifiedPartPeriodBalancingSolver();

        _lalb =
            new WemmerlovPpbLookAheadLookBackSolver();

        _modifiedLalb =
            new WemmerlovModifiedPpbLookAheadLookBackSolver();
    }

    [Benchmark]
    public UlsSolveResult FreelandColley() =>
        _freeland.Solve(_problem);

    [Benchmark]
    public UlsSolveResult IncrementalPartPeriod() =>
        _ippa.Solve(_problem);

    [Benchmark]
    public UlsSolveResult WemmerlovModifiedPpb() =>
        _modifiedPpb.Solve(_problem);

    [Benchmark]
    public UlsSolveResult WemmerlovPpbLalb() =>
        _lalb.Solve(_problem);

    [Benchmark]
    public UlsSolveResult WemmerlovModifiedPpbLalb() =>
        _modifiedLalb.Solve(_problem);
}
