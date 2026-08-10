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

The current library contains:

- **16 exact algorithms**;
- **11 heuristics**;
- **27 public solving strategies** sharing `IUlsSolver`;
- deterministic and randomized cross-validation against independent exact references;
- BenchmarkDotNet performance suites;
- versioned Doxygen API documentation;
- validated GitHub Releases with checksums and reproducibility metadata.

Developed and maintained by **David Lemoine — Lemoine-OR**.

## Documentation

The public portal is the recommended entry point:

### [Open the ULSAlgorithms documentation portal](https://lemoine-or.github.io/ULSAlgorithms/)

The portal is organized for algorithm users and researchers rather than around the raw source tree:

1. **Getting Started**
2. **ULS Problem & Notation**
3. **Algorithm Catalog**
4. **Exact Algorithms**
5. **Heuristics**
6. **Algorithm Selection**
7. **Complexity & Applicability**
8. **Validation & Benchmarks**
9. **API Reference**
10. **Scientific References**
11. **Releases & Reproducibility**

Each algorithm is documented with its family, assumptions, asymptotic complexity, scientific source, implementation notes and validation strategy.

## Quick start

```csharp
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.WagnerWhitin;
using ULSAlgorithms.Models;

var problem = new UlsProblem(
    demands:            [20.0, 30.0, 25.0, 40.0],
    setupCosts:         [100.0, 100.0, 100.0, 100.0],
    unitProductionCosts:[  0.0,   0.0,   0.0,   0.0],
    holdingCosts:       [  2.0,   2.0,   2.0,   0.0]);

IUlsSolver solver = new WagnerWhitinSolver();

var result = solver.Solve(problem);

Console.WriteLine($"Solver: {solver.Name}");
Console.WriteLine($"Status: {result.Status}");
Console.WriteLine($"Objective: {result.ObjectiveValue}");
```

All exact methods and heuristics use the same interface:

```csharp
UlsSolveResult Solve(
    UlsProblem problem,
    CancellationToken cancellationToken = default);
```

That common contract allows algorithms to be exchanged as Strategy implementations without changing the calling code.

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

The complete applicability and complexity matrix is maintained in `docs/algorithm-catalog.json` and rendered automatically in the documentation.

## Repository architecture

```text
ULSAlgorithms/
├── src/ULSAlgorithms/
│   ├── Abstractions/      common solver contract
│   ├── Models/            validated ULS problem
│   ├── Results/           solution and solve-status model
│   ├── Exact/             exact algorithms by family
│   └── Heuristics/        heuristic strategies
├── tests/                 xUnit validation and cross-checks
├── benchmarks/            BenchmarkDotNet suites
├── docs/
│   ├── pages/             curated scientific/user documentation
│   ├── portal/            public landing portal
│   ├── assets/            project identity and Doxygen assets
│   ├── brand/             shared algorithm-project identity guide
│   └── algorithm-catalog.json
├── build/                 validated build and release automation
└── tools/                 versioning and tooling bootstrap scripts
```

## Validation philosophy

Fast algorithms are useful only if their result can be trusted.

The project therefore combines:

- deterministic reference instances;
- randomized instance campaigns;
- independent quadratic dynamic-programming oracles where appropriate;
- cross-validation between mathematically independent exact implementations;
- explicit applicability tests for restricted algorithms;
- cancellation tests;
- objective and feasibility reconstruction;
- BenchmarkDotNet performance measurements.

Heuristics return **`Feasible`**, never `Optimal`. Exact methods return **`Optimal`** only after completing an exact algorithm.

## Build from source

Requirements for the validated code build:

- .NET 10 SDK;
- PowerShell.

```powershell
git clone https://github.com/Lemoine-OR/ULSAlgorithms.git
cd ULSAlgorithms

powershell -ExecutionPolicy Bypass `
  -File ".\build\Build-Validated.ps1"
```

For the complete documentation build, Graphviz and Doxygen are installed through the repository tooling:

```powershell
.\tools\Install-Graphviz.ps1
.\tools\Install-Doxygen.ps1
.\docs\build-documentation.ps1
```

The generated portal is written to:

```text
Documentation/site/index.html
```

## Releases and reproducibility

Public releases are created only through the validated GitHub Actions release workflow.

A release contains the binary ZIP, documentation ZIP, build metadata, manifests and SHA-256 checksums. The build version and Git commit are injected into the documentation portal so a published API snapshot can always be traced back to its exact source revision.

## Scientific provenance

ULSAlgorithms is not intended to hide the literature behind a single black-box solver. Public algorithms preserve their scientific identity.

When an implementation materially follows a paper, the source documentation records the publication and—when available—the DOI. When an implementation is a modern reconstruction rather than a line-by-line transcription of historical code, that distinction is stated explicitly.

See the [Scientific References](https://lemoine-or.github.io/ULSAlgorithms/api/scientific_references.html) page for the curated bibliography.

---

<p align="center">
  <strong>Lemoine-OR Algorithms</strong><br>
  Clean. Scientific. Open.
</p>
