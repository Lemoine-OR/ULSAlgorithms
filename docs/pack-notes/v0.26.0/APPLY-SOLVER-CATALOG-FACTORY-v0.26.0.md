# ULSAlgorithms v0.26.0 — Runtime Solver Catalog and Factory

Base: v0.25.0 / commit `204786e7cbde22a8644c0475aa10f8bb234b0579`

## Purpose

v0.26.0 adds a stable programmatic inventory for every public ULS strategy.
The release does not add or remove an algorithm: the public inventory remains
42 `IUlsSolver` strategies.

The objective is to remove application-level switches over concrete classes and
make ULSAlgorithms easier to integrate into reusable subproblem libraries,
configuration-driven applications, benchmark campaigns, consoles and user
interfaces.

## New public API

Namespace: `ULSAlgorithms.Catalog`

- `UlsSolverCategory`
- `UlsSolverDescriptor`
- `UlsSolverCatalog`
- `UlsSolverFactory`

Main views:

- `UlsSolverCatalog.All` — 42
- `UlsSolverCatalog.Exact` — 23
- `UlsSolverCatalog.DirectExact` — 17
- `UlsSolverCatalog.Formulations` — 4
- `UlsSolverCatalog.CuttingPlanes` — 2
- `UlsSolverCatalog.Heuristics` — 19

## Stable IDs

Every strategy now has one normalized lower-kebab-case ID. Examples:

- `adaptive-exact`
- `wagner-whitin-linear`
- `wagelmans-general`
- `federgruen-tzur-general`
- `aggregate-inventory-formulation`
- `general-ls-cutting-plane`
- `silver-meal`
- `karni-maximum-part-period-gain`

Lookup is case-insensitive, while the published canonical IDs remain lowercase.

## Factory

```csharp
IUlsSolver solver =
    UlsSolverFactory.Create("wagelmans-general");
```

The default construction policy is the same as direct construction of each
strategy. Solver-backed formulations and cutting-plane methods can therefore be
created without an installed optimization engine; automatic engine discovery is
only relevant when solving.

## Catalog metadata

Each descriptor exposes:

- stable ID and display name;
- `UlsSolverKind`;
- operational category;
- family;
- time and memory complexity;
- applicability text;
- external-solver requirement;
- scientific reference and DOI;
- implementation note;
- source path;
- implementation `Type`;
- a fresh-instance factory.

## Documentation synchronization

The runtime C# catalog is now the canonical metadata source.

A new console tool:

`tools/ULSAlgorithms.CatalogExporter`

projects the runtime catalog into `docs/algorithm-catalog.json`.

The repository adds `tools/Test-SolverCatalog.ps1`. Both the normal automation
preflight and the GitHub documentation workflow validate that the committed
JSON projection is synchronized with the runtime catalog.

This prevents an algorithm from being added or reclassified in code while the
documentation silently keeps stale metadata.

## Validation

The v0.26.0 test pack checks:

- 42 total public strategies;
- 23 exact / 19 heuristic;
- 17 direct exact / 4 formulations / 2 cutting planes;
- unique stable IDs;
- unique implementation types;
- successful default factory construction for every strategy;
- agreement between descriptor type and constructed type;
- agreement between descriptor `Kind` and solver `Kind`;
- six and only six external-solver-backed strategies;
- case-insensitive ID lookup;
- predictable unknown-ID behavior;
- `adaptive-exact` as the recommended exact entry point.

## Compatibility

No existing `IUlsSolver` signature changes.
No existing strategy class is removed or renamed.
Direct construction remains supported.
The catalog/factory is additive.
