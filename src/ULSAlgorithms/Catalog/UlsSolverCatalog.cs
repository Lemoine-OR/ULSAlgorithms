using System.Diagnostics.CodeAnalysis;
using ULSAlgorithms.Abstractions;

namespace ULSAlgorithms.Catalog;

/// <summary>
/// Canonical runtime inventory of every public <see cref="IUlsSolver"/> strategy.
/// </summary>
/// <remarks>
/// <para>
/// This catalog is the source of truth for stable strategy identifiers,
/// categories and public metadata. The repository documentation JSON is a
/// generated projection of this runtime catalog and is validated by CI.
/// </para>
/// <para>
/// The catalog stores default and, where supported, configured factories rather
/// than singleton solver instances. Each construction call returns a fresh
/// strategy instance.
/// </para>
/// </remarks>
public static class UlsSolverCatalog
{
    private static readonly UlsSolverDescriptor[] Descriptors =
    [
        new(
            "adaptive-exact",
            "Adaptive exact selection",
            UlsSolverCategory.DirectExact,
            "Adaptive exact strategy selection",
            "O(T) in the NSM case; O(T log T) in the general case",
            "O(T)",
            "All validated classical ULS instances; dispatches from the no-speculative-motive condition",
            "Wagelmans, Van Hoesel & Kolen (1992), Economic Lot Sizing: An O(n log n) Algorithm That Runs in Linear Time in the Wagner-Whitin Case, Operations Research 40(S1), S145-S156; Federgruen & Tzur (1991), A Simple Forward Algorithm to Solve General Dynamic Lot Sizing Models with n Periods in O(n log n) or O(n) Time, Management Science 37(8), 909-925",
            "10.1287/opre.40.1.S145",
            "Selects the linear Wagner-Whitin specialization when applicable; otherwise uses a configurable O(T log T) general exact fallback",
            "src/ULSAlgorithms/Selection/AdaptiveExactUlsSolver.cs",
            typeof(global::ULSAlgorithms.Selection.AdaptiveExactUlsSolver),
            static () => new global::ULSAlgorithms.Selection.AdaptiveExactUlsSolver(),
            UlsSolverConfigurationCapabilities.AdaptiveGeneralFallback,
            static options => new global::ULSAlgorithms.Selection.AdaptiveExactUlsSolver(
                options.AdaptiveGeneralFallback ??
                global::ULSAlgorithms.Selection.UlsGeneralExactFallback.WagelmansGeneral)),
        new(
            "wagner-whitin-classical",
            "Wagner–Whitin classical",
            UlsSolverCategory.DirectExact,
            "Wagner–Whitin DP",
            "O(T²)",
            "O(T²)",
            "Classical ULS / Wagner–Whitin model",
            "Wagner & Whitin (1958), Dynamic Version of the Economic Lot Size Model, Management Science 5(1), 89-96",
            "10.1287/mnsc.5.1.89",
            "Classical dynamic program",
            "src/ULSAlgorithms/Exact/WagnerWhitin/WagnerWhitinClassicalSolver.cs",
            typeof(global::ULSAlgorithms.Exact.WagnerWhitin.WagnerWhitinClassicalSolver),
            static () => new global::ULSAlgorithms.Exact.WagnerWhitin.WagnerWhitinClassicalSolver()),
        new(
            "wagner-whitin-evans",
            "Wagner–Whitin / Evans",
            UlsSolverCategory.DirectExact,
            "Wagner–Whitin DP",
            "O(T²)",
            "O(T)",
            "Classical ULS / Wagner–Whitin model",
            "Evans (1985), An Efficient Implementation of the Wagner-Whitin Algorithm for Dynamic Lot-Sizing, Journal of Operations Management 5(2), 229-235",
            "10.1016/0272-6963(85)90009-9",
            "Low-storage forward DP",
            "src/ULSAlgorithms/Exact/WagnerWhitin/WagnerWhitinEvansSolver.cs",
            typeof(global::ULSAlgorithms.Exact.WagnerWhitin.WagnerWhitinEvansSolver),
            static () => new global::ULSAlgorithms.Exact.WagnerWhitin.WagnerWhitinEvansSolver()),
        new(
            "wagner-whitin-linear",
            "Wagner–Whitin linear",
            UlsSolverCategory.DirectExact,
            "Geometric DP",
            "O(T)",
            "O(T)",
            "No speculative motive / Wagner–Whitin costs",
            "Wagelmans, Van Hoesel & Kolen (1992), Economic Lot Sizing: An O(n log n) Algorithm That Runs in Linear Time in the Wagner-Whitin Case, Operations Research 40(S1), S145-S156",
            "10.1287/opre.40.1.S145",
            "Linear convex-hull specialization",
            "src/ULSAlgorithms/Exact/WagnerWhitin/WagnerWhitinSolver.cs",
            typeof(global::ULSAlgorithms.Exact.WagnerWhitin.WagnerWhitinSolver),
            static () => new global::ULSAlgorithms.Exact.WagnerWhitin.WagnerWhitinSolver()),
        new(
            "wagelmans-general",
            "Wagelmans general",
            UlsSolverCategory.DirectExact,
            "Geometric DP",
            "O(T log T)",
            "O(T)",
            "General time-varying ULS costs",
            "Wagelmans, Van Hoesel & Kolen (1992), Economic Lot Sizing: An O(n log n) Algorithm That Runs in Linear Time in the Wagner-Whitin Case, Operations Research 40(S1), S145-S156",
            "10.1287/opre.40.1.S145",
            "General geometric dynamic program",
            "src/ULSAlgorithms/Exact/Wagelmans/WagelmansGeneralSolver.cs",
            typeof(global::ULSAlgorithms.Exact.Wagelmans.WagelmansGeneralSolver),
            static () => new global::ULSAlgorithms.Exact.Wagelmans.WagelmansGeneralSolver()),
        new(
            "federgruen-tzur-general",
            "Federgruen–Tzur general",
            UlsSolverCategory.DirectExact,
            "Geometric DP",
            "O(T log T)",
            "O(T)",
            "General time-varying ULS costs",
            "Federgruen & Tzur (1991), A Simple Forward Algorithm to Solve General Dynamic Lot Sizing Models with n Periods in O(n log n) or O(n) Time, Management Science 37(8), 909-925",
            "10.1287/mnsc.37.8.909",
            "Forward tree-accelerated DP",
            "src/ULSAlgorithms/Exact/FedergruenTzur/FedergruenTzurSolver.cs",
            typeof(global::ULSAlgorithms.Exact.FedergruenTzur.FedergruenTzurSolver),
            static () => new global::ULSAlgorithms.Exact.FedergruenTzur.FedergruenTzurSolver()),
        new(
            "federgruen-tzur-nsm",
            "Federgruen–Tzur linear (NSM)",
            UlsSolverCategory.DirectExact,
            "Geometric DP",
            "O(T)",
            "O(T)",
            "No speculative motive",
            "Federgruen & Tzur (1991), A Simple Forward Algorithm to Solve General Dynamic Lot Sizing Models with n Periods in O(n log n) or O(n) Time, Management Science 37(8), 909-925",
            "10.1287/mnsc.37.8.909",
            "Linear specialization",
            "src/ULSAlgorithms/Exact/FedergruenTzur/FedergruenTzurNoSpeculativeMotiveSolver.cs",
            typeof(global::ULSAlgorithms.Exact.FedergruenTzur.FedergruenTzurNoSpeculativeMotiveSolver),
            static () => new global::ULSAlgorithms.Exact.FedergruenTzur.FedergruenTzurNoSpeculativeMotiveSolver()),
        new(
            "federgruen-tzur-nondecreasing-setup",
            "Federgruen–Tzur linear (setup)",
            UlsSolverCategory.DirectExact,
            "Geometric DP",
            "O(T)",
            "O(T)",
            "Published restricted nondecreasing-setup case",
            "Federgruen & Tzur (1991), A Simple Forward Algorithm to Solve General Dynamic Lot Sizing Models with n Periods in O(n log n) or O(n) Time, Management Science 37(8), 909-925",
            "10.1287/mnsc.37.8.909",
            "Linear restricted specialization",
            "src/ULSAlgorithms/Exact/FedergruenTzur/FedergruenTzurNondecreasingSetupSolver.cs",
            typeof(global::ULSAlgorithms.Exact.FedergruenTzur.FedergruenTzurNondecreasingSetupSolver),
            static () => new global::ULSAlgorithms.Exact.FedergruenTzur.FedergruenTzurNondecreasingSetupSolver()),
        new(
            "aggarwal-park",
            "Aggarwal–Park",
            UlsSolverCategory.DirectExact,
            "Monge / geometric DP",
            "O(T log T)",
            "O(T)",
            "General ULS costs represented by the library",
            "Aggarwal & Park (1993), Improved Algorithms for Economic Lot Size Problems, Operations Research 41(3), 549-571",
            "10.1287/opre.41.3.549",
            "CDQ + implicit Monge/SMAWK architecture",
            "src/ULSAlgorithms/Exact/AggarwalPark/AggarwalParkSolver.cs",
            typeof(global::ULSAlgorithms.Exact.AggarwalPark.AggarwalParkSolver),
            static () => new global::ULSAlgorithms.Exact.AggarwalPark.AggarwalParkSolver()),
        new(
            "bahl-taj-planning-horizon",
            "Bahl–Taj planning horizon",
            UlsSolverCategory.DirectExact,
            "Planning-horizon DP",
            "O(T²) worst case",
            "O(T)",
            "No speculative motive",
            "Bahl & Taj (1991), A data-dependent efficient implementation of the Wagner-Whitin algorithm for lot-sizing, Computers & Industrial Engineering 20(2), 289-291",
            "10.1016/0360-8352(91)90033-3",
            "Data-dependent planning-horizon pruning",
            "src/ULSAlgorithms/Exact/WagnerWhitin/BahlTajPlanningHorizonSolver.cs",
            typeof(global::ULSAlgorithms.Exact.WagnerWhitin.BahlTajPlanningHorizonSolver),
            static () => new global::ULSAlgorithms.Exact.WagnerWhitin.BahlTajPlanningHorizonSolver()),
        new(
            "heady-zhu",
            "Heady–Zhu",
            UlsSolverCategory.DirectExact,
            "Planning-horizon DP",
            "O(T²) worst case",
            "O(T)",
            "Constant setup, production and relevant holding costs",
            "Heady & Zhu (1994), An Improved Implementation of the Wagner-Whitin Algorithm, Production and Operations Management 3(1), 55-63",
            "10.1111/j.1937-5956.1994.tb00109.x",
            "Planning horizon + economic-part-period pruning",
            "src/ULSAlgorithms/Exact/WagnerWhitin/HeadyZhuEconomicPartPeriodSolver.cs",
            typeof(global::ULSAlgorithms.Exact.WagnerWhitin.HeadyZhuEconomicPartPeriodSolver),
            static () => new global::ULSAlgorithms.Exact.WagnerWhitin.HeadyZhuEconomicPartPeriodSolver()),
        new(
            "chowdhury-baki-azab",
            "Chowdhury–Baki–Azab",
            UlsSolverCategory.DirectExact,
            "Linear Wagner–Whitin",
            "O(T)",
            "O(T)",
            "Strictly positive demand; stationary relevant holding; constant unit production cost",
            "Chowdhury, Baki & Azab (2018), Dynamic Economic Lot-Sizing Problem: A new O(T) Algorithm for the Wagner-Whitin Model, Computers & Industrial Engineering 117, 6-18",
            "10.1016/j.cie.2018.01.010",
            "Published O(T) active-diagonal algorithm",
            "src/ULSAlgorithms/Exact/ChowdhuryBakiAzab/ChowdhuryBakiAzabSolver.cs",
            typeof(global::ULSAlgorithms.Exact.ChowdhuryBakiAzab.ChowdhuryBakiAzabSolver),
            static () => new global::ULSAlgorithms.Exact.ChowdhuryBakiAzab.ChowdhuryBakiAzabSolver()),
        new(
            "sadjadi-aryanezhad-sadeghi",
            "Sadjadi–Aryanezhad–Sadeghi",
            UlsSolverCategory.DirectExact,
            "Planning-horizon DP",
            "O(T²) worst case",
            "O(T)",
            "Constant setup, production and relevant holding costs",
            "Sadjadi, Aryanezhad & Sadeghi (2009), An Improved Wagner-Whitin Algorithm, International Journal of Industrial Engineering & Production Research 20, 117-123",
            "",
            "Incremental pruning + planning horizon",
            "src/ULSAlgorithms/Exact/WagnerWhitin/SadjadiAryanezhadSadeghiSolver.cs",
            typeof(global::ULSAlgorithms.Exact.WagnerWhitin.SadjadiAryanezhadSadeghiSolver),
            static () => new global::ULSAlgorithms.Exact.WagnerWhitin.SadjadiAryanezhadSadeghiSolver()),
        new(
            "lyu-lee-parallel",
            "Lyu–Lee parallel",
            UlsSolverCategory.DirectExact,
            "Parallel DP",
            "O(T²) work; O(T²/p) ideal parallel candidate span",
            "O(T)",
            "General ULS costs",
            "Lyu & Lee (2001), A Parallel Algorithm for the Dynamic Lot-Sizing Problem",
            "10.1016/S0360-8352(01)00047-X",
            "Modern shared-memory reconstruction",
            "src/ULSAlgorithms/Exact/Parallel/LyuLeeParallelSolver.cs",
            typeof(global::ULSAlgorithms.Exact.Parallel.LyuLeeParallelSolver),
            static () => new global::ULSAlgorithms.Exact.Parallel.LyuLeeParallelSolver(),
            UlsSolverConfigurationCapabilities.Parallelism,
            static options => new global::ULSAlgorithms.Exact.Parallel.LyuLeeParallelSolver(
                options.MaxDegreeOfParallelism ?? -1,
                options.ParallelThreshold ?? 128)),
        new(
            "saydam-mcknew",
            "Saydam–McKnew",
            UlsSolverCategory.DirectExact,
            "Wagner–Whitin DP",
            "O(T²)",
            "O(T²)",
            "General ULS costs represented by the library",
            "Saydam & McKnew (1987), A Fast Microcomputer Program for Ordering Using the Wagner-Whitin Algorithm, Production and Inventory Management 28(4), 15-19",
            "",
            "Modern contiguous triangular-cost reconstruction",
            "src/ULSAlgorithms/Exact/SaydamMcKnew/SaydamMcKnewFastWagnerWhitinSolver.cs",
            typeof(global::ULSAlgorithms.Exact.SaydamMcKnew.SaydamMcKnewFastWagnerWhitinSolver),
            static () => new global::ULSAlgorithms.Exact.SaydamMcKnew.SaydamMcKnewFastWagnerWhitinSolver()),
        new(
            "jacobs-khumawala",
            "Jacobs–Khumawala",
            UlsSolverCategory.DirectExact,
            "Branch and bound",
            "O(T²)",
            "O(T)",
            "General ULS costs represented by the library",
            "Jacobs & Khumawala (1987), A Simplified Procedure for Optimal Single-Level Lot Sizing, Production and Inventory Management 28(3), 39-43",
            "",
            "Modern branch/subproblem reconstruction",
            "src/ULSAlgorithms/Exact/JacobsKhumawala/JacobsKhumawalaBranchAndBoundSolver.cs",
            typeof(global::ULSAlgorithms.Exact.JacobsKhumawala.JacobsKhumawalaBranchAndBoundSolver),
            static () => new global::ULSAlgorithms.Exact.JacobsKhumawala.JacobsKhumawalaBranchAndBoundSolver()),
        new(
            "zangwill-network",
            "Zangwill network",
            UlsSolverCategory.DirectExact,
            "Network / shortest path",
            "O(T²)",
            "O(T)",
            "Single-echelon no-backlogging ULS represented by the library",
            "Zangwill (1969), A Backlogging Model and a Multi-Echelon Model of a Dynamic Economic Lot Size Production System, Management Science 15(9), 506-527",
            "10.1287/mnsc.15.9.506",
            "Backward DAG shortest path",
            "src/ULSAlgorithms/Exact/Zangwill/ZangwillNetworkSolver.cs",
            typeof(global::ULSAlgorithms.Exact.Zangwill.ZangwillNetworkSolver),
            static () => new global::ULSAlgorithms.Exact.Zangwill.ZangwillNetworkSolver()),
        new(
            "aggregate-inventory-formulation",
            "Aggregate inventory formulation",
            UlsSolverCategory.OptimizationFormulation,
            "Solver-backed mathematical formulation",
            "Solver-dependent",
            "O(T) model + solver",
            "General classical ULS",
            "Wagner & Whitin (1958), Dynamic Version of the Economic Lot Size Model, Management Science 5(1), 89-96; Brahimi, Dauzere-Peres, Najid & Nordli (2006), Single Item Lot Sizing Problems, European Journal of Operational Research 168(1), 1-16",
            "10.1287/mnsc.5.1.89",
            "Aggregate x/y/I MILP with automatic solver selection",
            "src/ULSAlgorithms/Exact/Formulations/AggregateInventoryFormulationSolver.cs",
            typeof(global::ULSAlgorithms.Exact.Formulations.AggregateInventoryFormulationSolver),
            static () => new global::ULSAlgorithms.Exact.Formulations.AggregateInventoryFormulationSolver(),
            UlsSolverConfigurationCapabilities.OptimizationExecution,
            static options => new global::ULSAlgorithms.Exact.Formulations.AggregateInventoryFormulationSolver(
                options.OptimizationExecution)),
        new(
            "facility-location-formulation",
            "Facility-location formulation",
            UlsSolverCategory.OptimizationFormulation,
            "Solver-backed mathematical formulation",
            "Solver-dependent",
            "O(T²) model + solver",
            "General classical ULS",
            "Krarup & Bilde (1977), Plant Location, Set Covering and Economic Lot Size: An O(nm)-Algorithm for Structured Problems; Brahimi, Dauzere-Peres, Najid & Nordli (2006), Single Item Lot Sizing Problems, European Journal of Operational Research 168(1), 1-16",
            "10.1007/978-3-0348-5936-3_10",
            "Disaggregated q[t,k]/y formulation",
            "src/ULSAlgorithms/Exact/Formulations/FacilityLocationFormulationSolver.cs",
            typeof(global::ULSAlgorithms.Exact.Formulations.FacilityLocationFormulationSolver),
            static () => new global::ULSAlgorithms.Exact.Formulations.FacilityLocationFormulationSolver(),
            UlsSolverConfigurationCapabilities.OptimizationExecution,
            static options => new global::ULSAlgorithms.Exact.Formulations.FacilityLocationFormulationSolver(
                options.OptimizationExecution)),
        new(
            "shortest-path-formulation",
            "Shortest-path formulation",
            UlsSolverCategory.OptimizationFormulation,
            "Solver-backed network formulation",
            "Solver-dependent",
            "O(T²) model + solver",
            "No speculative motive / Wagner–Whitin costs",
            "Zangwill (1969), A Backlogging Model and a Multi-Echelon Model of a Dynamic Economic Lot Size Production System, Management Science 15(9), 506-527; Brahimi, Dauzere-Peres, Najid & Nordli (2006), Single Item Lot Sizing Problems, European Journal of Operational Research 168(1), 1-16",
            "10.1287/mnsc.15.9.506",
            "Continuous network-flow formulation with path reconstruction",
            "src/ULSAlgorithms/Exact/Formulations/ShortestPathFormulationSolver.cs",
            typeof(global::ULSAlgorithms.Exact.Formulations.ShortestPathFormulationSolver),
            static () => new global::ULSAlgorithms.Exact.Formulations.ShortestPathFormulationSolver(),
            UlsSolverConfigurationCapabilities.OptimizationExecution,
            static options => new global::ULSAlgorithms.Exact.Formulations.ShortestPathFormulationSolver(
                options.OptimizationExecution)),
        new(
            "inventory-eliminated-formulation",
            "Inventory-eliminated formulation",
            UlsSolverCategory.OptimizationFormulation,
            "Solver-backed mathematical formulation",
            "Solver-dependent",
            "O(T) variables + O(T²) coefficients/constraints",
            "General classical ULS",
            "Brahimi, Dauzere-Peres, Najid & Nordli (2006), Single Item Lot Sizing Problems, European Journal of Operational Research 168(1), 1-16",
            "10.1016/j.ejor.2004.01.054",
            "Aggregate x/y formulation with inventory algebraically eliminated",
            "src/ULSAlgorithms/Exact/Formulations/InventoryEliminatedFormulationSolver.cs",
            typeof(global::ULSAlgorithms.Exact.Formulations.InventoryEliminatedFormulationSolver),
            static () => new global::ULSAlgorithms.Exact.Formulations.InventoryEliminatedFormulationSolver(),
            UlsSolverConfigurationCapabilities.OptimizationExecution,
            static options => new global::ULSAlgorithms.Exact.Formulations.InventoryEliminatedFormulationSolver(
                options.OptimizationExecution)),
        new(
            "general-ls-cutting-plane",
            "General (l,S) cutting-plane",
            UlsSolverCategory.CuttingPlane,
            "Cutting planes / convex hull",
            "O(T²) separation per root iteration + solver",
            "O(T) separator + model/cuts",
            "General classical ULS",
            "Barany, Van Roy & Wolsey (1984), Uncapacitated Lot-Sizing: The Convex Hull of Solutions",
            "10.1007/BFb0121006",
            "Exact general (l,S) separation + strengthened final MILP",
            "src/ULSAlgorithms/Exact/CuttingPlanes/GeneralLsCuttingPlaneSolver.cs",
            typeof(global::ULSAlgorithms.Exact.CuttingPlanes.GeneralLsCuttingPlaneSolver),
            static () => new global::ULSAlgorithms.Exact.CuttingPlanes.GeneralLsCuttingPlaneSolver(),
            UlsSolverConfigurationCapabilities.OptimizationExecution |
            UlsSolverConfigurationCapabilities.CuttingPlane,
            static options => new global::ULSAlgorithms.Exact.CuttingPlanes.GeneralLsCuttingPlaneSolver(
                options.OptimizationExecution,
                options.CuttingPlane)),
        new(
            "wagner-whitin-ls-cutting-plane",
            "Wagner–Whitin (l,S) cutting-plane",
            UlsSolverCategory.CuttingPlane,
            "Cutting planes / Wagner–Whitin",
            "O(T²) separation per root iteration + solver",
            "O(T) separator + model/cuts",
            "No speculative motive / Wagner–Whitin costs",
            "Pochet & Wolsey (1994), Polyhedra for Lot-Sizing with Wagner-Whitin Costs",
            "10.1007/BF01582225",
            "O(T²) prefix-S Wagner–Whitin separation + strengthened final MILP",
            "src/ULSAlgorithms/Exact/CuttingPlanes/WagnerWhitinLsCuttingPlaneSolver.cs",
            typeof(global::ULSAlgorithms.Exact.CuttingPlanes.WagnerWhitinLsCuttingPlaneSolver),
            static () => new global::ULSAlgorithms.Exact.CuttingPlanes.WagnerWhitinLsCuttingPlaneSolver(),
            UlsSolverConfigurationCapabilities.OptimizationExecution |
            UlsSolverConfigurationCapabilities.CuttingPlane,
            static options => new global::ULSAlgorithms.Exact.CuttingPlanes.WagnerWhitinLsCuttingPlaneSolver(
                options.OptimizationExecution,
                options.CuttingPlane)),
        new(
            "lot-for-lot",
            "Lot-for-Lot",
            UlsSolverCategory.Heuristic,
            "Baseline",
            "O(T)",
            "O(T)",
            "General ULS costs",
            "Classical MRP lot-for-lot rule",
            "",
            "One replenishment per positive-demand period",
            "src/ULSAlgorithms/Heuristics/LotForLotSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.LotForLotSolver),
            static () => new global::ULSAlgorithms.Heuristics.LotForLotSolver()),
        new(
            "silver-meal",
            "Silver–Meal",
            UlsSolverCategory.Heuristic,
            "Average-cost",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Silver & Meal (1973), A Heuristic for Selecting Lot Size Quantities for the Case of a Deterministic Time-Varying Demand Rate and Discrete Opportunities for Replenishment, Production and Inventory Management 14(2), 64-74",
            "",
            "Least cost per covered period",
            "src/ULSAlgorithms/Heuristics/SilverMealSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.SilverMealSolver),
            static () => new global::ULSAlgorithms.Heuristics.SilverMealSolver()),
        new(
            "least-unit-cost",
            "Least Unit Cost",
            UlsSolverCategory.Heuristic,
            "Average-cost",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Classical Least Unit Cost (LUC) lot-sizing rule",
            "",
            "Least relevant cost per unit",
            "src/ULSAlgorithms/Heuristics/LeastUnitCostSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.LeastUnitCostSolver),
            static () => new global::ULSAlgorithms.Heuristics.LeastUnitCostSolver()),
        new(
            "part-period-balancing",
            "Part-Period Balancing",
            UlsSolverCategory.Heuristic,
            "Part-period",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "DeMatteis (1968), An Economic Lot-Sizing Technique I: The Part-Period Algorithm, IBM Systems Journal 7(1), 30-38",
            "10.1147/sj.71.0030",
            "Closest balance to economic part period",
            "src/ULSAlgorithms/Heuristics/PartPeriodBalancingSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.PartPeriodBalancingSolver),
            static () => new global::ULSAlgorithms.Heuristics.PartPeriodBalancingSolver()),
        new(
            "groff",
            "Groff",
            UlsSolverCategory.Heuristic,
            "Marginal-cost",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Groff (1979), A Lot Sizing Rule for Time-Phased Component Demand, Production and Inventory Management 20(4), 66-74",
            "",
            "Marginal setup/holding criterion",
            "src/ULSAlgorithms/Heuristics/GroffSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.GroffSolver),
            static () => new global::ULSAlgorithms.Heuristics.GroffSolver()),
        new(
            "periodic-order-quantity",
            "Periodic Order Quantity",
            UlsSolverCategory.Heuristic,
            "Fixed-cycle",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Classical Periodic Order Quantity (POQ) rule",
            "",
            "EOQ-derived replenishment interval",
            "src/ULSAlgorithms/Heuristics/PeriodicOrderQuantitySolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.PeriodicOrderQuantitySolver),
            static () => new global::ULSAlgorithms.Heuristics.PeriodicOrderQuantitySolver()),
        new(
            "freeland-colley",
            "Freeland–Colley",
            UlsSolverCategory.Heuristic,
            "Marginal-cost",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Freeland & Colley (1982), A Simple Heuristic Method for Lot Sizing in a Time-Phased Reorder System, Production and Inventory Management 23(1), 15-21",
            "",
            "Local incremental carrying-cost criterion",
            "src/ULSAlgorithms/Heuristics/FreelandColleySolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.FreelandColleySolver),
            static () => new global::ULSAlgorithms.Heuristics.FreelandColleySolver()),
        new(
            "patterson-laforge-incremental-part-period",
            "Patterson–LaForge IPPA",
            UlsSolverCategory.Heuristic,
            "Part-period",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Patterson & LaForge (1985), The Incremental Part-Period Algorithm: An Alternative to EOQ, Journal of Purchasing and Materials Management 21(2), 28-33",
            "10.1111/j.1745-493X.1985.tb00132.x",
            "Incremental part-period stopping rule",
            "src/ULSAlgorithms/Heuristics/PattersonLaForgeIncrementalPartPeriodSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.PattersonLaForgeIncrementalPartPeriodSolver),
            static () => new global::ULSAlgorithms.Heuristics.PattersonLaForgeIncrementalPartPeriodSolver()),
        new(
            "wemmerlov-modified-ppb",
            "Wemmerlöv corrected PPB",
            UlsSolverCategory.Heuristic,
            "Part-period",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Wemmerlöv (1983), The Part-Period Balancing Algorithm and Its Look Ahead-Look Back Feature: A Theoretical and Experimental Analysis of a Single Stage Lot-Sizing Procedure, Journal of Operations Management 4(1), 23-39",
            "10.1016/0272-6963(83)90023-2",
            "Corrected PPB with ν = 0.5",
            "src/ULSAlgorithms/Heuristics/WemmerlovModifiedPartPeriodBalancingSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.WemmerlovModifiedPartPeriodBalancingSolver),
            static () => new global::ULSAlgorithms.Heuristics.WemmerlovModifiedPartPeriodBalancingSolver()),
        new(
            "wemmerlov-ppb-lalb",
            "Wemmerlöv PPB + LALB",
            UlsSolverCategory.Heuristic,
            "Look-ahead / look-back",
            "O(T)",
            "O(T)",
            "Stationary costs; strictly positive demand",
            "Wemmerlöv (1983), The Part-Period Balancing Algorithm and Its Look Ahead-Look Back Feature: A Theoretical and Experimental Analysis of a Single Stage Lot-Sizing Procedure, Journal of Operations Management 4(1), 23-39",
            "10.1016/0272-6963(83)90023-2",
            "PPB with local LALB adjustment",
            "src/ULSAlgorithms/Heuristics/WemmerlovPpbLookAheadLookBackSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.WemmerlovPpbLookAheadLookBackSolver),
            static () => new global::ULSAlgorithms.Heuristics.WemmerlovPpbLookAheadLookBackSolver()),
        new(
            "wemmerlov-modified-ppb-lalb",
            "Wemmerlöv corrected PPB + LALB",
            UlsSolverCategory.Heuristic,
            "Look-ahead / look-back",
            "O(T)",
            "O(T)",
            "Stationary costs; strictly positive demand",
            "Wemmerlöv (1983), The Part-Period Balancing Algorithm and Its Look Ahead-Look Back Feature: A Theoretical and Experimental Analysis of a Single Stage Lot-Sizing Procedure, Journal of Operations Management 4(1), 23-39",
            "10.1016/0272-6963(83)90023-2",
            "Corrected PPB + LALB",
            "src/ULSAlgorithms/Heuristics/WemmerlovModifiedPpbLookAheadLookBackSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.WemmerlovModifiedPpbLookAheadLookBackSolver),
            static () => new global::ULSAlgorithms.Heuristics.WemmerlovModifiedPpbLookAheadLookBackSolver()),
        new(
            "part-period-simplified",
            "Part-Period Simplified",
            UlsSolverCategory.Heuristic,
            "Part-period",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "DeMatteis (1968), An Economic Lot-Sizing Technique I: The Part-Period Algorithm, IBM Systems Journal 7(1), 30-38; Baciarello et al. (2013)",
            "10.5772/56004",
            "No-overshoot EPP / Part-Period Simplified rule",
            "src/ULSAlgorithms/Heuristics/PartPeriodSimplifiedSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.PartPeriodSimplifiedSolver),
            static () => new global::ULSAlgorithms.Heuristics.PartPeriodSimplifiedSolver()),
        new(
            "segerstedt-reformulated-silver-meal",
            "Segerstedt reformulated Silver-Meal",
            UlsSolverCategory.Heuristic,
            "Average-cost",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Segerstedt, Abdul-Jalbar & Samuelsson (2023), Reformulated Silver-Meal and Similar Lot Sizing Techniques, Axioms 12(7), 661",
            "10.3390/axioms12070661",
            "Reformulated Silver-Meal over non-zero demand events",
            "src/ULSAlgorithms/Heuristics/SegerstedtReformulatedSilverMealSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.SegerstedtReformulatedSilverMealSolver),
            static () => new global::ULSAlgorithms.Heuristics.SegerstedtReformulatedSilverMealSolver()),
        new(
            "chiu-modified-least-unit-cost",
            "Chiu modified Least Unit Cost",
            UlsSolverCategory.Heuristic,
            "Average-cost / post-processing",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Chiu (2004), A modification of the least unit cost lot-sizing heuristic, Journal of Statistics and Management Systems 7(1), 197-207",
            "10.1080/09720510.2004.10701115",
            "Classical LUC plus cost-beneficial final-lot merge",
            "src/ULSAlgorithms/Heuristics/ChiuModifiedLeastUnitCostSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.ChiuModifiedLeastUnitCostSolver),
            static () => new global::ULSAlgorithms.Heuristics.ChiuModifiedLeastUnitCostSolver()),
        new(
            "chiu-ting-modified-part-period-balancing",
            "Chiu-Ting modified Part-Period Balancing",
            UlsSolverCategory.Heuristic,
            "Part-period / post-processing",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Chiu, Ting & Chiu (2005), A Modified Version of the Part Period Lot-Sizing Heuristic, International Journal for Engineering Modelling 18(1-2), 59-64",
            "",
            "Nearest-EPP PPB plus cost-beneficial final-lot merge",
            "src/ULSAlgorithms/Heuristics/ChiuTingModifiedPartPeriodBalancingSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.ChiuTingModifiedPartPeriodBalancingSolver),
            static () => new global::ULSAlgorithms.Heuristics.ChiuTingModifiedPartPeriodBalancingSolver()),
        new(
            "ho-chang-solis-net-least-period-cost",
            "Ho-Chang-Solis net Least Period Cost",
            UlsSolverCategory.Heuristic,
            "Average-cost / net period",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Ho, Chang & Solis (2006), Two modifications of the least cost per period heuristic for dynamic lot-sizing, Journal of the Operational Research Society 57(8), 1005-1013",
            "10.1057/palgrave.jors.2602076",
            "Incremental O(T) evaluation of the published nAPC stopping rule; zero-demand periods are excluded from the average denominator",
            "src/ULSAlgorithms/Heuristics/HoChangSolisNetLeastPeriodCostSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.HoChangSolisNetLeastPeriodCostSolver),
            static () => new global::ULSAlgorithms.Heuristics.HoChangSolisNetLeastPeriodCostSolver()),
        new(
            "ho-chang-solis-improved-net-least-period-cost",
            "Ho-Chang-Solis improved nLPC(i)",
            UlsSolverCategory.Heuristic,
            "Average-cost / net period",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Ho, Chang & Solis (2006), Two modifications of the least cost per period heuristic for dynamic lot-sizing, Journal of the Operational Research Society 57(8), 1005-1013",
            "10.1057/palgrave.jors.2602076",
            "Incremental nAPC rule with the published improved tie-breaking stop condition",
            "src/ULSAlgorithms/Heuristics/HoChangSolisImprovedNetLeastPeriodCostSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.HoChangSolisImprovedNetLeastPeriodCostSolver),
            static () => new global::ULSAlgorithms.Heuristics.HoChangSolisImprovedNetLeastPeriodCostSolver()),
        new(
            "mclaren-order-moment",
            "McLaren Order Moment",
            UlsSolverCategory.Heuristic,
            "Part-period / EOQ hybrid",
            "O(T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "McLaren (1977), Order Moment lot-sizing rule; Baciarello et al. (2013), Lot Sizing Heuristics Performance",
            "10.5772/56004",
            "EOQ-derived Order Moment Target with part-period accumulation and a final marginal holding/setup test",
            "src/ULSAlgorithms/Heuristics/McLarenOrderMomentSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.McLarenOrderMomentSolver),
            static () => new global::ULSAlgorithms.Heuristics.McLarenOrderMomentSolver()),
        new(
            "karni-maximum-part-period-gain",
            "Karni Maximum Part-Period Gain",
            UlsSolverCategory.Heuristic,
            "Global part-period merge",
            "O(T log T)",
            "O(T)",
            "Stationary setup, production and relevant holding costs",
            "Karni (1981), Maximum Part-Period Gain lot-sizing rule; Baciarello et al. (2013), Lot Sizing Heuristics Performance",
            "10.5772/56004",
            "Priority-queue acceleration of the published non-forward global smallest-part-period merge rule",
            "src/ULSAlgorithms/Heuristics/KarniMaximumPartPeriodGainSolver.cs",
            typeof(global::ULSAlgorithms.Heuristics.KarniMaximumPartPeriodGainSolver),
            static () => new global::ULSAlgorithms.Heuristics.KarniMaximumPartPeriodGainSolver()),
    ];

