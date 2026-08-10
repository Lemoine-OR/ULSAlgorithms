using ULSAlgorithms.Optimization;
using ULSAlgorithms.Optimization.Adapters.CoinOrCbc;
using ULSAlgorithms.Optimization.Adapters.Cplex;
using ULSAlgorithms.Optimization.Adapters.Gurobi;
using ULSAlgorithms.Optimization.Adapters.Xpress;
using Xunit;

namespace ULSAlgorithms.Tests.Optimization;

public sealed class ConcreteSolverAdapterTests
{
    [Fact]
    public void DefaultRegistry_ContainsExactlyFourConcreteAdapters()
    {
        SolverAdapterRegistry registry =
            DefaultSolverAdapterRegistry.Create();

        Assert.Equal(4, registry.Count);
        Assert.Single(
            registry.FindBySolverKind(
                SolverKind.Cplex));
        Assert.Single(
            registry.FindBySolverKind(
                SolverKind.Gurobi));
        Assert.Single(
            registry.FindBySolverKind(
                SolverKind.Xpress));
        Assert.Single(
            registry.FindBySolverKind(
                SolverKind.CoinOrCbc));
    }

    [Fact]
    public void DiscoveryPriority_MatchesLotSizingDataModel()
    {
        Assert.Equal(
            [
                SolverKind.Cplex,
                SolverKind.Gurobi,
                SolverKind.Xpress,
                SolverKind.CoinOrCbc
            ],
            OptimizationSolverDiscovery.DefaultPriority);
    }

    [Fact]
    public void ConcreteAdapters_ReportExpectedSolverKinds()
    {
        IOptimizationSolverAdapter[] adapters =
        [
            new CplexSolverAdapter(),
            new GurobiSolverAdapter(),
            new XpressSolverAdapter(),
            new CoinOrCbcSolverAdapter()
        ];

        Assert.Equal(
            [
                SolverKind.Cplex,
                SolverKind.Gurobi,
                SolverKind.Xpress,
                SolverKind.CoinOrCbc
            ],
            adapters.Select(
                static adapter =>
                    adapter.SolverKind));
    }

    [Fact]
    public void AllConcreteAdapters_SupportLpAndMilp()
    {
        IOptimizationSolverAdapter[] adapters =
        [
            new CplexSolverAdapter(),
            new GurobiSolverAdapter(),
            new XpressSolverAdapter(),
            new CoinOrCbcSolverAdapter()
        ];

        foreach (IOptimizationSolverAdapter adapter in adapters)
        {
            Assert.True(
                adapter.SupportsCapability(
                    SolverCapability.LinearProgramming));

            Assert.True(
                adapter.SupportsCapability(
                    SolverCapability.MixedIntegerLinearProgramming));
        }
    }

    [Fact]
    public void CplexRootResolver_FindsExpectedRuntimeLayout()
    {
        string root =
            Path.Combine(
                Path.GetTempPath(),
                "uls-cplex-" +
                Guid.NewGuid().ToString("N"));

        string runtime =
            Path.Combine(
                root,
                "cplex",
                "bin",
                "x64_win64");

        Directory.CreateDirectory(runtime);

        try
        {
            File.WriteAllBytes(
                Path.Combine(
                    runtime,
                    "ILOG.Concert.dll"),
                []);

            File.WriteAllBytes(
                Path.Combine(
                    runtime,
                    "ILOG.CPLEX.dll"),
                []);

            CplexInstallationInfo? installation =
                CplexInstallationLocator.TryResolveRoot(
                    root);

            Assert.NotNull(installation);
            Assert.Equal(
                Path.GetFullPath(root),
                installation.RootDirectory);
        }
        finally
        {
            Directory.Delete(
                root,
                recursive: true);
        }
    }
}
