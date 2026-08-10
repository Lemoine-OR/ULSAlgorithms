<p align="center">
  <img src="docs/assets/ulsalgorithms-logo.svg" alt="ULSAlgorithms" width="560">
</p>

<p align="center">
  <strong>Fast, scientific and reusable C# algorithms for deterministic Uncapacitated Lot-Sizing.</strong>
</p>

<p align="center">
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/actions/workflows/build.yml"><img alt="Build and Test" src="https://github.com/Lemoine-OR/ULSAlgorithms/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/actions/workflows/documentation.yml"><img alt="Documentation" src="https://github.com/Lemoine-OR/ULSAlgorithms/actions/workflows/documentation.yml/badge.svg"></a>
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/releases/latest"><img alt="Latest release" src="https://img.shields.io/github/v/release/Lemoine-OR/ULSAlgorithms?display_name=tag&sort=semver"></a>
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512BD4">
</p>

<p align="center">
  <a href="https://lemoine-or.github.io/ULSAlgorithms/"><strong>Documentation</strong></a>
  ·
  <a href="https://lemoine-or.github.io/ULSAlgorithms/#algorithms"><strong>Algorithms</strong></a>
  ·
  <a href="https://lemoine-or.github.io/ULSAlgorithms/api/getting_started.html"><strong>Getting started</strong></a>
  ·
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/releases/latest"><strong>Latest release</strong></a>
</p>

---

## Start in 30 seconds

All public methods use the same core strategy contract: create a `UlsProblem`, choose an `IUlsSolver`, call `Solve`.

```csharp
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;

var problem = new UlsProblem(
    demands:             [20.0, 30.0, 25.0, 40.0],
    setupCosts:          [200.0, 200.0, 200.0, 200.0],
    unitProductionCosts: [0.0, 0.0, 0.0, 0.0],
    holdingCosts:        [4.0, 4.0, 4.0, 0.0]);

IUlsSolver solver = new WagnerWhitinSolver();
var result = solver.Solve(problem);

Console.WriteLine(result.ObjectiveValue);
```

