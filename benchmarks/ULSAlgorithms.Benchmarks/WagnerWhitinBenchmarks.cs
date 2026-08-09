using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Measures the linear-time Wagner-Whitin implementation, including solution
/// reconstruction and public result allocation.
/// </summary>
[MemoryDiagnoser]
public class WagnerWhitinBenchmarks
{
    private WagnerWhitinSolver _solver = null!;
    private UlsProblem _problem = null!;

    [Params(100, 1_000, 10_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(20260809 + Horizon);
        var demands = new double[Horizon];
        var setupCosts = new double[Horizon];
        var productionCosts = new double[Horizon];
        var holdingCosts = new double[Horizon];

        for (var period = 0; period < Horizon; period++)
        {
            demands[period] = random.Next(1, 101);
            setupCosts[period] = random.Next(50, 501);
            productionCosts[period] = 10.0;
            holdingCosts[period] = random.NextDouble() * 2.0;
        }

        _problem = new UlsProblem(
            demands,
            setupCosts,
            productionCosts,
            holdingCosts);

        _solver = new WagnerWhitinSolver();
    }

    [Benchmark]
    public UlsSolveResult Solve()
    {
        return _solver.Solve(_problem);
    }
}
