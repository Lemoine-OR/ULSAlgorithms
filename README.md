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
- **2 exact `(l,S)` cutting-plane strategies**;
- **22 exact `IUlsSolver` strategies in total**;
- **11 heuristics**;
- **33 public solving strategies**;
- automatic **CPLEX → Gurobi → Xpress → CBC** discovery;
- solver-independent mathematical formulations and execution;
- numerical normalization and two-level independent checking;
- complete cut-generation traceability.

Developed and maintained by **David Lemoine — Lemoine-OR**.

## Classical `(l,S)` cutting planes

v0.20.0 adds two separate exact strategies:

```text
GeneralLsCuttingPlaneSolver
WagnerWhitinLsCuttingPlaneSolver
```

The general separator performs exact combinatorial separation over the
classical exponential `(l,S)` family in O(T²) time by selecting, for every
period, the smaller of the production term and the setup-covered-demand term.

The Wagner-Whitin separator scans the O(T²) prefix-S specialization equivalent
to:

```text
I[k-1] + Σ(j=k..l) d[j,l] y[j] >= d[k,l]
```

and requires the no-speculative-motive cost condition.

## Use

```csharp
IUlsSolver solver =
    new GeneralLsCuttingPlaneSolver();

UlsSolveResult result =
    solver.Solve(problem);
```

or asynchronously:

```csharp
IAsyncUlsSolver solver =
    new WagnerWhitinLsCuttingPlaneSolver();

UlsSolveResult result =
    await solver.SolveAsync(
        problem,
        cancellationToken);
```

## See every generated constraint

```csharp
if (result is CuttingPlaneUlsSolveResult r)
{
    foreach (CutRecord cut in
             r.CuttingPlaneExecution.Cuts.Cuts)
    {
        Console.WriteLine(
            $"{cut.Iteration} | " +
            $"{cut.Definition} | " +
            $"{cut.Disposition} | " +
            $"{cut.DispositionReason}");
    }
}
```

Each record retains `l`, `S`, coefficients, RHS, sense, violation, efficacy,
iteration, disposition and the exact row name inserted into the portable model.

## Exact architecture

```text
root aggregate LP
    ↓
(l,S) separation
    ↓
add unique violated cuts
    ↓
repeat
    ↓
strengthened final MILP
    ↓
UlsSolution reconstruction
    ↓
independent ULS checker
```

The final MILP uses the same solver selected during root separation. Automatic
priority remains CPLEX, Gurobi, Xpress, CBC.

## Scientific provenance

Main `(l,S)` references:

- Barany, Van Roy & Wolsey (1984),
  *Uncapacitated lot-sizing: the convex hull of solutions*,
  DOI `10.1007/BFb0121006`.
- Barany, Van Roy & Wolsey (1984),
  *Strong Formulations for Multi-Item Capacitated Lot Sizing*,
  DOI `10.1287/mnsc.30.10.1255`.
- Pochet & Wolsey (1994),
  *Polyhedra for lot-sizing with Wagner-Whitin costs*,
  DOI `10.1007/BF01582225`.

---

<p align="center">
  <strong>Lemoine-OR Algorithms</strong><br>
  Clean. Scientific. Open.
</p>
