using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Exact.Wagelmans;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Compares exact solvers on identical general-cost ULS instances.
/// </summary>
[MemoryDiagnoser]
public class GeneralExactSolversBenchmarks
{
    private UlsProblem _problem = null!;
    private WagnerWhitinClassicalSolver _classical = null!;
    private WagnerWhitinEvansSolver _evans = null!;
    private WagelmansGeneralSolver _wagelmans = null!;

    [Params(50, 100, 250, 500, 1_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(1992 + Horizon);

        var demands = new double[Horizon];
        var setupCosts = new double[Horizon];
        var productionCosts = new double[Horizon];
        var holdingCosts = new double[Horizon];

        for (var period = 0; period < Horizon; period++)
        {
            demands[period] = random.Next(1, 101);
            setupCosts[period] = random.Next(25, 501);
            productionCosts[period] = random.NextDouble() * 50.0;
            holdingCosts[period] = random.NextDouble() * 5.0;
        }

        _problem = new UlsProblem(
            demands,
            setupCosts,
            productionCosts,
            holdingCosts);

        _classical = new WagnerWhitinClassicalSolver();
        _evans = new WagnerWhitinEvansSolver();
        _wagelmans = new WagelmansGeneralSolver();
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
    public UlsSolveResult WagelmansGeneral()
    {
        return _wagelmans.Solve(_problem);
    }
}
