using BenchmarkDotNet.Attributes;
using ULSAlgorithms.Exact.FedergruenTzur;
using ULSAlgorithms.Exact.Wagelmans;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Compares the two Federgruen-Tzur O(n) specializations with compatible
/// exact solvers.
/// </summary>
[MemoryDiagnoser]
public class FedergruenTzurLinearBenchmarks
{
    private UlsProblem _noSpeculativeProblem = null!;
    private UlsProblem _nondecreasingSetupProblem = null!;

    private FedergruenTzurNoSpeculativeMotiveSolver _ftNoSpec = null!;
    private FedergruenTzurNondecreasingSetupSolver _ftNondecreasingSetup = null!;
    private FedergruenTzurSolver _ftGeneral = null!;
    private WagnerWhitinSolver _wagelmansLinear = null!;
    private WagelmansGeneralSolver _wagelmansGeneral = null!;

    [Params(100, 1_000, 10_000)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(1991 + Horizon);

        var demands = new double[Horizon];
        var setupCosts = new double[Horizon];
        var wwProductionCosts = new double[Horizon];
        var generalProductionCosts = new double[Horizon];
        var holdingCosts = new double[Horizon];

        var setup = 10.0;

        for (var period = 0; period < Horizon; period++)
        {
            demands[period] = random.Next(1, 101);

            setup += random.NextDouble() * 5.0;
            setupCosts[period] = setup;

            wwProductionCosts[period] = 10.0;
            generalProductionCosts[period] = random.NextDouble() * 100.0;
            holdingCosts[period] = random.NextDouble() * 5.0;
        }

        _noSpeculativeProblem = new UlsProblem(
            demands,
            setupCosts,
            wwProductionCosts,
            holdingCosts);

        _nondecreasingSetupProblem = new UlsProblem(
            demands,
            setupCosts,
            generalProductionCosts,
            holdingCosts);

        _ftNoSpec = new FedergruenTzurNoSpeculativeMotiveSolver();
        _ftNondecreasingSetup = new FedergruenTzurNondecreasingSetupSolver();
        _ftGeneral = new FedergruenTzurSolver();
        _wagelmansLinear = new WagnerWhitinSolver();
        _wagelmansGeneral = new WagelmansGeneralSolver();
    }

    [Benchmark]
    public UlsSolveResult FedergruenTzurNoSpeculative()
    {
        return _ftNoSpec.Solve(_noSpeculativeProblem);
    }

    [Benchmark]
    public UlsSolveResult WagelmansLinear()
    {
        return _wagelmansLinear.Solve(_noSpeculativeProblem);
    }

    [Benchmark]
    public UlsSolveResult FedergruenTzurGeneralOnNoSpeculative()
    {
        return _ftGeneral.Solve(_noSpeculativeProblem);
    }

    [Benchmark]
    public UlsSolveResult FedergruenTzurNondecreasingSetup()
    {
        return _ftNondecreasingSetup.Solve(_nondecreasingSetupProblem);
    }

    [Benchmark]
    public UlsSolveResult WagelmansGeneralOnNondecreasingSetup()
    {
        return _wagelmansGeneral.Solve(_nondecreasingSetupProblem);
    }

    [Benchmark]
    public UlsSolveResult FedergruenTzurGeneralOnNondecreasingSetup()
    {
        return _ftGeneral.Solve(_nondecreasingSetupProblem);
    }
}
