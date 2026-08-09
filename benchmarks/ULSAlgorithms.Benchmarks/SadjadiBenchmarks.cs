using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Compares the publication-specific Economic-Part-Period implementations.
/// </summary>
[MemoryDiagnoser]
public class SadjadiBenchmarks
{
    private UlsProblem _problem = null!;
    private SadjadiAryanezhadSadeghiSolver _sadjadi = null!;
    private HeadyZhuEconomicPartPeriodSolver _headyZhu = null!;
    private BahlTajPlanningHorizonSolver _bahlTaj = null!;
    private WagnerWhitinSolver _wagelmans = null!;

    [Params(100, 500, 1_000, 5_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(2009 + Horizon);
        var demands = new double[Horizon];

        for (var period = 0; period < Horizon; period++)
        {
            demands[period] = random.Next(1, 101);
        }

        _problem = new UlsProblem(
            demands,
            Enumerable.Repeat(54.0, Horizon).ToArray(),
            Enumerable.Repeat(2.0, Horizon).ToArray(),
            Enumerable.Repeat(0.4, Horizon).ToArray());

        _sadjadi = new SadjadiAryanezhadSadeghiSolver();
        _headyZhu = new HeadyZhuEconomicPartPeriodSolver();
        _bahlTaj = new BahlTajPlanningHorizonSolver();
        _wagelmans = new WagnerWhitinSolver();
    }

    [Benchmark]
    public UlsSolveResult Sadjadi() => _sadjadi.Solve(_problem);

    [Benchmark]
    public UlsSolveResult HeadyZhu() => _headyZhu.Solve(_problem);

    [Benchmark]
    public UlsSolveResult BahlTaj() => _bahlTaj.Solve(_problem);

    [Benchmark(Baseline = true)]
    public UlsSolveResult WagelmansLinear() => _wagelmans.Solve(_problem);
}
