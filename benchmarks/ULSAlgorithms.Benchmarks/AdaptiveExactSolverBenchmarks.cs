using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.Wagelmans;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;
using ULSAlgorithms.Selection;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Measures adaptive exact selection overhead against the solver that would be
/// called directly for the same cost structure.
/// </summary>
[MemoryDiagnoser]
public class AdaptiveExactSolverBenchmarks
{
    private UlsProblem _problem = null!;
    private IUlsSolver _direct = null!;
    private AdaptiveExactUlsSolver _adaptive = null!;

    [Params(100, 1_000, 10_000)]
    public int Horizon { get; set; }

    [Params("NSM", "General")]
    public string Scenario { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(2400 + Horizon);
        var demands = new double[Horizon];
        var setupCosts = new double[Horizon];
        var productionCosts = new double[Horizon];
        var holdingCosts = new double[Horizon];

        for (var period = 0; period < Horizon; period++)
        {
            demands[period] = random.Next(0, 101);
            setupCosts[period] = random.Next(25, 501);
            holdingCosts[period] = period == Horizon - 1
                ? 0.0
                : 1.0 + random.NextDouble() * 4.0;
        }

        if (Scenario == "NSM")
        {
            // Constant p and nonnegative h satisfy p[t] + h[t] >= p[t+1].
            Array.Fill(productionCosts, 10.0);
            _direct = new WagnerWhitinSolver();
        }
        else
        {
            for (var period = 0; period < Horizon; period++)
            {
                productionCosts[period] = random.NextDouble() * 50.0;
            }

            // Force at least one speculative-motive violation deterministically.
            productionCosts[0] = 0.0;
            holdingCosts[0] = 0.0;
            productionCosts[1] = 50.0;
            _direct = new WagelmansGeneralSolver();
        }

        _problem = new UlsProblem(
            demands,
            setupCosts,
            productionCosts,
            holdingCosts);

        _adaptive = new AdaptiveExactUlsSolver();
    }

    [Benchmark]
    public UlsProblemCharacteristics AnalyzeOnly()
    {
        return UlsProblemAnalyzer.Analyze(_problem);
    }

    [Benchmark(Baseline = true)]
    public UlsSolveResult DirectExact()
    {
        return _direct.Solve(_problem);
    }

    [Benchmark]
    public UlsSolveResult AdaptiveExact()
    {
        return _adaptive.Solve(_problem);
    }
}
