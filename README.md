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

**ULSAlgorithms** is a research-oriented C# library for deterministic **Uncapacitated Lot-Sizing (ULS)**.

The repository keeps historically important algorithms as distinct public
implementations so their scientific identity, assumptions, complexity and
performance remain independently testable.

The current library contains:

- **16 exact algorithms**;
- **11 heuristics**;
- **27 public `IUlsSolver` strategies**;
- **4 classical mathematical-programming formulations**;
- automatic **CPLEX → Gurobi → Xpress → CBC** discovery;
- a solver-independent mathematical-model execution layer;
- complete cutting-plane traceability infrastructure.

Developed and maintained by **David Lemoine — Lemoine-OR**.

## Automatic solver policy

Solver-backed methods use the same priority as LotSizingDataModel:

```text
1. IBM ILOG CPLEX
2. Gurobi
3. FICO Xpress
4. COIN-OR CBC
```

An installed but unusable solver is skipped when automatic selection is used.

## Mathematical formulations

The four current builders are:

```text
AggregateInventoryFormulationBuilder
FacilityLocationFormulationBuilder
ShortestPathFormulationBuilder
InventoryEliminatedFormulationBuilder
```

They return a provider-independent `LinearModel`.

## Execute a portable model

Starting with v0.18.0:

```csharp
var formulation =
    new AggregateInventoryFormulationBuilder()
        .Build(problem);

var modelSolver =
    new LinearModelSolver();

LinearModelSolveResult result =
    await modelSolver.SolveAsync(
        formulation.Model,
        new LinearModelSolveOptions
        {
            Solver = SolverKind.Automatic
        },
        cancellationToken);

Console.WriteLine(result.Solver?.SelectedSolver);
Console.WriteLine(result.Status);
Console.WriteLine(result.ObjectiveValue);
```

The execution backend is selected only after the model has been built.

## Independent validation

Every returned candidate solution is checked again against the portable model.

The checker verifies:

- bounds;
- binary/integer integrality;
- every linear constraint;
- objective reconstruction.

A native solver cannot produce an `Optimal` ULSAlgorithms result when the
independent checker rejects its returned values.

## Provider execution

| Solver | Execution mechanism |
|---|---|
| CPLEX | stand-alone `cplex` executable + XML `.sol` parser |
| Gurobi | `gurobi_cl` + portable text solution |
| Xpress | `Optimizer.dll` reflection (`ReadProb` / `Optimize` / `GetSolution`) |
| CBC | stand-alone `cbc` executable + portable text solution |

No commercial solver assembly is referenced at compile time.

## Cutting-plane traceability

The existing cutting-plane report records every generated `(l,S)` inequality,
whether it was added, its iteration, violation, efficacy, coefficients,
disposition and solver row name.

The future `(l,S)` algorithms will use the same execution layer introduced in
v0.18.0.

## Repository architecture

```text
ULSAlgorithms/
├── src/ULSAlgorithms/
│   ├── Abstractions/
│   ├── Models/
│   ├── Results/
│   ├── Exact/
│   ├── Heuristics/
│   ├── Formulations/
│   ├── Optimization/
│   │   ├── Modeling/
│   │   ├── Adapters/
│   │   ├── Execution/
│   │   │   └── Providers/
│   │   └── External/
│   └── CuttingPlanes/
├── tests/
├── benchmarks/
├── docs/
├── build/
└── tools/
```

## Build from source

```powershell
powershell -ExecutionPolicy Bypass -File ".\build\Build-Validated.ps1"
```

The main project has no compile-time dependency on CPLEX, Gurobi, Xpress or CBC.

## Scientific provenance

ULSAlgorithms does not hide the literature behind a single black-box solver.
Public algorithms and formulations preserve their source, assumptions and
implementation provenance.

See the [Scientific References](https://lemoine-or.github.io/ULSAlgorithms/api/scientific_references.html).

---

<p align="center">
  <strong>Lemoine-OR Algorithms</strong><br>
  Clean. Scientific. Open.
</p>
