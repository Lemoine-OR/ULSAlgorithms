<p align="center">
  <img src="docs/assets/ulsalgorithms-logo.svg" alt="ULSAlgorithms" width="560">
</p>

<p align="center">
  <strong>Fast, scientific and reusable C# / .NET algorithms for deterministic Uncapacitated Lot-Sizing.</strong>
</p>

<p align="center">
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/actions/workflows/build.yml"><img alt="Build and Test" src="https://github.com/Lemoine-OR/ULSAlgorithms/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/actions/workflows/documentation.yml"><img alt="Documentation" src="https://github.com/Lemoine-OR/ULSAlgorithms/actions/workflows/documentation.yml/badge.svg"></a>
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/releases/latest"><img alt="Latest release" src="https://img.shields.io/github/v/release/Lemoine-OR/ULSAlgorithms?display_name=tag&sort=semver"></a>
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512BD4">
  <img alt="MIT" src="https://img.shields.io/badge/license-MIT-0B7285">
  <img alt="Stable API" src="https://img.shields.io/badge/API-stable%201.x-15803D">
</p>

<p align="center">
  <a href="https://lemoine-or.github.io/ULSAlgorithms/"><strong>Project & Documentation</strong></a>
  ·
  <a href="https://lemoine-or.github.io/ULSAlgorithms/#algorithms"><strong>Algorithms</strong></a>
  ·
  <a href="https://lemoine-or.github.io/ULSAlgorithms/api/getting_started.html"><strong>Getting started</strong></a>
  ·
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/releases/latest"><strong>Latest release</strong></a>
  ·
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/tree/main/src/ULSAlgorithms"><strong>Source</strong></a>
</p>

---

ULSAlgorithms is a high-performance library for the deterministic, finite-horizon,
uncapacitated lot-sizing problem (ULS). All public methods share a common
`IUlsSolver` strategy contract and are available through stable catalog IDs.

<table>
<tr>
<td width="25%"><strong>17 direct exact</strong><br><sub>Dynamic programming, geometric, network, branch-and-bound and specialized methods.</sub></td>
<td width="25%"><strong>4 formulations</strong><br><sub>Portable solver-backed mathematical formulations.</sub></td>
<td width="25%"><strong>2 cutting planes</strong><br><sub>Exact <code>(l,S)</code> cut-and-solve methods.</sub></td>
<td width="25%"><strong>19 heuristics</strong><br><sub>Classical and literature-backed fast construction rules.</sub></td>
</tr>
</table>

<p align="center"><strong>42 public strategies · one API · stable 1.x contract</strong></p>

## Start in 30 seconds

For most client code, start with the stable factory ID `adaptive-exact`:

```csharp
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Catalog;
using ULSAlgorithms.Models;

var problem = new UlsProblem(
    demands:             [20.0, 30.0, 25.0, 40.0],
    setupCosts:          [200.0, 200.0, 200.0, 200.0],
    unitProductionCosts: [0.0, 0.0, 0.0, 0.0],
    holdingCosts:        [4.0, 4.0, 4.0, 0.0]);

IUlsSolver solver = UlsSolverFactory.Create("adaptive-exact");
var result = solver.Solve(problem);

Console.WriteLine(result.Status);
Console.WriteLine(result.ObjectiveValue);
```

