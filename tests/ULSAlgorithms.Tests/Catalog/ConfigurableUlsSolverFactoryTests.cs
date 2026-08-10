using ULSAlgorithms.Catalog;
using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.Exact.CuttingPlanes;
using ULSAlgorithms.Exact.Formulations;
using ULSAlgorithms.Exact.Parallel;
using ULSAlgorithms.Optimization;
using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Selection;
using Xunit;

namespace ULSAlgorithms.Tests.Catalog;

public sealed class ConfigurableUlsSolverFactoryTests
{
    [Fact]
    public void Catalog_ExposesEightConfigurableStrategies()
    {
        Assert.Equal(8, UlsSolverCatalog.Configurable.Count);

        Assert.All(
            UlsSolverCatalog.Configurable,
            descriptor => Assert.True(descriptor.SupportsConfiguration));
    }

    [Fact]
    public void AdaptiveFactory_CanSelectFedergruenTzurFallback()
    {
        var solver = Assert.IsType<AdaptiveExactUlsSolver>(
            UlsSolverFactory.Create(
                "adaptive-exact",
                new UlsSolverCreationOptions
                {
                    AdaptiveGeneralFallback =
                        UlsGeneralExactFallback.FedergruenTzurGeneral
                }));

        Assert.Equal(
            UlsGeneralExactFallback.FedergruenTzurGeneral,
            solver.Fallback);
    }

    [Fact]
    public void ParallelFactory_CanConfigureLyuLee()
    {
        var solver = Assert.IsType<LyuLeeParallelSolver>(
            UlsSolverFactory.Create(
                "lyu-lee-parallel",
                new UlsSolverCreationOptions
                {
                    MaxDegreeOfParallelism = 4,
                    ParallelThreshold = 256
                }));

        Assert.Equal(4, solver.MaxDegreeOfParallelism);
        Assert.Equal(256, solver.ParallelThreshold);
    }

    [Fact]
    public void FormulationFactory_AcceptsExplicitOptimizationEngine()
    {
        var solver = UlsSolverFactory.Create(
            "aggregate-inventory-formulation",
            new UlsSolverCreationOptions
            {
                OptimizationExecution =
                    new LinearModelSolveOptions
                    {
                        Solver = SolverKind.CoinOrCbc,
                        AllowFallbackWhenExplicit = false
                    }
            });

        Assert.IsType<AggregateInventoryFormulationSolver>(solver);
    }

    [Fact]
    public void CuttingPlaneFactory_AcceptsExecutionAndCutOptions()
    {
        var solver = UlsSolverFactory.Create(
            "general-ls-cutting-plane",
            new UlsSolverCreationOptions
            {
                OptimizationExecution =
                    new LinearModelSolveOptions
                    {
                        Solver = SolverKind.Automatic
                    },
                CuttingPlane =
                    new LsCuttingPlaneOptions
                    {
                        MaximumIterations = 12,
                        MaximumCutsPerIteration = 8
                    }
            });

        Assert.IsType<GeneralLsCuttingPlaneSolver>(solver);
    }

    [Fact]
    public void EmptyOptions_PreserveHistoricalDefaultFactory()
    {
        var direct = UlsSolverFactory.Create(
            "wagelmans-general");

        var configured = UlsSolverFactory.Create(
            "wagelmans-general",
            new UlsSolverCreationOptions());

        Assert.Equal(
            direct.GetType(),
            configured.GetType());
        Assert.Equal(
            direct.Kind,
            configured.Kind);
    }

    [Fact]
    public void IrrelevantOptions_AreRejectedRatherThanIgnored()
    {
        Assert.Throws<ArgumentException>(() =>
            UlsSolverFactory.Create(
                "wagelmans-general",
                new UlsSolverCreationOptions
                {
                    AdaptiveGeneralFallback =
                        UlsGeneralExactFallback.FedergruenTzurGeneral
                }));
    }

    [Fact]
    public void CuttingPlaneOptions_AreRejectedForPlainFormulation()
    {
        Assert.Throws<ArgumentException>(() =>
            UlsSolverFactory.Create(
                "aggregate-inventory-formulation",
                new UlsSolverCreationOptions
                {
                    CuttingPlane =
                        new LsCuttingPlaneOptions()
                }));
    }

    [Fact]
    public void InvalidParallelSettings_AreRejectedAtCreation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            UlsSolverFactory.Create(
                "lyu-lee-parallel",
                new UlsSolverCreationOptions
                {
                    MaxDegreeOfParallelism = 0
                }));
    }

    [Fact]
    public void InvalidOptimizationOptions_AreRejectedAtCreation()
    {
        Assert.Throws<InvalidOperationException>(() =>
            UlsSolverFactory.Create(
                "facility-location-formulation",
                new UlsSolverCreationOptions
                {
                    OptimizationExecution =
                        new LinearModelSolveOptions
                        {
                            Solver = SolverKind.Unknown
                        }
                }));
    }

    [Fact]
    public void Descriptor_CreateSupportsConfiguredPath()
    {
        var descriptor =
            UlsSolverCatalog.Get("adaptive-exact");

        var solver = Assert.IsType<AdaptiveExactUlsSolver>(
            descriptor.Create(
                new UlsSolverCreationOptions
                {
                    AdaptiveGeneralFallback =
                        UlsGeneralExactFallback.FedergruenTzurGeneral
                }));

        Assert.Equal(
            UlsGeneralExactFallback.FedergruenTzurGeneral,
            solver.Fallback);
    }

    [Fact]
    public void TryCreate_UnknownIdStillReturnsFalseWithOptions()
    {
        var created = UlsSolverFactory.TryCreate(
            "not-a-solver",
            new UlsSolverCreationOptions
            {
                MaxDegreeOfParallelism = 2
            },
            out var solver);

        Assert.False(created);
        Assert.Null(solver);
    }

    [Fact]
    public void CapabilityMetadata_MatchesConfiguredStrategyFamilies()
    {
        Assert.Equal(
            UlsSolverConfigurationCapabilities.AdaptiveGeneralFallback,
            UlsSolverCatalog
                .Get("adaptive-exact")
                .ConfigurationCapabilities);

        Assert.Equal(
            UlsSolverConfigurationCapabilities.Parallelism,
            UlsSolverCatalog
                .Get("lyu-lee-parallel")
                .ConfigurationCapabilities);

        Assert.Equal(
            UlsSolverConfigurationCapabilities.OptimizationExecution,
            UlsSolverCatalog
                .Get("aggregate-inventory-formulation")
                .ConfigurationCapabilities);

        Assert.Equal(
            UlsSolverConfigurationCapabilities.OptimizationExecution |
            UlsSolverConfigurationCapabilities.CuttingPlane,
            UlsSolverCatalog
                .Get("general-ls-cutting-plane")
                .ConfigurationCapabilities);
    }
}
