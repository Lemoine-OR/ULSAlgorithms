<p align="center">
  <img src="docs/assets/ulsalgorithms-logo.svg" alt="ULSAlgorithms" width="560">
</p>

<p align="center">
  <strong>High-performance exact and heuristic algorithms for the Uncapacitated Lot-Sizing problem (ULS), implemented in C# / .NET.</strong>
</p>

<p align="center">
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/actions/workflows/build.yml"><img alt="Build and Test" src="https://github.com/Lemoine-OR/ULSAlgorithms/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/actions/workflows/documentation.yml"><img alt="Documentation" src="https://github.com/Lemoine-OR/ULSAlgorithms/actions/workflows/documentation.yml/badge.svg"></a>
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/releases/latest"><img alt="Latest release" src="https://img.shields.io/github/v/release/Lemoine-OR/ULSAlgorithms?display_name=tag&sort=semver"></a>
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512BD4">
</p>

<p align="center">
  <a href="https://lemoine-or.github.io/ULSAlgorithms/"><strong>Documentation portal</strong></a>
  ·
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/releases/latest"><strong>Latest release</strong></a>
  ·
  <a href="https://github.com/Lemoine-OR/ULSAlgorithms/actions"><strong>CI / CD</strong></a>
</p>

---

## Overview

**ULSAlgorithms** is a research-oriented C# library for solving the deterministic **Uncapacitated Lot-Sizing problem** with a common Strategy-pattern API.

The repository deliberately keeps historically important methods as **separate public implementations**. A classical Wagner–Whitin dynamic program, a low-storage Evans implementation, geometric dynamic programs, planning-horizon accelerations, network formulations, parallel methods and classical heuristics therefore remain independently testable, benchmarkable and citable.

The current algorithm catalog contains:

- **16 exact algorithms**;
- **11 heuristics**;
- **27 public solving strategies** sharing `IUlsSolver`.

The infrastructure also defines a common architecture for future **solver-backed formulations and cutting-plane methods**, including automatic optimization-solver selection and complete cut-generation traceability.

Developed and maintained by **David Lemoine — Lemoine-OR**.

## Optimization solver policy

Any ULSAlgorithms method that requires a mathematical optimizer uses a common selection layer.

With `SolverKind.Automatic`, the default priority is:

```text
1. IBM ILOG CPLEX
2. Gurobi
3. FICO Xpress
4. COIN-OR CBC
```

This order intentionally matches LotSizingDataModel.

Selection is based on **real adapter availability and required capabilities**. An installed solver that cannot load, has no usable license, or lacks a required capability is skipped with diagnostics.

The caller may still explicitly request a concrete solver and may disable fallback.

## Cutting-plane traceability

Solver-backed cutting-plane algorithms must expose the inequalities they generate.

For `(l,S)` cuts the public trace records:

- `l` and `S`;
- all nonzero coefficients;
- sense and right-hand side;
- separation procedure (`WagnerWhitin` or `General`);
- iteration;
- violation and efficacy;
- whether the cut was actually added;
- duplicate / below-tolerance / invalid / solver-rejected reason;
- solver constraint name.

`CutGenerationReport` aggregates generated and added counts while retaining the complete ordered cut list.

## Documentation

The public portal is the recommended entry point:

### [Open the ULSAlgorithms documentation portal](https://lemoine-or.github.io/ULSAlgorithms/)

In addition to algorithm documentation, see:

- **Optimization Solver Integration**
- **Cut Generation Traceability**
- **Validation & Benchmarks**
- **Scientific References**
- **Releases & Reproducibility**

## Quick start

```csharp
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;

var problem = new UlsProblem(
    demands:             [20.0, 30.0, 25.0, 40.0],
    setupCosts:          [100.0, 100.0, 100.0, 100.0],
    unitProductionCosts: [  0.0,   0.0,   0.0,   0.0],
    holdingCosts:        [  2.0,   2.0,   2.0,   0.0]);

IUlsSolver solver = new WagnerWhitinSolver();

var result = solver.Solve(problem);

Console.WriteLine($"Solver: {solver.Name}");
Console.WriteLine($"Status: {result.Status}");
Console.WriteLine($"Objective: {result.ObjectiveValue}");
```

