using ULSAlgorithms.Catalog;
using Xunit;

namespace ULSAlgorithms.Tests.Catalog;

public sealed class ScientificMetadataTests
{
    private static readonly IReadOnlyDictionary<string, ProvenanceExpectation>
        AuditedProvenance =
        new Dictionary<string, ProvenanceExpectation>(
            StringComparer.Ordinal)
        {
            ["adaptive-exact"] =
                new(
                    "Wagelmans, Van Hoesel & Kolen (1992); Federgruen & Tzur (1991)",
                    "10.1287/opre.40.1.S145",
                    "O(T) in the NSM case; O(T log T) in the general case",
                    "O(T)",
                    "All validated classical ULS instances; dispatches from the no-speculative-motive condition",
                    "Selects the linear Wagner-Whitin specialization when applicable; otherwise uses a configurable O(T log T) general exact fallback"),
            ["wagner-whitin-classical"] =
                new(
                    "Wagner & Whitin (1958)",
                    "10.1287/mnsc.5.1.89",
                    "O(T²)",
                    "O(T²)",
                    "Classical ULS / Wagner–Whitin model",
                    "Classical dynamic program"),
            ["wagner-whitin-evans"] =
                new(
                    "Evans (1985)",
                    "10.1016/0272-6963(85)90009-9",
                    "O(T²)",
                    "O(T)",
                    "Classical ULS / Wagner–Whitin model",
                    "Low-storage forward DP"),
            ["wagner-whitin-linear"] =
                new(
                    "Wagelmans, Van Hoesel & Kolen (1992)",
                    "10.1287/opre.40.1.S145",
                    "O(T)",
                    "O(T)",
                    "No speculative motive / Wagner–Whitin costs",
                    "Linear convex-hull specialization"),
            ["wagelmans-general"] =
                new(
                    "Wagelmans, Van Hoesel & Kolen (1992)",
                    "10.1287/opre.40.1.S145",
                    "O(T log T)",
                    "O(T)",
                    "General time-varying ULS costs",
                    "General geometric dynamic program"),
            ["federgruen-tzur-general"] =
                new(
                    "Federgruen & Tzur (1991)",
                    "10.1287/mnsc.37.8.909",
                    "O(T log T)",
                    "O(T)",
                    "General time-varying ULS costs",
                    "Forward tree-accelerated DP"),
            ["federgruen-tzur-nsm"] =
                new(
                    "Federgruen & Tzur (1991)",
                    "10.1287/mnsc.37.8.909",
                    "O(T)",
                    "O(T)",
                    "No speculative motive",
                    "Linear specialization"),
            ["federgruen-tzur-nondecreasing-setup"] =
                new(
                    "Federgruen & Tzur (1991)",
                    "10.1287/mnsc.37.8.909",
                    "O(T)",
                    "O(T)",
                    "Published restricted nondecreasing-setup case",
                    "Linear restricted specialization"),
            ["aggarwal-park"] =
                new(
                    "Aggarwal & Park (1993), Improved Algorithms for Economic Lot Size Problems, Operations Research 41(3), 549-571",
                    "10.1287/opre.41.3.549",
                    "O(T log T)",
                    "O(T)",
                    "General ULS costs represented by the library",
                    "CDQ + implicit Monge/SMAWK architecture"),
            ["bahl-taj-planning-horizon"] =
                new(
                    "Bahl & Taj (1991)",
                    "10.1016/0360-8352(91)90033-3",
                    "O(T²) worst case",
                    "O(T)",
                    "No speculative motive",
                    "Data-dependent planning-horizon pruning"),
            ["heady-zhu"] =
                new(
                    "Heady & Zhu (1994)",
                    "10.1111/j.1937-5956.1994.tb00109.x",
                    "O(T²) worst case",
                    "O(T)",
                    "Constant setup, production and relevant holding costs",
                    "Planning horizon + economic-part-period pruning"),
            ["chowdhury-baki-azab"] =
                new(
                    "Chowdhury, Baki & Azab (2018)",
                    "10.1016/j.cie.2018.01.010",
                    "O(T)",
                    "O(T)",
                    "Strictly positive demand; stationary relevant holding; constant unit production cost",
                    "Published O(T) active-diagonal algorithm"),
            ["sadjadi-aryanezhad-sadeghi"] =
                new(
                    "Sadjadi, Aryanezhad & Sadeghi (2009)",
                    "",
                    "O(T²) worst case",
                    "O(T)",
                    "Constant setup, production and relevant holding costs",
                    "Incremental pruning + planning horizon"),
            ["lyu-lee-parallel"] =
                new(
                    "Lyu & Lee (2001)",
                    "10.1016/S0360-8352(01)00047-X",
                    "O(T²) work; O(T²/p) ideal parallel candidate span",
                    "O(T)",
                    "General ULS costs",
                    "Modern shared-memory reconstruction"),
            ["saydam-mcknew"] =
                new(
                    "Saydam & McKnew (1987)",
                    "",
                    "O(T²)",
                    "O(T²)",
                    "General ULS costs represented by the library",
                    "Modern contiguous triangular-cost reconstruction"),
            ["jacobs-khumawala"] =
                new(
                    "Jacobs & Khumawala (1987)",
                    "",
                    "O(T²)",
                    "O(T)",
                    "General ULS costs represented by the library",
                    "Modern branch/subproblem reconstruction"),
            ["zangwill-network"] =
                new(
                    "Zangwill (1969)",
                    "10.1287/mnsc.15.9.506",
                    "O(T²)",
                    "O(T)",
                    "Single-echelon no-backlogging ULS represented by the library",
                    "Backward DAG shortest path"),
            ["aggregate-inventory-formulation"] =
                new(
                    "Wagner & Whitin (1958); Brahimi et al. (2006)",
                    "10.1287/mnsc.5.1.89",
                    "Solver-dependent",
                    "O(T) model + solver",
                    "General classical ULS",
                    "Aggregate x/y/I MILP with automatic solver selection"),
            ["facility-location-formulation"] =
                new(
                    "Krarup & Bilde (1977); Brahimi et al. (2006)",
                    "10.1007/978-3-0348-5936-3_10",
                    "Solver-dependent",
                    "O(T²) model + solver",
                    "General classical ULS",
                    "Disaggregated q[t,k]/y formulation"),
            ["shortest-path-formulation"] =
                new(
                    "Zangwill (1969); Brahimi et al. (2006)",
                    "10.1287/mnsc.15.9.506",
                    "Solver-dependent",
                    "O(T²) model + solver",
                    "No speculative motive / Wagner–Whitin costs",
                    "Continuous network-flow formulation with path reconstruction"),
            ["inventory-eliminated-formulation"] =
                new(
                    "Brahimi et al. (2006)",
                    "10.1016/j.ejor.2004.01.054",
                    "Solver-dependent",
                    "O(T) variables + O(T²) coefficients/constraints",
                    "General classical ULS",
                    "Aggregate x/y formulation with inventory algebraically eliminated"),
            ["general-ls-cutting-plane"] =
                new(
                    "Barany, Van Roy & Wolsey (1984)",
                    "10.1007/BFb0121006",
                    "O(T²) separation per root iteration + solver",
                    "O(T) separator + model/cuts",
                    "General classical ULS",
                    "Exact general (l,S) separation + strengthened final MILP"),
            ["wagner-whitin-ls-cutting-plane"] =
                new(
                    "Pochet & Wolsey (1994)",
                    "10.1007/BF01582225",
                    "O(T²) separation per root iteration + solver",
                    "O(T) separator + model/cuts",
                    "No speculative motive / Wagner–Whitin costs",
                    "O(T²) prefix-S Wagner–Whitin separation + strengthened final MILP"),
            ["lot-for-lot"] =
                new(
                    "Classical MRP rule",
                    "",
                    "O(T)",
                    "O(T)",
                    "General ULS costs",
                    "One replenishment per positive-demand period"),
            ["silver-meal"] =
                new(
                    "Silver & Meal (1973)",
                    "",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Least cost per covered period"),
            ["least-unit-cost"] =
                new(
                    "Classical LUC rule",
                    "",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Least relevant cost per unit"),
            ["part-period-balancing"] =
                new(
                    "DeMatteis (1968)",
                    "10.1147/sj.71.0030",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Closest balance to economic part period"),
            ["groff"] =
                new(
                    "Groff (1979)",
                    "",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Marginal setup/holding criterion"),
            ["periodic-order-quantity"] =
                new(
                    "Classical POQ rule",
                    "",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "EOQ-derived replenishment interval"),
            ["freeland-colley"] =
                new(
                    "Freeland & Colley (1982)",
                    "",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Local incremental carrying-cost criterion"),
            ["patterson-laforge-incremental-part-period"] =
                new(
                    "Patterson & LaForge (1985)",
                    "10.1111/j.1745-493X.1985.tb00132.x",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Incremental part-period stopping rule"),
            ["wemmerlov-modified-ppb"] =
                new(
                    "Wemmerlöv (1983)",
                    "10.1016/0272-6963(83)90023-2",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Corrected PPB with ν = 0.5"),
            ["wemmerlov-ppb-lalb"] =
                new(
                    "Wemmerlöv (1983)",
                    "10.1016/0272-6963(83)90023-2",
                    "O(T)",
                    "O(T)",
                    "Stationary costs; strictly positive demand",
                    "PPB with local LALB adjustment"),
            ["wemmerlov-modified-ppb-lalb"] =
                new(
                    "Wemmerlöv (1983)",
                    "10.1016/0272-6963(83)90023-2",
                    "O(T)",
                    "O(T)",
                    "Stationary costs; strictly positive demand",
                    "Corrected PPB + LALB"),
            ["part-period-simplified"] =
                new(
                    "DeMatteis (1968); Baciarello et al. (2013)",
                    "10.5772/56004",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "No-overshoot EPP / Part-Period Simplified rule"),
            ["segerstedt-reformulated-silver-meal"] =
                new(
                    "Segerstedt, Abdul-Jalbar & Samuelsson (2023)",
                    "10.3390/axioms12070661",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Reformulated Silver-Meal over non-zero demand events"),
            ["chiu-modified-least-unit-cost"] =
                new(
                    "Chiu (2004)",
                    "10.1080/09720510.2004.10701115",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Classical LUC plus cost-beneficial final-lot merge"),
            ["chiu-ting-modified-part-period-balancing"] =
                new(
                    "Chiu, Ting & Chiu (2005)",
                    "",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Nearest-EPP PPB plus cost-beneficial final-lot merge"),
            ["ho-chang-solis-net-least-period-cost"] =
                new(
                    "Ho, Chang & Solis (2006)",
                    "10.1057/palgrave.jors.2602076",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Incremental O(T) evaluation of the published nAPC stopping rule; zero-demand periods are excluded from the average denominator"),
            ["ho-chang-solis-improved-net-least-period-cost"] =
                new(
                    "Ho, Chang & Solis (2006)",
                    "10.1057/palgrave.jors.2602076",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Incremental nAPC rule with the published improved tie-breaking stop condition"),
            ["mclaren-order-moment"] =
                new(
                    "McLaren (1977); Baciarello et al. (2013)",
                    "10.5772/56004",
                    "O(T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "EOQ-derived Order Moment Target with part-period accumulation and a final marginal holding/setup test"),
            ["karni-maximum-part-period-gain"] =
                new(
                    "Karni (1981); Baciarello et al. (2013)",
                    "10.5772/56004",
                    "O(T log T)",
                    "O(T)",
                    "Stationary setup, production and relevant holding costs",
                    "Priority-queue acceleration of the published non-forward global smallest-part-period merge rule"),
        };

    [Fact]
    public void EveryPublicStrategy_MatchesAuditedScientificProvenanceBaseline()
    {
        Assert.Equal(
            42,
            UlsSolverCatalog.All.Count);

        Assert.Equal(
            42,
            AuditedProvenance.Count);

        foreach (var descriptor in UlsSolverCatalog.All)
        {
            Assert.True(
                AuditedProvenance.ContainsKey(
                    descriptor.Id),
                $"Strategy '{descriptor.Id}' is missing from the audited provenance baseline.");

            var expected =
                AuditedProvenance[descriptor.Id];

            Assert.Equal(
                expected.ScientificReference,
                descriptor.ScientificReference);

            Assert.Equal(
                expected.Doi,
                descriptor.Doi);

            Assert.Equal(
                expected.TimeComplexity,
                descriptor.TimeComplexity);

            Assert.Equal(
                expected.SpaceComplexity,
                descriptor.SpaceComplexity);

            Assert.Equal(
                expected.Applicability,
                descriptor.Applicability);

            Assert.Equal(
                expected.Implementation,
                descriptor.Implementation);

            Assert.False(
                string.IsNullOrWhiteSpace(
                    descriptor.Family));

            Assert.StartsWith(
    "src/ULSAlgorithms/",
    descriptor.SourcePath,
    StringComparison.Ordinal);

            Assert.EndsWith(
                ".cs",
                descriptor.SourcePath,
                StringComparison.Ordinal);

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

            Assert.DoesNotContain(
                "doi.org/",
                descriptor.Doi,
                StringComparison.OrdinalIgnoreCase);
        }

        foreach (var auditedId in AuditedProvenance.Keys)
        {
            Assert.True(
                UlsSolverCatalog.TryGet(
                    auditedId,
                    out _),
                $"Audited strategy '{auditedId}' is not present in the runtime catalog.");
        }
    }

    [Fact]
    public void ExactlyTenHistoricalOrClassicalEntries_IntentionallyDoNotAssertADoi()
    {
        string[] expectedWithoutDoi =
        [
            "chiu-ting-modified-part-period-balancing",
            "freeland-colley",
            "groff",
            "jacobs-khumawala",
            "least-unit-cost",
            "lot-for-lot",
            "periodic-order-quantity",
            "sadjadi-aryanezhad-sadeghi",
            "saydam-mcknew",
            "silver-meal"
        ];

        var actualWithoutDoi =
            UlsSolverCatalog.All
                .Where(
                    descriptor =>
                        string.IsNullOrWhiteSpace(
                            descriptor.Doi))
                .Select(
                    descriptor =>
                        descriptor.Id)
                .OrderBy(
                    id =>
                        id,
                    StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(
            expectedWithoutDoi,
            actualWithoutDoi);
    }

    [Fact]
    public void Evans_MetadataRecordsPublishedJournalOfOperationsManagementDoi()
    {
        var descriptor =
            UlsSolverCatalog.Get(
                "wagner-whitin-evans");

        Assert.Equal(
            "10.1016/0272-6963(85)90009-9",
            descriptor.Doi);

        Assert.Equal(
            "O(T²)",
            descriptor.TimeComplexity);

        Assert.Equal(
            "O(T)",
            descriptor.SpaceComplexity);
    }

    [Fact]
    public void PartPeriodBalancing_MetadataRecordsDeMatteisDoi()
    {
        var descriptor =
            UlsSolverCatalog.Get(
                "part-period-balancing");

        Assert.Equal(
            "10.1147/sj.71.0030",
            descriptor.Doi);

        Assert.Equal(
            "DeMatteis (1968)",
            descriptor.ScientificReference);
    }

    [Fact]
    public void LyuLee_MetadataSeparatesTotalWorkFromIdealParallelSpan()
    {
        var descriptor =
            UlsSolverCatalog.Get(
                "lyu-lee-parallel");

        Assert.Equal(
            "O(T²) work; O(T²/p) ideal parallel candidate span",
            descriptor.TimeComplexity);

        Assert.Equal(
            "Modern shared-memory reconstruction",
            descriptor.Implementation);
    }

    private sealed record ProvenanceExpectation(
        string ScientificReference,
        string Doi,
        string TimeComplexity,
        string SpaceComplexity,
        string Applicability,
        string Implementation);
}
