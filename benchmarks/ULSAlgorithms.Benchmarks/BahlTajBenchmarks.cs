using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Compares Bahl-Taj's data-dependent planning-horizon pruning with the
/// low-storage Evans implementation.
/// </summary>
[MemoryDiagnoser]
public class BahlTajBenchmarks
{
    private UlsProblem _frequentSetupProblem = null!;
    private UlsProblem _longCycleProblem = null!;

    private BahlTajPlanningHorizonSolver _bahlTaj = null!;
    private WagnerWhitinEvansSolver _evans = null!;
    private WagnerWhitinSolver _linear = null!;

    [Params(100, 500, 1_000, 5_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random =
            new Random(1991 + Horizon);

        var demands =
            new double[Horizon];

        var frequentSetupCosts =
            new double[Horizon];

        var longCycleSetupCosts =
            new double[Horizon];

        var productionCosts =
            new double[Horizon];

        var highHoldingCosts =
            new double[Horizon];

        var lowHoldingCosts =
            new double[Horizon];

        for (var period = 0;
             period < Horizon;
             period++)
        {
            demands[period] =
                random.Next(1, 101);

            frequentSetupCosts[period] =
                random.NextDouble() * 5.0;

            longCycleSetupCosts[period] =
                5_000.0 + (random.NextDouble() * 500.0);

            productionCosts[period] =
                10.0;

            highHoldingCosts[period] =
                20.0 + (random.NextDouble() * 10.0);

            lowHoldingCosts[period] =
                random.NextDouble() * 0.01;
        }

        _frequentSetupProblem =
            new UlsProblem(
                demands,
                frequentSetupCosts,
                productionCosts,
                highHoldingCosts);

        _longCycleProblem =
            new UlsProblem(
                demands,
                longCycleSetupCosts,
                productionCosts,
                lowHoldingCosts);

        _bahlTaj =
            new BahlTajPlanningHorizonSolver();

        _evans =
            new WagnerWhitinEvansSolver();

        _linear =
            new WagnerWhitinSolver();
    }

    [Benchmark]
    public UlsSolveResult BahlTaj_FrequentSetups()
    {
        return _bahlTaj.Solve(
            _frequentSetupProblem);
    }

    [Benchmark]
    public UlsSolveResult Evans_FrequentSetups()
    {
        return _evans.Solve(
            _frequentSetupProblem);
    }

    [Benchmark]
    public UlsSolveResult WagelmansLinear_FrequentSetups()
    {
        return _linear.Solve(
            _frequentSetupProblem);
    }

    [Benchmark]
    public UlsSolveResult BahlTaj_LongCycles()
    {
        return _bahlTaj.Solve(
            _longCycleProblem);
    }

    [Benchmark]
    public UlsSolveResult Evans_LongCycles()
    {
        return _evans.Solve(
            _longCycleProblem);
    }

    [Benchmark]
    public UlsSolveResult WagelmansLinear_LongCycles()
    {
        return _linear.Solve(
            _longCycleProblem);
    }
}