    private static readonly Dictionary<string, UlsSolverDescriptor> ById =
        CreateIndex(Descriptors);

    private static readonly IReadOnlyList<UlsSolverDescriptor> AllView =
        Array.AsReadOnly(Descriptors);

    private static readonly IReadOnlyList<UlsSolverDescriptor> ExactView =
        Array.AsReadOnly(
            Descriptors
                .Where(descriptor => descriptor.Kind == UlsSolverKind.Exact)
                .ToArray());

    private static readonly IReadOnlyList<UlsSolverDescriptor> DirectExactView =
        Array.AsReadOnly(
            Descriptors
                .Where(descriptor =>
                    descriptor.Category == UlsSolverCategory.DirectExact)
                .ToArray());

    private static readonly IReadOnlyList<UlsSolverDescriptor> FormulationsView =
        Array.AsReadOnly(
            Descriptors
                .Where(descriptor =>
                    descriptor.Category ==
                    UlsSolverCategory.OptimizationFormulation)
                .ToArray());

    private static readonly IReadOnlyList<UlsSolverDescriptor> CuttingPlanesView =
        Array.AsReadOnly(
            Descriptors
                .Where(descriptor =>
                    descriptor.Category == UlsSolverCategory.CuttingPlane)
                .ToArray());

    private static readonly IReadOnlyList<UlsSolverDescriptor> HeuristicsView =
        Array.AsReadOnly(
            Descriptors
                .Where(descriptor =>
                    descriptor.Category == UlsSolverCategory.Heuristic)
                .ToArray());

