using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Exact.JacobsKhumawala;
using ULSAlgorithms.Exact.SaydamMcKnew;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Exact.Zangwill;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Compares historical O(T²) exact ULS architectures.
/// </summary>
[MemoryDiagnoser]
public class ExactAlgorithmsPackIIBenchmarks
{
    private UlsProblem _problem = null!;

    private SaydamMcKnewFastWagnerWhitinSolver _saydam = null!;
    private JacobsKhumawalaBranchAndBoundSolver _jacobs = null!;
    private ZangwillNetworkSolver _zangwill = null!;
    private WagnerWhitinEvansSolver _evans = null!;

    [Params(100, 500, 1_000, 2_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random =
            new Random(
                1987 +
                Horizon);

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
                random.NextDouble() < 0.10
                    ? 0.0
                    : random.Next(1, 101);

            setupCosts[period] =
                random.Next(10, 501);

            productionCosts[period] =
                random.NextDouble() * 50.0;

            holdingCosts[period] =
                random.NextDouble() * 5.0;
        }

        _problem = new UlsProblem(
            demands,
            setupCosts,
            productionCosts,
            holdingCosts);

        _saydam =
            new SaydamMcKnewFastWagnerWhitinSolver();

        _jacobs =
            new JacobsKhumawalaBranchAndBoundSolver();

        _zangwill =
            new ZangwillNetworkSolver();

        _evans =
            new WagnerWhitinEvansSolver();
    }

    [Benchmark(Baseline = true)]
    public UlsSolveResult Evans() =>
        _evans.Solve(_problem);

    [Benchmark]
    public UlsSolveResult SaydamMcKnew() =>
        _saydam.Solve(_problem);

    [Benchmark]
    public UlsSolveResult JacobsKhumawala() =>
        _jacobs.Solve(_problem);

    [Benchmark]
    public UlsSolveResult Zangwill() =>
        _zangwill.Solve(_problem);
}
