# ULSAlgorithms v0.27.0 — Configurable Solver Factory

Base: v0.26.0 / commit `431c469249629c407174f59e00b3b1d49eb6c49f`

## Purpose

v0.26.0 introduced a canonical runtime catalog and stable-ID factory for all
42 public `IUlsSolver` strategies.

v0.27.0 extends that architecture with strict constructor-level configuration
while preserving the original `UlsSolverFactory.Create(string id)` API.

No algorithm is added or removed.

## New API

### `UlsSolverCreationOptions`

This light composition object reuses the option models already owned by the
algorithms:

- `AdaptiveGeneralFallback`
- `MaxDegreeOfParallelism`
- `ParallelThreshold`
- `OptimizationExecution` (`LinearModelSolveOptions`)
- `CuttingPlane` (`LsCuttingPlaneOptions`)

No duplicate optimization or cutting-plane configuration hierarchy is created.

### Configured factory overload

```csharp
IUlsSolver solver =
    UlsSolverFactory.Create(
        "adaptive-exact",
        new UlsSolverCreationOptions
        {
            AdaptiveGeneralFallback =
                UlsGeneralExactFallback.FedergruenTzurGeneral
        });
```

`TryCreate` receives an equivalent configured overload.

### Descriptor-level construction

`UlsSolverDescriptor.Create(UlsSolverCreationOptions)` exposes the same path,
which keeps metadata-driven clients independent from concrete solver classes.

## Strict compatibility validation

Configuration is never silently ignored.

Examples:

- adaptive fallback on `wagelmans-general` -> rejected;
- cutting-plane options on a plain formulation -> rejected;
- invalid Lyu-Lee worker count -> rejected at creation;
- invalid `LinearModelSolveOptions` -> rejected at creation.

An empty `UlsSolverCreationOptions` remains exactly equivalent to the historical
default factory path.

## Configuration capability metadata

New flags:

- `AdaptiveGeneralFallback`
- `Parallelism`
- `OptimizationExecution`
- `CuttingPlane`

`UlsSolverDescriptor` now exposes:

- `ConfigurationCapabilities`
- `SupportsConfiguration`

`UlsSolverCatalog.Configurable` contains the eight currently configurable
strategies:

1. `adaptive-exact`
2. `lyu-lee-parallel`
3. `aggregate-inventory-formulation`
4. `facility-location-formulation`
5. `shortest-path-formulation`
6. `inventory-eliminated-formulation`
7. `general-ls-cutting-plane`
8. `wagner-whitin-ls-cutting-plane`

## External optimization selection

The factory passes the existing `LinearModelSolveOptions` to solver-backed
strategies.

This preserves the current automatic priority:

`CPLEX -> Gurobi -> Xpress -> COIN-OR CBC`

and allows explicit `SolverKind.Cplex`, `Gurobi`, `Xpress` or `CoinOrCbc`
selection, including `AllowFallbackWhenExplicit` and all existing numerical and
file-management options.

## Cutting-plane engineering

The two `(l,S)` cutting-plane strategies accept both:

- `LinearModelSolveOptions`
- `LsCuttingPlaneOptions`

The latter retains all existing engineering controls:

- maximum root iterations;
- violation tolerance;
- minimum efficacy;
- cut selection policy;
- maximum cuts per iteration.

No cut traceability behavior is changed.

## Catalog remains the source of truth

Configured factories are stored on the relevant `UlsSolverDescriptor` entries,
not in a separate switch table.

`docs/algorithm-catalog.json` moves to schema version 3 and projects
`configurationCapabilities` for every strategy.

`Test-SolverCatalog.ps1` continues to guarantee runtime/documentation
synchronization.

## Compatibility

- existing `IUlsSolver` API unchanged;
- all existing concrete constructors unchanged;
- `UlsSolverFactory.Create(string id)` unchanged;
- strategy IDs unchanged;
- strategy count unchanged: 42;
- default adaptive policy unchanged;
- default external-solver priority unchanged.

## Validation targets

The v0.27.0 tests verify:

- eight configurable strategies;
- adaptive fallback configuration;
- Lyu-Lee parallel configuration;
- explicit CBC formulation construction;
- combined execution/cutting-plane configuration;
- default factory compatibility;
- rejection of irrelevant options;
- rejection of invalid parallel settings;
- rejection of invalid optimization options;
- descriptor-level configured creation;
- configured `TryCreate` behavior;
- capability metadata.
