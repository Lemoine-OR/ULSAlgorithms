# ULSAlgorithms v1.0.0 — stable release freeze

## Purpose

v1.0.0 is deliberately a stability release, not a feature release.

The complete implementation, scientific inventory, numerical policies and
release engineering were qualified in v0.29.0. This pack changes only the
declared version and the documentation/automation wording required to establish
the stable 1.x compatibility contract.

## Product surface frozen at 1.0.0

The stable compatibility contract includes:

- exported public .NET types and members in `ULSAlgorithms`;
- `IUlsSolver` and the public result model;
- all existing stable solver IDs in `UlsSolverCatalog`;
- `UlsSolverConfiguration` schema version 1.

The complete public strategy inventory remains unchanged:

```text
17 direct exact algorithms
 4 mathematical formulations
 2 (l,S) cutting-plane methods
19 heuristics
------------------------------
42 public strategies
```

## No functional changes

This pack adds no:

- algorithm;
- heuristic;
- formulation;
- cutting-plane method;
- solver adapter;
- public API member;
- numerical tolerance change;
- solver-selection change.

The expected test inventory therefore remains 272 tests.

## Release validation

The v1.0.0 release must pass the same gates as v0.29.0:

- all repository .NET projects build in Release on Windows and Linux;
- complete unit-test suite passes on both platforms;
- Linux portability smoke passes;
- real CBC qualification passes for all six solver-backed strategies;
- runtime/documentation catalog synchronization passes;
- public API compatibility validation passes;
- official .NET package validation passes;
- isolated NuGet consumer smoke returns objective 680;
- `.nupkg` and `.snupkg` validation passes;
- complete release assets/manifests/checksums validation passes.

## Official package baseline

`PackageValidationBaselineVersion` is not set to `1.0.0` inside the 1.0.0
release itself. Once the stable package is available from the NuGet feed chosen
for package-baseline resolution, v1.0.0 can be configured as the official .NET
package-validation baseline for subsequent compatible 1.x releases.

The repository public-API snapshot remains the immediate breaking-change gate.
