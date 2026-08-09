using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Exact.AggarwalPark;
using ULSAlgorithms.Exact.FedergruenTzur;
using ULSAlgorithms.Exact.Wagelmans;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Compares Aggarwal-Park recursive Monge matrix searching with the other
/// general exact ULS algorithms.
/// </summary>
[MemoryDiagnoser]
public class AggarwalParkBenchmarks
{
    private UlsProblem _problem = null!;

    private WagnerWhitinEvansSolver _evans = null!;
    private WagelmansGeneralSolver _wagelmans = null!;
    private FedergruenTzurSolver _federgruenTzur = null!;
    private AggarwalParkSolver _aggarwalPark = null!;

    [Params(50, 100, 250, 500, 1_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random =
            new Random(1993 + Horizon);

        var demands =
            new double[Horizon];

        var setupCosts =
            new double[Horizon];

        var productionCosts =
            new double[Horizon];

        var holdingCosts =
            new double[Horizon];

        for (var period = 0;
             period < Horizon;
             period++)
        {
            demands[period] =
                random.Next(1, 101);

            setupCosts[period] =
                random.Next(25, 501);

            productionCosts[period] =
                random.NextDouble() * 50.0;

            holdingCosts[period] =
                random.NextDouble() * 5.0;
        }

        _problem =
            new UlsProblem(
                demands,
                setupCosts,
                productionCosts,
                holdingCosts);

        _evans =
            new WagnerWhitinEvansSolver();

        _wagelmans =
            new WagelmansGeneralSolver();

        _federgruenTzur =
            new FedergruenTzurSolver();

        _aggarwalPark =
            new AggarwalParkSolver();
    }

    [Benchmark(Baseline = true)]
    public UlsSolveResult Evans1985()
    {
        return _evans.Solve(_problem);
    }

    [Benchmark]
    public UlsSolveResult WagelmansGeneral()
    {
        return _wagelmans.Solve(_problem);
    }

    [Benchmark]
    public UlsSolveResult FedergruenTzur()
    {
        return _federgruenTzur.Solve(_problem);
    }

    [Benchmark]
    public UlsSolveResult AggarwalPark()
    {
        return _aggarwalPark.Solve(_problem);
    }
}
