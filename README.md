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

**ULSAlgorithms** is a research-oriented C# library for deterministic
**Uncapacitated Lot-Sizing (ULS)**.

The library currently contains:

- **22 exact `IUlsSolver` strategies**;
- **11 heuristics**;
- **33 public solving strategies**;
- four mathematical-programming formulations;
- general and Wagner–Whitin `(l,S)` cutting-plane solvers;
- automatic **CPLEX → Gurobi → Xpress → CBC** discovery;
- numerical normalization and independent solution checking;
- cut-pool policies, convergence statistics and BenchmarkDotNet separator
  benchmarks.

Developed and maintained by **David Lemoine — Lemoine-OR**.

## Cutting-plane engineering

v0.21.0 adds configurable root cut-pool strategies:

```text
AllViolated
MostViolatedPerL
TopByViolation
TopByEfficacy
```

Example:

```csharp
var cuts =
    new LsCuttingPlaneOptions
    {
        SelectionPolicy =
            CutSelectionPolicy.TopByViolation,
        MaximumCutsPerIteration =
            20,
        MinimumEfficacy =
            1e-4
    };

IUlsSolver solver =
    new GeneralLsCuttingPlaneSolver(
        cuttingPlaneOptions: cuts);
```

Every generated cut remains traceable. Eligible cuts rejected only because of
the pool policy receive `CutDisposition.NotSelected`.

## Convergence

`CuttingPlaneExecutionReport.Convergence` provides root bound evolution,
LP/separation time, candidate counts, selected/added cuts, final MILP objective
and the fraction of the initial root gap closed by `(l,S)` cuts.

## Benchmarks

`LsSeparationBenchmarks` compares the pure general and Wagner–Whitin separators
for horizons 50, 100, 250 and 500, independently of external solver time.

## Exactness

Selection policies affect root strengthening only. The final model restores
binary setup variables and is solved exactly with the same selected
optimization engine.

---

<p align="center">
  <strong>Lemoine-OR Algorithms</strong><br>
  Clean. Scientific. Open.
</p>