    private static readonly IReadOnlyList<UlsSolverDescriptor> ConfigurableView =
        Array.AsReadOnly(
            Descriptors
                .Where(descriptor => descriptor.SupportsConfiguration)
                .ToArray());

    /// <summary>Gets all public strategies in stable catalog order.</summary>
    public static IReadOnlyList<UlsSolverDescriptor> All => AllView;

    /// <summary>
    /// Gets all exact strategies, including direct algorithms, formulations and
    /// cutting-plane methods.
    /// </summary>
    public static IReadOnlyList<UlsSolverDescriptor> Exact => ExactView;

    /// <summary>Gets direct exact algorithms that need no external optimizer.</summary>
    public static IReadOnlyList<UlsSolverDescriptor> DirectExact => DirectExactView;

    /// <summary>Gets exact solver-backed mathematical formulations.</summary>
    public static IReadOnlyList<UlsSolverDescriptor> Formulations => FormulationsView;

    /// <summary>Gets exact solver-backed cutting-plane strategies.</summary>
    public static IReadOnlyList<UlsSolverDescriptor> CuttingPlanes => CuttingPlanesView;

    /// <summary>Gets all heuristic strategies.</summary>
    public static IReadOnlyList<UlsSolverDescriptor> Heuristics => HeuristicsView;

