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

- **22 exact `IUlsSolver` strategies**;
- **15 heuristic `IUlsSolver` strategies**;
- **37 public solving strategies**;
- four mathematical-programming formulations;
- general and Wagner-Whitin `(l,S)` cutting-plane solvers;
- automatic **CPLEX → Gurobi → Xpress → CBC** discovery;
- numerical normalization and independent solution checking;
- cutting-plane convergence engineering and BenchmarkDotNet benchmarks.

Developed and maintained by **David Lemoine — Lemoine-OR**.

## v0.22.0 literature heuristics

Four additional public methods are available:

```text
PartPeriodSimplifiedSolver
SegerstedtReformulatedSilverMealSolver
ChiuModifiedLeastUnitCostSolver
ChiuTingModifiedPartPeriodBalancingSolver
```

The release also separates **Part-Period Simplified / no-overshoot LTC** from
nearest-EPP **Part-Period Balancing** instead of treating those rules as one
algorithm.

## Scientific sources

- DeMatteis (1968), *An Economic Lot-Sizing Technique I: The Part-Period Algorithm*.
- Baciarello et al. (2013), DOI `10.5772/56004`.
- Segerstedt, Abdul-Jalbar & Samuelsson (2023),
  DOI `10.3390/axioms12070661`.
- Chiu (2004), DOI `10.1080/09720510.2004.10701115`.
- Chiu, Ting & Chiu (2005), *A modified version of the part period lot-sizing heuristic*.

Methods whose detailed published rules are not yet available are not
mislabelled or reconstructed from abstracts.

---

<p align="center">
  <strong>Lemoine-OR Algorithms</strong><br>
  Clean. Scientific. Open.
</p>
