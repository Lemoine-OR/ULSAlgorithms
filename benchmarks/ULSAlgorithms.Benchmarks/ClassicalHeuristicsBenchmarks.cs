using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Heuristics;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Scaling benchmark for the first classical ULS heuristic pack.
/// </summary>
[MemoryDiagnoser]
public class ClassicalHeuristicsBenchmarks
{
    private UlsProblem _problem = null!;

    private LotForLotSolver _l4l = null!;
    private SilverMealSolver _silverMeal = null!;
    private LeastUnitCostSolver _luc = null!;
    private PartPeriodBalancingSolver _ppb = null!;
    private GroffSolver _groff = null!;
    private PeriodicOrderQuantitySolver _poq = null!;

    [Params(100, 1_000, 10_000, 100_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(1973 + Horizon);

        var demands = new double[Horizon];

        for (var period = 0; period < Horizon; period++)
        {
            demands[period] =
                random.NextDouble() < 0.10
                    ? 0.0
                    : random.Next(1, 101);
        }

        _problem = new UlsProblem(
            demands,
            Enumerable.Repeat(100.0, Horizon).ToArray(),
            Enumerable.Repeat(5.0, Horizon).ToArray(),
            Enumerable.Repeat(1.0, Horizon).ToArray());

        _l4l = new LotForLotSolver();
        _silverMeal = new SilverMealSolver();
        _luc = new LeastUnitCostSolver();
        _ppb = new PartPeriodBalancingSolver();
        _groff = new GroffSolver();
        _poq = new PeriodicOrderQuantitySolver();
    }

    [Benchmark]
    public UlsSolveResult LotForLot() => _l4l.Solve(_problem);

    [Benchmark]
    public UlsSolveResult SilverMeal() => _silverMeal.Solve(_problem);

    [Benchmark]
    public UlsSolveResult LeastUnitCost() => _luc.Solve(_problem);

    [Benchmark]
    public UlsSolveResult PartPeriodBalancing() => _ppb.Solve(_problem);

    [Benchmark]
    public UlsSolveResult Groff() => _groff.Solve(_problem);

    [Benchmark]
    public UlsSolveResult PeriodicOrderQuantity() => _poq.Solve(_problem);
}
