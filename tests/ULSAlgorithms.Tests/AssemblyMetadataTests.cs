using System.Reflection;
using Xunit;

namespace ULSAlgorithms.Tests;

/// <summary>
/// Validates the repository and versioning metadata carried by the public assembly.
/// </summary>
public sealed class AssemblyMetadataTests
{
    private static readonly Assembly Assembly = typeof(ULSAlgorithmsInfo).Assembly;

    [Fact]
    public void AssemblyName_IsULSAlgorithms()
    {
        Assert.Equal("ULSAlgorithms", Assembly.GetName().Name);
        Assert.Equal("ULSAlgorithms", ULSAlgorithmsInfo.Name);
    }

    [Fact]
    public void AssemblyVersion_IsDefined()
    {
        Assert.NotNull(Assembly.GetName().Version);
        Assert.NotEqual(new Version(0, 0, 0, 0), ULSAlgorithmsInfo.Version);
    }

    [Fact]
    public void InformationalVersion_IsDefined()
    {
        Assert.False(string.IsNullOrWhiteSpace(ULSAlgorithmsInfo.InformationalVersion));
    }

    [Fact]
    public void ProductMetadata_IsULSAlgorithms()
    {
        var product = Assembly.GetCustomAttribute<AssemblyProductAttribute>();

        Assert.NotNull(product);
        Assert.Equal("ULSAlgorithms", product.Product);
    }

    [Fact]
    public void RepositoryMetadata_PointsToCanonicalRepository()
    {
        var repository = Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(attribute => attribute.Key == "Repository");

        Assert.NotNull(repository);
        Assert.Equal(
            "https://github.com/Lemoine-OR/ULSAlgorithms",
            repository.Value);
    }
}
