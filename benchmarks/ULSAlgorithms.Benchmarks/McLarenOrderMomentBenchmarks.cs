using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

[MemoryDiagnoser]
public class McLarenOrderMomentBenchmarks
{

    private UlsProblem _problem = null!;

    [Params(50, 100, 250, 500)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random =
            new Random(2300 + Horizon);

        var demand =
            new double[Horizon];

        var setup =
            new double[Horizon];

        var production =
            new double[Horizon];

        var holding =
            new double[Horizon];

        for (int period = 0;
             period < Horizon;
             period++)
        {
            demand[period] =
                random.NextDouble() < 0.25
                    ? 0.0
                    : random.Next(1, 101);

            setup[period] = 200.0;
            production[period] = 0.0;
            holding[period] = 4.0;
        }

        _problem =
            new UlsProblem(
                demand,
                setup,
                production,
                holding);
    }

    [Benchmark(Baseline = true)]
    public UlsSolveResult PartPeriodBalancing() =>
        new PartPeriodBalancingSolver().Solve(_problem);

    [Benchmark]
    public UlsSolveResult McLarenOrderMoment() =>
        new McLarenOrderMomentSolver().Solve(_problem);
}