    /// <summary>
    /// Gets strategies exposing at least one constructor-level configurable
    /// factory setting.
    /// </summary>
    public static IReadOnlyList<UlsSolverDescriptor> Configurable =>
        ConfigurableView;

    /// <summary>
    /// Gets the recommended automatic exact entry point.
    /// </summary>
    public static UlsSolverDescriptor RecommendedExact =>
        Get("adaptive-exact");

    /// <summary>
    /// Gets one descriptor by stable identifier.
    /// </summary>
    /// <param name="id">Stable lower-kebab-case strategy identifier.</param>
    /// <returns>The matching descriptor.</returns>
    /// <exception cref="KeyNotFoundException">No strategy uses the identifier.</exception>
    public static UlsSolverDescriptor Get(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (ById.TryGetValue(id, out var descriptor))
        {
            return descriptor;
        }

        throw new KeyNotFoundException(
            $"Unknown ULS solver identifier '{id}'.");
    }

    /// <summary>
    /// Attempts to resolve one descriptor by stable identifier.
    /// </summary>
    /// <param name="id">Stable identifier.</param>
    /// <param name="descriptor">Resolved descriptor, or null when not found.</param>
    /// <returns>True when a matching descriptor exists.</returns>
    public static bool TryGet(
        string? id,
        [NotNullWhen(true)] out UlsSolverDescriptor? descriptor)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            descriptor = null;
            return false;
        }

        return ById.TryGetValue(id, out descriptor);
    }

    /// <summary>
    /// Gets the descriptor associated with a concrete solver type.
    /// </summary>
    /// <param name="implementationType">Public solver implementation type.</param>
    /// <returns>The matching descriptor.</returns>
    public static UlsSolverDescriptor GetByType(Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(implementationType);

        foreach (var descriptor in Descriptors)
        {
            if (descriptor.ImplementationType == implementationType)
            {
                return descriptor;
            }
        }

        throw new KeyNotFoundException(
            $"Type '{implementationType.FullName}' is not registered in the ULS solver catalog.");
    }

    private static Dictionary<string, UlsSolverDescriptor> CreateIndex(
        IReadOnlyList<UlsSolverDescriptor> descriptors)
    {
        var index = new Dictionary<string, UlsSolverDescriptor>(
            descriptors.Count,
            StringComparer.OrdinalIgnoreCase);

        var types = new HashSet<Type>();

        foreach (var descriptor in descriptors)
        {
            if (!index.TryAdd(descriptor.Id, descriptor))
            {
                throw new InvalidOperationException(
                    $"Duplicate solver catalog identifier '{descriptor.Id}'.");
            }

            if (!types.Add(descriptor.ImplementationType))
            {
                throw new InvalidOperationException(
                    $"Duplicate solver catalog type '{descriptor.ImplementationType.FullName}'.");
            }
        }

        return index;
    }
}