> **New to the library?** Open the [Getting Started guide](https://lemoine-or.github.io/ULSAlgorithms/api/getting_started.html).  
> **Looking for a method?** Every algorithm below opens a dedicated, uniform documentation page.

## Choose a family

<table>
<tr>
<td width="25%"><strong>Exact algorithms</strong><br><sub>Direct dynamic programming, network and combinatorial methods.</sub></td>
<td width="25%"><strong>Mathematical optimization</strong><br><sub>Solver-backed formulations with automatic CPLEX → Gurobi → Xpress → CBC selection.</sub></td>
<td width="25%"><strong>Cutting planes</strong><br><sub>Exact (l,S) cut-and-solve methods with full cut traceability.</sub></td>
<td width="25%"><strong>Heuristics</strong><br><sub>Fast construction rules returning feasible plans without an optimality claim.</sub></td>
</tr>
</table>

## All algorithms

Click any panel to open its dedicated page: description, technical specifications, how it works, minimal usage and API/source links.

### Exact algorithms — dynamic programming & specialized methods

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wagnerwhitinclassicalsolver.html"><strong>Wagner–Whitin classical</strong></a><br><sub>Exact</sub><br><code>WagnerWhitinClassicalSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wagnerwhitinevanssolver.html"><strong>Wagner–Whitin / Evans</strong></a><br><sub>Exact</sub><br><code>WagnerWhitinEvansSolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wagnerwhitinsolver.html"><strong>Wagner–Whitin linear</strong></a><br><sub>Exact</sub><br><code>WagnerWhitinSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wagelmansgeneralsolver.html"><strong>Wagelmans general</strong></a><br><sub>Exact</sub><br><code>WagelmansGeneralSolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/federgruentzursolver.html"><strong>Federgruen–Tzur general</strong></a><br><sub>Exact</sub><br><code>FedergruenTzurSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/federgruentzurnospeculativemotivesolver.html"><strong>Federgruen–Tzur linear (NSM)</strong></a><br><sub>Exact</sub><br><code>FedergruenTzurNoSpeculativeMotiveSolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/federgruentzurnondecreasingsetupsolver.html"><strong>Federgruen–Tzur linear (setup)</strong></a><br><sub>Exact</sub><br><code>FedergruenTzurNondecreasingSetupSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/aggarwalparksolver.html"><strong>Aggarwal–Park</strong></a><br><sub>Exact</sub><br><code>AggarwalParkSolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/bahltajplanninghorizonsolver.html"><strong>Bahl–Taj planning horizon</strong></a><br><sub>Exact</sub><br><code>BahlTajPlanningHorizonSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/headyzhueconomicpartperiodsolver.html"><strong>Heady–Zhu</strong></a><br><sub>Exact</sub><br><code>HeadyZhuEconomicPartPeriodSolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/chowdhurybakiazabsolver.html"><strong>Chowdhury–Baki–Azab</strong></a><br><sub>Exact</sub><br><code>ChowdhuryBakiAzabSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/sadjadiaryanezhadsadeghisolver.html"><strong>Sadjadi–Aryanezhad–Sadeghi</strong></a><br><sub>Exact</sub><br><code>SadjadiAryanezhadSadeghiSolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/lyuleeparallelsolver.html"><strong>Lyu–Lee parallel</strong></a><br><sub>Exact</sub><br><code>LyuLeeParallelSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/saydammcknewfastwagnerwhitinsolver.html"><strong>Saydam–McKnew</strong></a><br><sub>Exact</sub><br><code>SaydamMcKnewFastWagnerWhitinSolver</code></td></tr>
</table>

### Exact algorithms — network & combinatorial

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/jacobskhumawalabranchandboundsolver.html"><strong>Jacobs–Khumawala</strong></a><br><sub>Exact</sub><br><code>JacobsKhumawalaBranchAndBoundSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/zangwillnetworksolver.html"><strong>Zangwill network</strong></a><br><sub>Exact</sub><br><code>ZangwillNetworkSolver</code></td></tr>
</table>

### Mathematical optimization

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/aggregateinventoryformulationsolver.html"><strong>Aggregate inventory formulation</strong></a><br><sub>Optimization</sub><br><code>AggregateInventoryFormulationSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/facilitylocationformulationsolver.html"><strong>Facility-location formulation</strong></a><br><sub>Optimization</sub><br><code>FacilityLocationFormulationSolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/shortestpathformulationsolver.html"><strong>Shortest-path formulation</strong></a><br><sub>Optimization</sub><br><code>ShortestPathFormulationSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/inventoryeliminatedformulationsolver.html"><strong>Inventory-eliminated formulation</strong></a><br><sub>Optimization</sub><br><code>InventoryEliminatedFormulationSolver</code></td></tr>
</table>

### Cutting planes

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/generallscuttingplanesolver.html"><strong>General (l,S) cutting-plane</strong></a><br><sub>Cutting plane</sub><br><code>GeneralLsCuttingPlaneSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wagnerwhitinlscuttingplanesolver.html"><strong>Wagner–Whitin (l,S) cutting-plane</strong></a><br><sub>Cutting plane</sub><br><code>WagnerWhitinLsCuttingPlaneSolver</code></td></tr>
</table>

### Heuristics — baseline & average-cost

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/lotforlotsolver.html"><strong>Lot-for-Lot</strong></a><br><sub>Heuristic</sub><br><code>LotForLotSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/periodicorderquantitysolver.html"><strong>Periodic Order Quantity</strong></a><br><sub>Heuristic</sub><br><code>PeriodicOrderQuantitySolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/silvermealsolver.html"><strong>Silver–Meal</strong></a><br><sub>Heuristic</sub><br><code>SilverMealSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/segerstedtreformulatedsilvermealsolver.html"><strong>Reformulated Silver–Meal</strong></a><br><sub>Heuristic</sub><br><code>SegerstedtReformulatedSilverMealSolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/leastunitcostsolver.html"><strong>Least Unit Cost</strong></a><br><sub>Heuristic</sub><br><code>LeastUnitCostSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/chiumodifiedleastunitcostsolver.html"><strong>Chiu modified Least Unit Cost</strong></a><br><sub>Heuristic</sub><br><code>ChiuModifiedLeastUnitCostSolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/hochangsolisnetleastperiodcostsolver.html"><strong>Ho–Chang–Solis nLPC</strong></a><br><sub>Heuristic</sub><br><code>HoChangSolisNetLeastPeriodCostSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/hochangsolisimprovednetleastperiodcostsolver.html"><strong>Ho–Chang–Solis nLPC(i)</strong></a><br><sub>Heuristic</sub><br><code>HoChangSolisImprovedNetLeastPeriodCostSolver</code></td></tr>
</table>

### Heuristics — part-period

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/partperiodsimplifiedsolver.html"><strong>Part-Period Simplified</strong></a><br><sub>Heuristic</sub><br><code>PartPeriodSimplifiedSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/partperiodbalancingsolver.html"><strong>Part-Period Balancing</strong></a><br><sub>Heuristic</sub><br><code>PartPeriodBalancingSolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/chiutingmodifiedpartperiodbalancingsolver.html"><strong>Chiu–Ting modified PPB</strong></a><br><sub>Heuristic</sub><br><code>ChiuTingModifiedPartPeriodBalancingSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/pattersonlaforgeincrementalpartperiodsolver.html"><strong>Patterson–LaForge incremental part-period</strong></a><br><sub>Heuristic</sub><br><code>PattersonLaForgeIncrementalPartPeriodSolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wemmerlovmodifiedpartperiodbalancingsolver.html"><strong>Wemmerlöv modified PPB</strong></a><br><sub>Heuristic</sub><br><code>WemmerlovModifiedPartPeriodBalancingSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wemmerlovppblookaheadlookbacksolver.html"><strong>Wemmerlöv PPB Look-Ahead / Look-Back</strong></a><br><sub>Heuristic</sub><br><code>WemmerlovPpbLookAheadLookBackSolver</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/wemmerlovmodifiedppblookaheadlookbacksolver.html"><strong>Wemmerlöv modified PPB Look-Ahead / Look-Back</strong></a><br><sub>Heuristic</sub><br><code>WemmerlovModifiedPpbLookAheadLookBackSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/mclarenordermomentsolver.html"><strong>McLaren Order Moment</strong></a><br><sub>Heuristic</sub><br><code>McLarenOrderMomentSolver</code></td></tr>
</table>

### Heuristics — marginal-cost

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/groffsolver.html"><strong>Groff</strong></a><br><sub>Heuristic</sub><br><code>GroffSolver</code></td><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/freelandcolleysolver.html"><strong>Freeland–Colley</strong></a><br><sub>Heuristic</sub><br><code>FreelandColleySolver</code></td></tr>
</table>

### Heuristics — global merge

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/ULSAlgorithms/algorithms/karnimaximumpartperiodgainsolver.html"><strong>Karni Maximum Part-Period Gain</strong></a><br><sub>Heuristic</sub><br><code>KarniMaximumPartPeriodGainSolver</code></td><td width="50%">&nbsp;</td></tr>
</table>

## Current algorithm inventory

**22 exact strategies + 19 heuristics = 41 public `IUlsSolver` strategies.**

## Documentation structure

The user-facing documentation is intentionally simple:

- **Home:** browse all algorithms as cards and filter by method family.
- **Algorithm page:** one identical structure for every method.
- **Getting Started:** the shortest path from arrays to a solution.
- **Simple API:** only the common objects needed by most users.
- **Advanced API:** generated Doxygen reference for implementation details.
- **Validation & benchmarks:** scientific and engineering evidence.

Release-oriented “Pack I / Pack II” pages are no longer part of the user navigation. Release provenance remains available through GitHub releases and internal reproducibility notes.

---

<p align="center">
  <strong>Lemoine-OR Algorithms</strong><br>
  Clean. Scientific. Open.
</p>
