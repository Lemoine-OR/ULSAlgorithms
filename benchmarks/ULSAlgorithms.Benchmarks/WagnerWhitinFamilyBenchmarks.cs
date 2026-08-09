using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Compares the three Wagner-Whitin implementations available in the library.
/// </summary>
[MemoryDiagnoser]
public class WagnerWhitinFamilyBenchmarks
{
    private WagnerWhitinClassicalSolver _classical = null!;
    private WagnerWhitinEvansSolver _evans = null!;
    private WagnerWhitinSolver _linear = null!;
    private UlsProblem _problem = null!;

    [Params(50, 100, 250, 500, 1_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(1958 + Horizon);

        var demands = new double[Horizon];
        var setupCosts = new double[Horizon];
        var productionCosts = new double[Horizon];
        var holdingCosts = new double[Horizon];

        for (var period = 0; period < Horizon; period++)
        {
            demands[period] = random.Next(1, 101);
            setupCosts[period] = random.Next(25, 501);
            productionCosts[period] = 10.0;
            holdingCosts[period] = random.NextDouble() * 3.0;
        }

        _problem = new UlsProblem(
            demands,
            setupCosts,
            productionCosts,
            holdingCosts);

        _classical = new WagnerWhitinClassicalSolver();
        _evans = new WagnerWhitinEvansSolver();
        _linear = new WagnerWhitinSolver();
    }

    [Benchmark(Baseline = true)]
    public UlsSolveResult Classical()
    {
        return _classical.Solve(_problem);
    }

    [Benchmark]
    public UlsSolveResult Evans1985()
    {
        return _evans.Solve(_problem);
    }

    [Benchmark]
    public UlsSolveResult WagelmansLinear()
    {
        return _linear.Solve(_problem);
    }
}