> **New to the library?** Open the [Getting Started guide](https://lemoine-or.github.io/ULSAlgorithms/api/getting_started.html).  
> **Looking for a method?** Browse the panels below or the [project documentation](https://lemoine-or.github.io/ULSAlgorithms/).  
> **Need reproducibility?** Use stable solver IDs and the versioned JSON configuration.

## Why ULSAlgorithms?

<table>
<tr>
<td width="25%"><strong>Fast</strong><br><sub>Computationally efficient literature-backed implementations and data structures.</sub></td>
<td width="25%"><strong>Scientific</strong><br><sub>Explicit provenance, applicability and DOI metadata where a DOI is available.</sub></td>
<td width="25%"><strong>Uniform</strong><br><sub>One Strategy contract, stable IDs, a canonical catalog and a common factory.</sub></td>
<td width="25%"><strong>Validated</strong><br><sub>Windows/Linux CI, independent exact cross-checks, package validation and real CBC qualification.</sub></td>
</tr>
</table>

## Choose a family

<table>
<tr>
<td width="25%"><strong>Exact algorithms</strong><br><sub>Self-contained exact methods requiring no external optimizer.</sub></td>
<td width="25%"><strong>Mathematical optimization</strong><br><sub>Solver-backed formulations with CPLEX → Gurobi → Xpress → CBC discovery.</sub></td>
<td width="25%"><strong>Cutting planes</strong><br><sub>Exact <code>(l,S)</code> methods with cut traceability and final MILP validation.</sub></td>
<td width="25%"><strong>Heuristics</strong><br><sub>Fast feasible construction rules without an optimality claim.</sub></td>
</tr>
</table>

## All algorithms

Click a method name to open its dedicated documentation page. Every panel also
shows the stable ID used by `UlsSolverFactory.Create(...)`.

### Recommended exact entry point

<table>
<tr><td><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/adaptiveexactulssolver.html"><strong>Adaptive exact selection</strong></a> · <strong>Recommended</strong><br><sub>Exact · automatic dispatch · O(T) / O(T log T)</sub><br><code>adaptive-exact</code><br><sub><code>AdaptiveExactUlsSolver</code></sub></td></tr>
</table>

`adaptive-exact` uses the linear Wagner–Whitin specialization when the
no-speculative-motive condition applies and otherwise dispatches to the
configured general exact fallback.

### Direct exact algorithms

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wagnerwhitinclassicalsolver.html"><strong>Wagner–Whitin classical</strong></a><br><sub>Exact · O(T²)</sub><br><code>wagner-whitin-classical</code><br><sub><code>WagnerWhitinClassicalSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wagnerwhitinevanssolver.html"><strong>Wagner–Whitin / Evans</strong></a><br><sub>Exact · O(T²), O(T) space</sub><br><code>wagner-whitin-evans</code><br><sub><code>WagnerWhitinEvansSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wagnerwhitinsolver.html"><strong>Wagner–Whitin linear</strong></a><br><sub>Exact · O(T)</sub><br><code>wagner-whitin-linear</code><br><sub><code>WagnerWhitinSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wagelmansgeneralsolver.html"><strong>Wagelmans general</strong></a><br><sub>Exact · O(T log T)</sub><br><code>wagelmans-general</code><br><sub><code>WagelmansGeneralSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/federgruentzursolver.html"><strong>Federgruen–Tzur general</strong></a><br><sub>Exact · O(T log T)</sub><br><code>federgruen-tzur-general</code><br><sub><code>FedergruenTzurSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/federgruentzurnospeculativemotivesolver.html"><strong>Federgruen–Tzur linear (NSM)</strong></a><br><sub>Exact · O(T)</sub><br><code>federgruen-tzur-nsm</code><br><sub><code>FedergruenTzurNoSpeculativeMotiveSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/federgruentzurnondecreasingsetupsolver.html"><strong>Federgruen–Tzur linear (setup)</strong></a><br><sub>Exact · O(T)</sub><br><code>federgruen-tzur-nondecreasing-setup</code><br><sub><code>FedergruenTzurNondecreasingSetupSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/aggarwalparksolver.html"><strong>Aggarwal–Park</strong></a><br><sub>Exact · O(T log T)</sub><br><code>aggarwal-park</code><br><sub><code>AggarwalParkSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/bahltajplanninghorizonsolver.html"><strong>Bahl–Taj planning horizon</strong></a><br><sub>Exact · O(T²) worst case</sub><br><code>bahl-taj-planning-horizon</code><br><sub><code>BahlTajPlanningHorizonSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/headyzhueconomicpartperiodsolver.html"><strong>Heady–Zhu</strong></a><br><sub>Exact · O(T²) worst case</sub><br><code>heady-zhu</code><br><sub><code>HeadyZhuEconomicPartPeriodSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/chowdhurybakiazabsolver.html"><strong>Chowdhury–Baki–Azab</strong></a><br><sub>Exact · O(T)</sub><br><code>chowdhury-baki-azab</code><br><sub><code>ChowdhuryBakiAzabSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/sadjadiaryanezhadsadeghisolver.html"><strong>Sadjadi–Aryanezhad–Sadeghi</strong></a><br><sub>Exact · O(T²) worst case</sub><br><code>sadjadi-aryanezhad-sadeghi</code><br><sub><code>SadjadiAryanezhadSadeghiSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/lyuleeparallelsolver.html"><strong>Lyu–Lee parallel</strong></a><br><sub>Exact · O(T²) work</sub><br><code>lyu-lee-parallel</code><br><sub><code>LyuLeeParallelSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/saydammcknewfastwagnerwhitinsolver.html"><strong>Saydam–McKnew</strong></a><br><sub>Exact · O(T²)</sub><br><code>saydam-mcknew</code><br><sub><code>SaydamMcKnewFastWagnerWhitinSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/jacobskhumawalabranchandboundsolver.html"><strong>Jacobs–Khumawala</strong></a><br><sub>Exact · O(T²)</sub><br><code>jacobs-khumawala</code><br><sub><code>JacobsKhumawalaBranchAndBoundSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/zangwillnetworksolver.html"><strong>Zangwill network</strong></a><br><sub>Exact · O(T²)</sub><br><code>zangwill-network</code><br><sub><code>ZangwillNetworkSolver</code></sub></td></tr>
</table>

### Mathematical optimization

These four exact strategies require an external optimization engine.

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/aggregateinventoryformulationsolver.html"><strong>Aggregate inventory formulation</strong></a><br><sub>Exact · Optimization · Solver-dependent</sub><br><code>aggregate-inventory-formulation</code><br><sub><code>AggregateInventoryFormulationSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/facilitylocationformulationsolver.html"><strong>Facility-location formulation</strong></a><br><sub>Exact · Optimization · Solver-dependent</sub><br><code>facility-location-formulation</code><br><sub><code>FacilityLocationFormulationSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/shortestpathformulationsolver.html"><strong>Shortest-path formulation</strong></a><br><sub>Exact · Optimization · Solver-dependent</sub><br><code>shortest-path-formulation</code><br><sub><code>ShortestPathFormulationSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/inventoryeliminatedformulationsolver.html"><strong>Inventory-eliminated formulation</strong></a><br><sub>Exact · Optimization · Solver-dependent</sub><br><code>inventory-eliminated-formulation</code><br><sub><code>InventoryEliminatedFormulationSolver</code></sub></td></tr>
</table>

### Cutting planes

These two exact strategies require an external optimization engine.

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/generallscuttingplanesolver.html"><strong>General (l,S) cutting-plane</strong></a><br><sub>Exact · Cutting plane · O(T²) separation + solver</sub><br><code>general-ls-cutting-plane</code><br><sub><code>GeneralLsCuttingPlaneSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wagnerwhitinlscuttingplanesolver.html"><strong>Wagner–Whitin (l,S) cutting-plane</strong></a><br><sub>Exact · Cutting plane · O(T²) separation + solver</sub><br><code>wagner-whitin-ls-cutting-plane</code><br><sub><code>WagnerWhitinLsCuttingPlaneSolver</code></sub></td></tr>
</table>

### Heuristics

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/lotforlotsolver.html"><strong>Lot-for-Lot</strong></a><br><sub>Heuristic · Baseline · O(T)</sub><br><code>lot-for-lot</code><br><sub><code>LotForLotSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/periodicorderquantitysolver.html"><strong>Periodic Order Quantity</strong></a><br><sub>Heuristic · Fixed-cycle · O(T)</sub><br><code>periodic-order-quantity</code><br><sub><code>PeriodicOrderQuantitySolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/silvermealsolver.html"><strong>Silver–Meal</strong></a><br><sub>Heuristic · Average-cost · O(T)</sub><br><code>silver-meal</code><br><sub><code>SilverMealSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/segerstedtreformulatedsilvermealsolver.html"><strong>Segerstedt reformulated Silver-Meal</strong></a><br><sub>Heuristic · Average-cost · O(T)</sub><br><code>segerstedt-reformulated-silver-meal</code><br><sub><code>SegerstedtReformulatedSilverMealSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/leastunitcostsolver.html"><strong>Least Unit Cost</strong></a><br><sub>Heuristic · Average-cost · O(T)</sub><br><code>least-unit-cost</code><br><sub><code>LeastUnitCostSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/chiumodifiedleastunitcostsolver.html"><strong>Chiu modified Least Unit Cost</strong></a><br><sub>Heuristic · Average-cost / post-processing · O(T)</sub><br><code>chiu-modified-least-unit-cost</code><br><sub><code>ChiuModifiedLeastUnitCostSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/hochangsolisnetleastperiodcostsolver.html"><strong>Ho–Chang–Solis nLPC</strong></a><br><sub>Heuristic · Average-cost / net period · O(T)</sub><br><code>ho-chang-solis-net-least-period-cost</code><br><sub><code>HoChangSolisNetLeastPeriodCostSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/hochangsolisimprovednetleastperiodcostsolver.html"><strong>Ho–Chang–Solis nLPC(i)</strong></a><br><sub>Heuristic · Average-cost / net period · O(T)</sub><br><code>ho-chang-solis-improved-net-least-period-cost</code><br><sub><code>HoChangSolisImprovedNetLeastPeriodCostSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/partperiodsimplifiedsolver.html"><strong>Part-Period Simplified</strong></a><br><sub>Heuristic · Part-period · O(T)</sub><br><code>part-period-simplified</code><br><sub><code>PartPeriodSimplifiedSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/partperiodbalancingsolver.html"><strong>Part-Period Balancing</strong></a><br><sub>Heuristic · Part-period · O(T)</sub><br><code>part-period-balancing</code><br><sub><code>PartPeriodBalancingSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/chiutingmodifiedpartperiodbalancingsolver.html"><strong>Chiu–Ting modified PPB</strong></a><br><sub>Heuristic · Part-period / post-processing · O(T)</sub><br><code>chiu-ting-modified-part-period-balancing</code><br><sub><code>ChiuTingModifiedPartPeriodBalancingSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/pattersonlaforgeincrementalpartperiodsolver.html"><strong>Patterson–LaForge IPPA</strong></a><br><sub>Heuristic · Part-period · O(T)</sub><br><code>patterson-laforge-incremental-part-period</code><br><sub><code>PattersonLaForgeIncrementalPartPeriodSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wemmerlovmodifiedpartperiodbalancingsolver.html"><strong>Wemmerlöv corrected PPB</strong></a><br><sub>Heuristic · Part-period · O(T)</sub><br><code>wemmerlov-modified-ppb</code><br><sub><code>WemmerlovModifiedPartPeriodBalancingSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wemmerlovppblookaheadlookbacksolver.html"><strong>Wemmerlöv PPB + LALB</strong></a><br><sub>Heuristic · Look-ahead / look-back · O(T)</sub><br><code>wemmerlov-ppb-lalb</code><br><sub><code>WemmerlovPpbLookAheadLookBackSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wemmerlovmodifiedppblookaheadlookbacksolver.html"><strong>Wemmerlöv corrected PPB + LALB</strong></a><br><sub>Heuristic · Look-ahead / look-back · O(T)</sub><br><code>wemmerlov-modified-ppb-lalb</code><br><sub><code>WemmerlovModifiedPpbLookAheadLookBackSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/mclarenordermomentsolver.html"><strong>McLaren Order Moment</strong></a><br><sub>Heuristic · Part-period / EOQ hybrid · O(T)</sub><br><code>mclaren-order-moment</code><br><sub><code>McLarenOrderMomentSolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/groffsolver.html"><strong>Groff</strong></a><br><sub>Heuristic · Marginal-cost · O(T)</sub><br><code>groff</code><br><sub><code>GroffSolver</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/freelandcolleysolver.html"><strong>Freeland–Colley</strong></a><br><sub>Heuristic · Marginal-cost · O(T)</sub><br><code>freeland-colley</code><br><sub><code>FreelandColleySolver</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/karnimaximumpartperiodgainsolver.html"><strong>Karni Maximum Part-Period Gain</strong></a><br><sub>Heuristic · Global part-period merge · O(T log T)</sub><br><code>karni-maximum-part-period-gain</code><br><sub><code>KarniMaximumPartPeriodGainSolver</code></sub></td><td width="50%">&nbsp;</td></tr>
</table>

## Browse and create strategies

The runtime catalog is the canonical public inventory:

```csharp
foreach (var strategy in UlsSolverCatalog.All)
{
    Console.WriteLine(
        $"{strategy.Id} | {strategy.Name} | {strategy.TimeComplexity}");
}

IUlsSolver wagelmans = UlsSolverFactory.Create("wagelmans-general");
IUlsSolver silverMeal = UlsSolverFactory.Create("silver-meal");
```

Stable IDs are part of the 1.x compatibility contract.

## Configure a strategy

```csharp
using ULSAlgorithms.Selection;

IUlsSolver solver =
    UlsSolverFactory.Create(
        "adaptive-exact",
        new UlsSolverCreationOptions
        {
            AdaptiveGeneralFallback =
                UlsGeneralExactFallback.FedergruenTzurGeneral
        });
```

The same mechanism exposes Lyu–Lee parallel settings, external optimization
execution options and cutting-plane engineering options. Unsupported options
are rejected rather than silently ignored.

## Reproducible JSON configuration

```csharp
var configuration =
    new UlsSolverConfiguration
    {
        SolverId = "adaptive-exact",
        Options = new UlsSolverCreationOptions
        {
            AdaptiveGeneralFallback =
                UlsGeneralExactFallback.WagelmansGeneral
        }
    };

configuration.SaveJson("solver-config.json");

var loaded = UlsSolverConfiguration.LoadJson("solver-config.json");
IUlsSolver solver = UlsSolverFactory.Create(loaded);
```

Typical JSON:

```json
{
  "schemaVersion": 1,
  "solverId": "adaptive-exact",
  "options": {
    "adaptiveGeneralFallback": "wagelmansGeneral"
  }
}
```

Configuration schema version 1 is part of the stable 1.x compatibility contract.

## External optimization engines

Direct exact algorithms and heuristics require no external optimizer.
Solver-backed formulations and cutting-plane methods use the portable
optimization layer and support:

```text
CPLEX -> Gurobi -> Xpress -> COIN-OR CBC
```

An explicit engine can be requested through `LinearModelSolveOptions`.

## Distribution

Each validated GitHub release contains:

- binary ZIP and SHA-256 sidecar;
- documentation ZIP and SHA-256 sidecar;
- NuGet package (`.nupkg`) and SHA-256 sidecar;
- NuGet portable-symbol package (`.snupkg`) and SHA-256 sidecar;
- build metadata;
- binary manifest;
- release manifest and its SHA-256 sidecar.

Use the [latest release](https://github.com/Lemoine-OR/ULSAlgorithms/releases/latest) for validated binaries and
packages.

## Validation and performance

The qualification pipeline includes:

- deterministic literature-style tests;
- an independent quadratic Wagner–Whitin oracle;
- randomized exact cross-validation;
- feasibility and objective reconstruction;
- edge-case and cancellation tests;
- BenchmarkDotNet campaigns;
- runtime/documentation catalog synchronization;
- the public-API compatibility baseline;
- official .NET package validation;
- isolated real NuGet consumer restore/build/run validation;
- repository-wide Release builds and the complete unit-test suite on Windows and Linux;
- Linux portability smoke;
- real COIN-OR CBC end-to-end qualification for all six solver-backed strategies;
- a Cobertura-compatible coverage artifact without an arbitrary pass threshold;
- reproducible release manifests and SHA-256 checksums.

Benchmark results are evidence for the tested hardware, runtime and workload;
they are not universal performance theorems.

## Documentation

The [project site and generated documentation](https://lemoine-or.github.io/ULSAlgorithms/) provide:

- one panel and page per public strategy;
- descriptions and applicability conditions;
- complexity and implementation information;
- scientific references and DOI links;
- mathematical formulations where relevant;
- usage examples and generated API reference.

## Project links

| Resource | URL |
|---|---|
| **Project & documentation** | https://lemoine-or.github.io/ULSAlgorithms/ |
| **Source repository** | https://github.com/Lemoine-OR/ULSAlgorithms |
| **Source code** | https://github.com/Lemoine-OR/ULSAlgorithms/tree/main/src/ULSAlgorithms |
| **Latest stable release** | https://github.com/Lemoine-OR/ULSAlgorithms/releases/latest |
| **Changelog** | https://github.com/Lemoine-OR/ULSAlgorithms/blob/main/CHANGELOG.md |
| **API stability policy** | https://github.com/Lemoine-OR/ULSAlgorithms/blob/main/API-STABILITY.md |

## Citation

Academic users can cite the software with [`CITATION.cff`](CITATION.cff).
The citation metadata is also embedded in the NuGet package.

## API stability

Version 1.0.0 established the stable 1.x compatibility contract. It covers the
public .NET API baseline, `IUlsSolver`, existing stable strategy IDs and
`UlsSolverConfiguration` schema version 1.

See [`API-STABILITY.md`](API-STABILITY.md).

## License

ULSAlgorithms is released under the [MIT License](LICENSE).

## Author

**David Lemoine — Lemoine-OR**

---

<p align="center">
  <strong>Lemoine-OR Algorithms</strong><br>
  Clean. Scientific. Open.
</p>
