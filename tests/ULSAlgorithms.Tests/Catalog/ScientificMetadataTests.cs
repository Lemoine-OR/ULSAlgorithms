using ULSAlgorithms.Catalog;
using Xunit;

namespace ULSAlgorithms.Tests.Catalog;

public sealed class ScientificMetadataTests
{
    [Fact]
    public void EveryPublicStrategy_HasScientificProvenanceMetadata()
    {
        Assert.Equal(
            42,
            UlsSolverCatalog.All.Count);

        foreach (var descriptor in UlsSolverCatalog.All)
        {
            Assert.False(
                string.IsNullOrWhiteSpace(
                    descriptor.ScientificReference));

            if (string.IsNullOrWhiteSpace(
                    descriptor.Doi))
            {
                continue;
            }

            Assert.StartsWith(
                "10.",
                descriptor.Doi,
                StringComparison.Ordinal);

            Assert.DoesNotContain(
                "https://",
                descriptor.Doi,
                StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void AggarwalPark_MetadataUsesPublishedOperationsResearchCitation()
    {
        var descriptor =
            UlsSolverCatalog.Get(
                "aggarwal-park");

        Assert.Equal(
            "10.1287/opre.41.3.549",
            descriptor.Doi);

        Assert.Contains(
            "1993",
            descriptor.ScientificReference,
            StringComparison.Ordinal);

        Assert.Contains(
            "Improved Algorithms for Economic Lot Size Problems",
            descriptor.ScientificReference,
            StringComparison.Ordinal);
    }
}
