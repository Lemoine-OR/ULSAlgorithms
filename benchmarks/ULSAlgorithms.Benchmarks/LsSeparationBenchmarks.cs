using BenchmarkDotNet.Attributes;
using ULSAlgorithms.CuttingPlanes.Separation;
using ULSAlgorithms.Formulations;
using ULSAlgorithms.Formulations.Aggregate;
using ULSAlgorithms.Models;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Measures pure (l,S) separation cost independently of external solver time.
/// </summary>
[MemoryDiagnoser]
public class LsSeparationBenchmarks
{
    private UlsProblem _problem = null!;
    private UlsFormulation _formulation = null!;
    private Dictionary<int, double> _values = null!;
    private GeneralLsCutSeparator _general = null!;
    private WagnerWhitinLsCutSeparator _wagnerWhitin = null!;

    [Params(50, 100, 250, 500)]
    public int Horizon { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random =
            new Random(
                1984 + Horizon);

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
                random.Next(1, 101);

            setup[period] =
                random.Next(25, 501);

            production[period] =
                0.0;

            holding[period] =
                period == Horizon - 1
                    ? 0.0
                    : 1.0 +
                      4.0 *
                      random.NextDouble();
        }

        _problem =
            new UlsProblem(
                demand,
                setup,
                production,
                holding);

        _formulation =
            new AggregateInventoryFormulationBuilder()
                .Build(_problem);

        _values =
            _formulation.Model.Variables
                .ToDictionary(
                    static variable =>
                        variable.Id,
                    static _ =>
                        0.0);

        for (int period = 0;
             period < Horizon;
             period++)
        {
            _values[
                _formulation.Variables.Production[period]] =
                demand[period];

            _values[
                _formulation.Variables.Setup[period]] =
                0.25 +
                0.5 *
                random.NextDouble();
        }

        _general =
            new GeneralLsCutSeparator();

        _wagnerWhitin =
            new WagnerWhitinLsCutSeparator();
    }

    [Benchmark(Baseline = true)]
    public int GeneralExactSeparation()
    {
        return _general
            .Separate(
                _problem,
                _formulation,
                _values)
            .Count;
    }

    [Benchmark]
    public int WagnerWhitinSeparation()
    {
        return _wagnerWhitin
            .Separate(
                _problem,
                _formulation,
                _values)
            .Count;
    }
}
