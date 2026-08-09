using BenchmarkDotNet.Attributes;

namespace ULSAlgorithms.Benchmarks;

/// <summary>
/// Bootstrap benchmark used only to validate the benchmark harness.
/// </summary>
/// <remarks>
/// It must be replaced by algorithmic benchmarks once ULS solvers are introduced.
/// </remarks>
public class InfrastructureBenchmark
{
    [Benchmark]
    public string ReadInformationalVersion()
    {
        return ULSAlgorithmsInfo.InformationalVersion;
    }
}
