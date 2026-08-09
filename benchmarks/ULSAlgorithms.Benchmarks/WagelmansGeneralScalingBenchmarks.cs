using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Exact.Wagelmans;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Measures scaling of the O(n log n) Wagelmans general solver.
/// </summary>
[MemoryDiagnoser]
public class WagelmansGeneralScalingBenchmarks
{
    private UlsProblem _problem = null!;
    private WagelmansGeneralSolver _solver = null!;

    [Params(100, 1_000, 10_000, 100_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(40 + Horizon);

        var demands = new double[Horizon];
        var setupCosts = new double[Horizon];
        var productionCosts = new double[Horizon];
        var holdingCosts = new double[Horizon];

        for (var period = 0; period < Horizon; period++)
        {
            demands[period] = random.Next(1, 101);
            setupCosts[period] = random.Next(10, 501);
            productionCosts[period] = random.NextDouble() * 100.0;
            holdingCosts[period] = random.NextDouble() * 10.0;
        }

        _problem = new UlsProblem(
            demands,
            setupCosts,
            productionCosts,
            holdingCosts);

        _solver = new WagelmansGeneralSolver();
    }

    [Benchmark]
    public UlsSolveResult Solve()
    {
        return _solver.Solve(_problem);
    }
}
