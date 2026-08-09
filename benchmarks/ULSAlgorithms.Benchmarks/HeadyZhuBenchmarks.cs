using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Compares the fixed-cost Heady-Zhu Economic-Part-Period implementation with
/// other public Wagner-Whitin-family exact solvers.
/// </summary>
[MemoryDiagnoser]
public class HeadyZhuBenchmarks
{
    private UlsProblem _strongPruningProblem = null!;
    private UlsProblem _weakPruningProblem = null!;

    private HeadyZhuEconomicPartPeriodSolver _headyZhu = null!;
    private BahlTajPlanningHorizonSolver _bahlTaj = null!;
    private WagnerWhitinEvansSolver _evans = null!;
    private WagnerWhitinSolver _wagelmansLinear = null!;

    [Params(100, 500, 1_000, 5_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random =
            new Random(1994 + Horizon);

        var demands =
            new double[Horizon];

        for (var period = 0;
             period < Horizon;
             period++)
        {
            demands[period] =
                random.Next(1, 101);
        }

        var strongSetup =
            Enumerable.Repeat(10.0, Horizon).ToArray();

        var weakSetup =
            Enumerable.Repeat(10_000.0, Horizon).ToArray();

        var production =
            Enumerable.Repeat(5.0, Horizon).ToArray();

        var strongHolding =
            Enumerable.Repeat(10.0, Horizon).ToArray();

        var weakHolding =
            Enumerable.Repeat(0.01, Horizon).ToArray();

        _strongPruningProblem =
            new UlsProblem(
                demands,
                strongSetup,
                production,
                strongHolding);

        _weakPruningProblem =
            new UlsProblem(
                demands,
                weakSetup,
                production,
                weakHolding);

        _headyZhu =
            new HeadyZhuEconomicPartPeriodSolver();

        _bahlTaj =
            new BahlTajPlanningHorizonSolver();

        _evans =
            new WagnerWhitinEvansSolver();

        _wagelmansLinear =
            new WagnerWhitinSolver();
    }

    [Benchmark]
    public UlsSolveResult HeadyZhu_StrongPruning()
    {
        return _headyZhu.Solve(
            _strongPruningProblem);
    }

    [Benchmark]
    public UlsSolveResult BahlTaj_StrongPruning()
    {
        return _bahlTaj.Solve(
            _strongPruningProblem);
    }

    [Benchmark]
    public UlsSolveResult Evans_StrongPruning()
    {
        return _evans.Solve(
            _strongPruningProblem);
    }

    [Benchmark]
    public UlsSolveResult WagelmansLinear_StrongPruning()
    {
        return _wagelmansLinear.Solve(
            _strongPruningProblem);
    }

    [Benchmark]
    public UlsSolveResult HeadyZhu_WeakPruning()
    {
        return _headyZhu.Solve(
            _weakPruningProblem);
    }

    [Benchmark]
    public UlsSolveResult BahlTaj_WeakPruning()
    {
        return _bahlTaj.Solve(
            _weakPruningProblem);
    }

    [Benchmark]
    public UlsSolveResult Evans_WeakPruning()
    {
        return _evans.Solve(
            _weakPruningProblem);
    }

    [Benchmark]
    public UlsSolveResult WagelmansLinear_WeakPruning()
    {
        return _wagelmansLinear.Solve(
            _weakPruningProblem);
    }
}
