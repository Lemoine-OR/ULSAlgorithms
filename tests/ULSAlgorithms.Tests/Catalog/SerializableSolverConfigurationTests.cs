using System.Text.Json;
using ULSAlgorithms.Catalog;
using ULSAlgorithms.CuttingPlanes;
using ULSAlgorithms.Exact.CuttingPlanes;
using ULSAlgorithms.Exact.Parallel;
using ULSAlgorithms.Optimization;
using ULSAlgorithms.Optimization.Execution;
using ULSAlgorithms.Selection;
using Xunit;

namespace ULSAlgorithms.Tests.Catalog;

public sealed class SerializableSolverConfigurationTests
{
    [Fact]
    public void AdaptiveConfiguration_RoundTripsAndCreatesRequestedFallback()
    {
        var original =
            new UlsSolverConfiguration
            {
                SolverId = "adaptive-exact",
                Options =
                    new UlsSolverCreationOptions
                    {
                        AdaptiveGeneralFallback =
                            UlsGeneralExactFallback.FedergruenTzurGeneral
                    }
            };

        var json =
            original.ToJson();

        Assert.Contains(
            "\"schemaVersion\": 1",
            json,
            StringComparison.Ordinal);
        Assert.Contains(
            "\"solverId\": \"adaptive-exact\"",
            json,
            StringComparison.Ordinal);
        Assert.Contains(
            "\"federgruenTzurGeneral\"",
            json,
            StringComparison.Ordinal);

        var loaded =
            UlsSolverConfiguration.ParseJson(json);

        var solver =
            Assert.IsType<AdaptiveExactUlsSolver>(
                UlsSolverFactory.Create(loaded));

        Assert.Equal(
            UlsGeneralExactFallback.FedergruenTzurGeneral,
            solver.Fallback);
    }

    [Fact]
    public void ParallelConfiguration_RoundTrips()
    {
        var configuration =
            new UlsSolverConfiguration
            {
                SolverId = "lyu-lee-parallel",
                Options =
                    new UlsSolverCreationOptions
                    {
                        MaxDegreeOfParallelism = 3,
                        ParallelThreshold = 192
                    }
            };

        var loaded =
            UlsSolverConfiguration.ParseJson(
                configuration.ToJson());

        var solver =
            Assert.IsType<LyuLeeParallelSolver>(
                loaded.CreateSolver());

        Assert.Equal(
            3,
            solver.MaxDegreeOfParallelism);
        Assert.Equal(
            192,
            solver.ParallelThreshold);
    }

    [Fact]
    public void CuttingPlaneConfiguration_RoundTripsAllNestedOptions()
    {
        var configuration =
            new UlsSolverConfiguration
            {
                SolverId = "general-ls-cutting-plane",
                Options =
                    new UlsSolverCreationOptions
                    {
                        OptimizationExecution =
                            new LinearModelSolveOptions
                            {
                                Solver =
                                    SolverKind.CoinOrCbc,
                                AllowFallbackWhenExplicit =
                                    false,
                                FeasibilityTolerance =
                                    1.0e-8,
                                KeepTemporaryFiles =
                                    true
                            },
                        CuttingPlane =
                            new LsCuttingPlaneOptions
                            {
                                MaximumIterations = 17,
                                MinimumEfficacy = 0.02,
                                SelectionPolicy =
                                    CutSelectionPolicy.TopByEfficacy,
                                MaximumCutsPerIteration = 9
                            }
                    }
            };

        var json =
            configuration.ToJson();

        Assert.Contains(
            "\"coinOrCbc\"",
            json,
            StringComparison.Ordinal);
        Assert.Contains(
            "\"topByEfficacy\"",
            json,
            StringComparison.Ordinal);

        var loaded =
            UlsSolverConfiguration.ParseJson(json);

        Assert.IsType<GeneralLsCuttingPlaneSolver>(
            loaded.CreateSolver());

        var execution =
            Assert.IsType<LinearModelSolveOptions>(
                loaded.Options.OptimizationExecution);

        var cuttingPlane =
            Assert.IsType<LsCuttingPlaneOptions>(
                loaded.Options.CuttingPlane);

        Assert.Equal(
            SolverKind.CoinOrCbc,
            execution.Solver);
        Assert.Equal(
            17,
            cuttingPlane.MaximumIterations);
        Assert.Equal(
            CutSelectionPolicy.TopByEfficacy,
            cuttingPlane.SelectionPolicy);
    }

    [Fact]
    public void SaveAndLoadJson_UsesAReproducibleFile()
    {
        var path =
            Path.Combine(
                Path.GetTempPath(),
                $"ulsalgorithms-{Guid.NewGuid():N}.json");

        try
        {
            var configuration =
                new UlsSolverConfiguration
                {
                    SolverId =
                        "wagelmans-general"
                };

            configuration.SaveJson(path);

            var loaded =
                UlsSolverConfiguration.LoadJson(path);

            Assert.Equal(
                "wagelmans-general",
                loaded.SolverId);
            Assert.Equal(
                UlsSolverConfiguration.CurrentSchemaVersion,
                loaded.SchemaVersion);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void MissingSchemaVersion_IsRejected()
    {
        const string json = """
            {
              "solverId": "adaptive-exact",
              "options": {}
            }
            """;

        Assert.Throws<JsonException>(() =>
            UlsSolverConfiguration.ParseJson(json));
    }

    [Fact]
    public void UnknownSchemaVersion_IsRejected()
    {
        const string json = """
            {
              "schemaVersion": 2,
              "solverId": "adaptive-exact",
              "options": {}
            }
            """;

        Assert.Throws<NotSupportedException>(() =>
            UlsSolverConfiguration.ParseJson(json));
    }

    [Fact]
    public void UnknownSolverId_IsRejected()
    {
        const string json = """
            {
              "schemaVersion": 1,
              "solverId": "not-a-solver",
              "options": {}
            }
            """;

        Assert.Throws<KeyNotFoundException>(() =>
            UlsSolverConfiguration.ParseJson(json));
    }

    [Fact]
    public void IncompatibleOptions_AreRejectedAfterDeserialization()
    {
        const string json = """
            {
              "schemaVersion": 1,
              "solverId": "wagelmans-general",
              "options": {
                "parallelThreshold": 64
              }
            }
            """;

        Assert.Throws<ArgumentException>(() =>
            UlsSolverConfiguration.ParseJson(json));
    }

    [Fact]
    public void UnknownJsonProperty_IsRejected()
    {
        const string json = """
            {
              "schemaVersion": 1,
              "solverId": "adaptive-exact",
              "options": {},
              "futureMeaning": true
            }
            """;

        Assert.Throws<JsonException>(() =>
            UlsSolverConfiguration.ParseJson(json));
    }

    [Fact]
    public void IntegerEnums_AreRejected()
    {
        const string json = """
            {
              "schemaVersion": 1,
              "solverId": "adaptive-exact",
              "options": {
                "adaptiveGeneralFallback": 0
              }
            }
            """;

        Assert.Throws<JsonException>(() =>
            UlsSolverConfiguration.ParseJson(json));
    }
}
