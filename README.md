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

The library now contains:

- **16 direct/native exact algorithms**;
- **4 solver-backed exact formulation strategies**;
- **20 exact `IUlsSolver` strategies in total**;
- **11 heuristics**;
- **31 public solving strategies**;
- automatic **CPLEX → Gurobi → Xpress → CBC** discovery;
- a provider-independent linear-model execution layer;
- two-level independent solution validation;
- cutting-plane traceability infrastructure.

Developed and maintained by **David Lemoine — Lemoine-OR**.

## Use a formulation like any other exact solver

```csharp
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Exact.Formulations;

IUlsSolver solver =
    new AggregateInventoryFormulationSolver();

UlsSolveResult result =
    solver.Solve(problem);
```

The same common `IUlsSolver` contract is used by Wagner–Whitin, Wagelmans,
Federgruen–Tzur, classical heuristics and the solver-backed formulations.

## Asynchronous solver-backed API

```csharp
IAsyncUlsSolver solver =
    new FacilityLocationFormulationSolver();

UlsSolveResult result =
    await solver.SolveAsync(
        problem,
        cancellationToken);
```

The four public formulation strategies are:

```text
AggregateInventoryFormulationSolver
FacilityLocationFormulationSolver
ShortestPathFormulationSolver
InventoryEliminatedFormulationSolver
```

## Solver provenance

When the result comes from a formulation strategy:

```csharp
if (result is SolverBackedUlsSolveResult solverBacked)
{
    Console.WriteLine(solverBacked.FormulationKind);
    Console.WriteLine(
        solverBacked.OptimizationSolver?.SelectedSolver);
    Console.WriteLine(
        solverBacked.OptimizationSolver?.SolverVersion);
}
```

Thus a published benchmark can retain both the mathematical formulation and the
actual optimization engine used on the machine.

## Two validation layers

A solver-backed solution must pass:

```text
native solver
    ↓
portable LinearModel checker
    ↓
formulation → UlsSolution reconstruction
    ↓
ULS-domain checker
    ↓
objective agreement check
    ↓
Optimal / Feasible
```

The public `UlsSolutionValidator` independently checks material balance, final
inventory, setup linking and every cost component.

## Numerical normalization

The numerical cleanup introduced in v0.18.0 is retained:

```text
zero tolerance          1e-8
integrality tolerance   1e-7
near-integer tolerance  1e-8
```

Small numerical residues are cleaned; materially incorrect values are never
silently rounded.

## Automatic solver policy

Solver-backed methods use:

```text
1. IBM ILOG CPLEX
2. Gurobi
3. FICO Xpress
4. COIN-OR CBC
```

No commercial solver assembly is referenced at compile time.

## Cutting-plane traceability

The existing cutting-plane report records every generated `(l,S)` inequality,
whether it was added, its iteration, violation, efficacy, coefficients,
disposition and solver row name.

With the formulation strategies and reconstruction layer complete, the next
solver-backed algorithm family can reuse the same execution and validation
pipeline.

## Build from source

```powershell
powershell -ExecutionPolicy Bypass -File ".\build\Build-Validated.ps1"
```

## Scientific provenance

ULSAlgorithms keeps direct algorithms and mathematical formulations as separate
public implementations so their assumptions, scientific sources, complexity and
computational behavior remain independently citable and benchmarkable.

See the [Scientific References](https://lemoine-or.github.io/ULSAlgorithms/api/scientific_references.html).

---

<p align="center">
  <strong>Lemoine-OR Algorithms</strong><br>
  Clean. Scientific. Open.
</p>
