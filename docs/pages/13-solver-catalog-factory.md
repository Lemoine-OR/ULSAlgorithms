\page solver_catalog_factory Solver Catalog and Factory

# Solver Catalog and Factory

v0.26.0 exposes the complete public strategy inventory as runtime metadata.
The catalog is intended for reusable subproblem libraries, experiment runners,
configuration-driven applications, consoles and graphical interfaces.

## Complete inventory

```csharp
using ULSAlgorithms.Catalog;

foreach (var strategy in UlsSolverCatalog.All)
{
    Console.WriteLine(
        $"{strategy.Id}: {strategy.Name} — {strategy.TimeComplexity}");
}
```

The current inventory is:

| View | Count |
|---|---:|
| `UlsSolverCatalog.All` | 42 |
| `UlsSolverCatalog.Exact` | 23 |
| `UlsSolverCatalog.DirectExact` | 17 |
| `UlsSolverCatalog.Formulations` | 4 |
| `UlsSolverCatalog.CuttingPlanes` | 2 |
| `UlsSolverCatalog.Heuristics` | 19 |

`Exact` includes the 17 direct exact algorithms, four solver-backed
formulations and two cutting-plane strategies.

## Stable identifiers

Each descriptor has a stable lower-kebab-case `Id`, for example:

```text
adaptive-exact
wagner-whitin-linear
wagelmans-general
aggregate-inventory-formulation
general-ls-cutting-plane
silver-meal
karni-maximum-part-period-gain
```

Identifiers are resolved case-insensitively by the runtime API, but the
canonical spelling is the lowercase form published by the catalog.

## Create a strategy by identifier

```csharp
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Catalog;

IUlsSolver solver =
    UlsSolverFactory.Create("wagelmans-general");

var result = solver.Solve(problem);
```

Unknown identifiers throw `KeyNotFoundException`.

For non-throwing configuration paths:

```csharp
if (UlsSolverFactory.TryCreate(configuredId, out var solver))
{
    var result = solver.Solve(problem);
}
```

Every call returns a fresh solver instance.

## Filter by operational category

```csharp
var localExact =
    UlsSolverCatalog.Exact
        .Where(strategy => !strategy.RequiresExternalSolver);

var solverBacked =
    UlsSolverCatalog.Exact
        .Where(strategy => strategy.RequiresExternalSolver);
```

`RequiresExternalSolver` is true only for the four mathematical formulations
and the two cutting-plane strategies. Their default constructors use the
library's normal automatic engine selection when `Solve` is eventually called.

## Descriptor metadata

Each `UlsSolverDescriptor` provides:

- `Id`
- `Name`
- `Kind`
- `Category`
- `Family`
- `TimeComplexity`
- `SpaceComplexity`
- `Applicability`
- `RequiresExternalSolver`
- `ScientificReference`
- `Doi`
- `Implementation`
- `SourcePath`
- `ImplementationType`

The metadata is descriptive. Applicability text does not replace the
strategy-specific runtime guards already implemented by individual solvers.

## Recommended automatic exact entry point

```csharp
var recommended =
    UlsSolverCatalog.RecommendedExact;

IUlsSolver solver =
    recommended.Create();
```

`RecommendedExact` resolves to `adaptive-exact`, i.e.
`AdaptiveExactUlsSolver`.

## One source of truth for code and documentation

`UlsSolverCatalog` is the canonical metadata inventory.

`docs/algorithm-catalog.json` is a generated projection consumed by the
documentation portal. CI verifies that it is byte-for-byte synchronized with
the runtime catalog.

After adding or changing catalog metadata, regenerate the projection with:

```powershell
dotnet run -c Release `
  --project .\tools\ULSAlgorithms.CatalogExporter\ULSAlgorithms.CatalogExporter.csproj `
  -- --write .\docs\algorithm-catalog.json
```

Validation can be run explicitly with:

```powershell
.\tools\Test-SolverCatalog.ps1
```

The normal Build/Test preflight and the documentation workflow both run the
same synchronization check.
