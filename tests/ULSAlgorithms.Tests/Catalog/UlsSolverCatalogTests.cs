using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Catalog;
using ULSAlgorithms.Selection;
using Xunit;

namespace ULSAlgorithms.Tests.Catalog;

public sealed class UlsSolverCatalogTests
{
    [Fact]
    public void Catalog_HasExpectedPublicInventory()
    {
        Assert.Equal(42, UlsSolverCatalog.All.Count);
        Assert.Equal(23, UlsSolverCatalog.Exact.Count);
        Assert.Equal(17, UlsSolverCatalog.DirectExact.Count);
        Assert.Equal(4, UlsSolverCatalog.Formulations.Count);
        Assert.Equal(2, UlsSolverCatalog.CuttingPlanes.Count);
        Assert.Equal(19, UlsSolverCatalog.Heuristics.Count);
    }

    [Fact]
    public void Catalog_IdsAndTypesAreUnique()
    {
        Assert.Equal(
            UlsSolverCatalog.All.Count,
            UlsSolverCatalog.All
                .Select(descriptor => descriptor.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());

        Assert.Equal(
            UlsSolverCatalog.All.Count,
            UlsSolverCatalog.All
                .Select(descriptor => descriptor.ImplementationType)
                .Distinct()
                .Count());
    }

    [Fact]
    public void Factory_CreatesEveryCatalogStrategyWithMatchingContract()
    {
        foreach (var descriptor in UlsSolverCatalog.All)
        {
            var solver = UlsSolverFactory.Create(descriptor.Id);

            Assert.Equal(descriptor.ImplementationType, solver.GetType());
            Assert.Equal(descriptor.Kind, solver.Kind);
        }
    }

    [Fact]
    public void Catalog_ExternalSolverMetadataMatchesOperationalCategories()
    {
        var external = UlsSolverCatalog.All
            .Where(descriptor => descriptor.RequiresExternalSolver)
            .ToArray();

        Assert.Equal(6, external.Length);
        Assert.All(
            external,
            descriptor => Assert.True(
                descriptor.Category is
                    UlsSolverCategory.OptimizationFormulation or
                    UlsSolverCategory.CuttingPlane));

        Assert.All(
            UlsSolverCatalog.DirectExact,
            descriptor => Assert.False(descriptor.RequiresExternalSolver));

        Assert.All(
            UlsSolverCatalog.Heuristics,
            descriptor => Assert.False(descriptor.RequiresExternalSolver));
    }

    [Fact]
    public void Factory_UsesCaseInsensitiveStableIdentifiers()
    {
        var solver = UlsSolverFactory.Create("WAGELMANS-GENERAL");

        Assert.Equal(
            UlsSolverCatalog.Get("wagelmans-general").ImplementationType,
            solver.GetType());
    }

    [Fact]
    public void Factory_UnknownIdentifierHasPredictableBehavior()
    {
        Assert.False(
            UlsSolverFactory.TryCreate(
                "not-a-solver",
                out var solver));

        Assert.Null(solver);

        Assert.Throws<KeyNotFoundException>(() =>
            UlsSolverFactory.Create("not-a-solver"));
    }

    [Fact]
    public void RecommendedExact_IsAdaptiveSelector()
    {
        var descriptor = UlsSolverCatalog.RecommendedExact;

        Assert.Equal("adaptive-exact", descriptor.Id);
        Assert.Equal(UlsSolverKind.Exact, descriptor.Kind);
        Assert.Equal(
            typeof(AdaptiveExactUlsSolver),
            descriptor.ImplementationType);
        Assert.False(descriptor.RequiresExternalSolver);
    }

    [Fact]
    public void Catalog_MetadataIsComplete()
    {
        foreach (var descriptor in UlsSolverCatalog.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(descriptor.Id));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.Name));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.Family));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.TimeComplexity));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.SpaceComplexity));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.Applicability));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.ScientificReference));
            Assert.False(string.IsNullOrWhiteSpace(descriptor.Implementation));
            Assert.EndsWith(
                ".cs",
                descriptor.SourcePath,
                StringComparison.Ordinal);
            Assert.True(
                typeof(IUlsSolver).IsAssignableFrom(
                    descriptor.ImplementationType));
        }
    }
}
