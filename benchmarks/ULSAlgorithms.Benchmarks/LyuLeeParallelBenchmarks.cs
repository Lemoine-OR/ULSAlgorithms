using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Exact.Parallel;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Measures the modern shared-memory Lyu-Lee reconstruction against a
/// sequential quadratic implementation.
/// </summary>
[MemoryDiagnoser]
public class LyuLeeParallelBenchmarks
{
    private UlsProblem _problem = null!;
    private LyuLeeParallelSolver _parallel = null!;
    private LyuLeeParallelSolver _singleWorker = null!;
    private WagnerWhitinEvansSolver _evans = null!;

    [Params(100, 500, 1_000, 2_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(2001 + Horizon);

        var demands = new double[Horizon];
        var setupCosts = new double[Horizon];
        var productionCosts = new double[Horizon];
        var holdingCosts = new double[Horizon];

        for (var period = 0; period < Horizon; period++)
        {
            demands[period] = random.Next(1, 101);
            setupCosts[period] = random.Next(10, 501);
            productionCosts[period] = random.NextDouble() * 50.0;
            holdingCosts[period] = random.NextDouble() * 5.0;
        }

        _problem = new UlsProblem(
            demands,
            setupCosts,
            productionCosts,
            holdingCosts);

        _parallel = new LyuLeeParallelSolver();
        _singleWorker = new LyuLeeParallelSolver(1, 1);
        _evans = new WagnerWhitinEvansSolver();
    }

    [Benchmark(Baseline = true)]
    public UlsSolveResult Evans() => _evans.Solve(_problem);

    [Benchmark]
    public UlsSolveResult LyuLeeSingleWorker() =>
        _singleWorker.Solve(_problem);

    [Benchmark]
    public UlsSolveResult LyuLeeParallel() =>
        _parallel.Solve(_problem);
}