All exact methods and heuristics use:

```csharp
UlsSolveResult Solve(
    UlsProblem problem,
    CancellationToken cancellationToken = default);
```

## Automatic optimization-solver selection

Solver-backed methods use the shared optimization layer:

```csharp
var options = new SolverSelectionOptions();
options.RequiredCapabilities.Add(
    SolverCapability.MixedIntegerLinearProgramming);

var selection = await new SolverSelectionService().SelectAsync(
    SolverKind.Automatic,
    registry,
    options,
    cancellationToken);
```

Concrete provider adapters perform the real machine / native-library / license check.

## Algorithm families

| Family | Representative public strategies |
|---|---|
| Classical dynamic programming | `WagnerWhitinClassicalSolver`, `WagnerWhitinEvansSolver`, `SaydamMcKnewFastWagnerWhitinSolver` |
| Geometric / accelerated DP | `WagnerWhitinSolver`, `WagelmansGeneralSolver`, `FedergruenTzurSolver`, `AggarwalParkSolver` |
| Planning-horizon methods | `BahlTajPlanningHorizonSolver`, `HeadyZhuEconomicPartPeriodSolver`, `SadjadiAryanezhadSadeghiSolver` |
| Linear specialized exact methods | `ChowdhuryBakiAzabSolver`, Federgruen–Tzur specializations |
| Network / shortest path | `ZangwillNetworkSolver` |
| Branch and bound | `JacobsKhumawalaBranchAndBoundSolver` |
| Parallel exact DP | `LyuLeeParallelSolver` |
| Classical heuristics | Silver–Meal, LUC, PPB, Groff, POQ, Freeland–Colley, IPPA, Wemmerlöv variants |

## Repository architecture

```text
ULSAlgorithms/
├── src/ULSAlgorithms/
│   ├── Abstractions/      common ULS strategy contract
│   ├── Models/            validated ULS problem
│   ├── Results/           solution and solve-status model
│   ├── Exact/             exact algorithms by family
│   ├── Heuristics/        heuristic strategies
│   ├── Optimization/      external-solver selection and provenance
│   └── CuttingPlanes/     solver-independent cut traceability
├── tests/                 xUnit validation and cross-checks
├── benchmarks/            BenchmarkDotNet suites
├── docs/                  portal, API, scientific documentation
├── build/                 validated build and release automation
└── tools/                 versioning and tooling bootstrap scripts
```

## Validation philosophy

Fast algorithms are useful only if their result can be trusted.

The project combines deterministic reference instances, randomized campaigns,
independent exact oracles, cross-validation, cancellation tests, applicability
tests and BenchmarkDotNet measurements.

Solver-backed infrastructure is tested separately for deterministic selection
order, capability filtering, fallback behavior and complete generated/added cut
accounting.

Heuristics return **`Feasible`**, never `Optimal`. Exact methods return
**`Optimal`** only after completing an exact algorithm.

## Build from source

Requirements:

- .NET 10 SDK;
- PowerShell.

```powershell
powershell -ExecutionPolicy Bypass -File ".\build\Build-Validated.ps1"
```

For documentation:

```powershell
.\tools\Install-Graphviz.ps1
.\tools\Install-Doxygen.ps1
.\docs\build-documentation.ps1
```

## Releases and reproducibility

Public releases are created only through the validated GitHub Actions release
workflow.

A release contains the binary ZIP, documentation ZIP, build metadata,
manifests and SHA-256 checksums. Solver-backed execution reports also retain the
concrete solver and adapter identity selected on the execution machine.

## Scientific provenance

ULSAlgorithms does not hide the literature behind a single black-box solver.
Public algorithms preserve their scientific identity, assumptions, complexity
and implementation provenance.

See the [Scientific References](https://lemoine-or.github.io/ULSAlgorithms/api/scientific_references.html).

---

<p align="center">
  <strong>Lemoine-OR Algorithms</strong><br>
  Clean. Scientific. Open.
</p>
