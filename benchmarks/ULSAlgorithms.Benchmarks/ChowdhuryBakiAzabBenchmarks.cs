using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Exact.ChowdhuryBakiAzab;
using ULSAlgorithms.Exact.FedergruenTzur;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Compares the independent linear-time Wagner-Whitin implementations.
/// </summary>
[MemoryDiagnoser]
public class ChowdhuryBakiAzabBenchmarks
{
    private UlsProblem _problem = null!;
    private ChowdhuryBakiAzabSolver _chowdhury = null!;
    private WagnerWhitinSolver _wagelmans = null!;
    private FedergruenTzurNoSpeculativeMotiveSolver _federgruenTzur = null!;

    [Params(100, 1_000, 10_000, 100_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(2018 + Horizon);

        var demands = new double[Horizon];
        var setupCosts = new double[Horizon];
        var productionCosts = new double[Horizon];
        var holdingCosts = new double[Horizon];

        for (var period = 0; period < Horizon; period++)
        {
            demands[period] = random.Next(1, 101);
            setupCosts[period] = random.Next(10, 501);
            productionCosts[period] = 5.0;
            holdingCosts[period] = 2.0;
        }

        _problem = new UlsProblem(
            demands,
            setupCosts,
            productionCosts,
            holdingCosts);

        _chowdhury = new ChowdhuryBakiAzabSolver();
        _wagelmans = new WagnerWhitinSolver();
        _federgruenTzur =
            new FedergruenTzurNoSpeculativeMotiveSolver();
    }

    [Benchmark(Baseline = true)]
    public UlsSolveResult WagelmansLinear() =>
        _wagelmans.Solve(_problem);

    [Benchmark]
    public UlsSolveResult FedergruenTzurLinear() =>
        _federgruenTzur.Solve(_problem);

    [Benchmark]
    public UlsSolveResult ChowdhuryBakiAzab() =>
        _chowdhury.Solve(_problem);
}
