\page solver_catalog_factory Solver Catalog and Factory

# Solver Catalog and Factory

v0.26.0 exposed the complete public strategy inventory as runtime metadata.
v0.27.0 extends the same catalog with strict constructor-level configuration.
The API is intended for reusable subproblem libraries, experiment runners,
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
| `UlsSolverCatalog.Configurable` | 8 |

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
- `ConfigurationCapabilities`
- `SupportsConfiguration`

The metadata is descriptive. Applicability text does not replace the
strategy-specific runtime guards already implemented by individual solvers.

## Configure construction in v0.27.0

The historical API remains unchanged:

```csharp
IUlsSolver solver =
    UlsSolverFactory.Create("wagelmans-general");
```

A second overload accepts `UlsSolverCreationOptions`:

```csharp
var options =
    new UlsSolverCreationOptions
    {
        AdaptiveGeneralFallback =
            UlsGeneralExactFallback.FedergruenTzurGeneral
    };

IUlsSolver solver =
    UlsSolverFactory.Create(
        "adaptive-exact",
        options);
```

Configuration is strict. A non-empty setting that does not belong to the
selected strategy throws instead of being silently ignored.

### Adaptive fallback

```csharp
var solver =
    UlsSolverFactory.Create(
        "adaptive-exact",
        new UlsSolverCreationOptions
        {
            AdaptiveGeneralFallback =
                UlsGeneralExactFallback.FedergruenTzurGeneral
        });
```

The default remains Wagelmans general. This option exists for reproducible
research and explicit policy control; v0.25.0 benchmark evidence still supports
Wagelmans as the default general fallback.

### Lyu-Lee parallel execution

```csharp
var solver =
    UlsSolverFactory.Create(
        "lyu-lee-parallel",
        new UlsSolverCreationOptions
        {
            MaxDegreeOfParallelism = 4,
            ParallelThreshold = 256
        });
```

Unspecified fields preserve the existing `LyuLeeParallelSolver` defaults.

### Choose an external optimization engine

The factory reuses `LinearModelSolveOptions`; it does not introduce a second
solver-configuration model.

```csharp
using ULSAlgorithms.Optimization;
using ULSAlgorithms.Optimization.Execution;

var solver =
    UlsSolverFactory.Create(
        "aggregate-inventory-formulation",
        new UlsSolverCreationOptions
        {
            OptimizationExecution =
                new LinearModelSolveOptions
                {
                    Solver = SolverKind.CoinOrCbc,
                    AllowFallbackWhenExplicit = false
                }
        });
```

`SolverKind.Automatic` retains the normal CPLEX → Gurobi → Xpress → CBC
priority. Explicit `Cplex`, `Gurobi`, `Xpress` and `CoinOrCbc` values are also
available.

The full `LinearModelSolveOptions` object remains available, including
feasibility/integrality tolerances, model export, temporary-file retention and
temporary-root settings.

### Configure cutting planes

Cutting-plane strategies accept both solver execution options and the existing
`LsCuttingPlaneOptions`:

```csharp
var solver =
    UlsSolverFactory.Create(
        "general-ls-cutting-plane",
        new UlsSolverCreationOptions
        {
            OptimizationExecution =
                new LinearModelSolveOptions
                {
                    Solver = SolverKind.Automatic
                },
            CuttingPlane =
                new LsCuttingPlaneOptions
                {
                    MaximumIterations = 20,
                    MaximumCutsPerIteration = 10
                }
        });
```

The existing cutting-plane object continues to control violation tolerance,
minimum efficacy and cut-selection policy as well.

### Discover configuration capabilities

```csharp
foreach (var strategy in UlsSolverCatalog.Configurable)
{
    Console.WriteLine(
        $"{strategy.Id}: {strategy.ConfigurationCapabilities}");
}
```

The current capability flags are:

- `AdaptiveGeneralFallback`
- `Parallelism`
- `OptimizationExecution`
- `CuttingPlane`

This metadata is also projected into `docs/algorithm-catalog.json`, allowing
configuration-driven UIs to expose only relevant controls.

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

## Persist a configured strategy

v0.28.0 adds `UlsSolverConfiguration`, a versioned JSON envelope around the
stable strategy ID and `UlsSolverCreationOptions`:

```csharp
var configuration =
    new UlsSolverConfiguration
    {
        SolverId = "lyu-lee-parallel",
        Options =
            new UlsSolverCreationOptions
            {
                MaxDegreeOfParallelism = 4,
                ParallelThreshold = 256
            }
    };

configuration.SaveJson("solver-config.json");

var solver =
    UlsSolverFactory.Create(
        UlsSolverConfiguration.LoadJson("solver-config.json"));
```

See @ref serializable_solver_configuration for schema, validation and
reproducibility guidance.

