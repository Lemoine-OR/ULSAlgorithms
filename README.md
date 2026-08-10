# ULSAlgorithms

High-performance C# / .NET algorithms for the deterministic, finite-horizon,
uncapacitated lot-sizing problem (ULS).

ULSAlgorithms provides a common `IUlsSolver` contract for direct exact
algorithms, mathematical formulations, cutting-plane methods and classical
heuristics. The implementation emphasizes computational efficiency,
literature-backed algorithms, reproducible validation and explicit scientific
provenance.

## Public strategy inventory

| Family | Count |
|---|---:|
| Direct exact algorithms | 17 |
| Mathematical formulations | 4 |
| `(l,S)` cutting-plane methods | 2 |
| Heuristics | 19 |
| **Total public strategies** | **42** |

The canonical inventory is available at runtime through `UlsSolverCatalog`.

## Requirements

- .NET 10 for the library and development toolchain.
- No external optimizer is required for direct exact algorithms or heuristics.
- Solver-backed formulations and cutting-plane methods can use CPLEX, Gurobi,
  Xpress or COIN-OR CBC through the portable optimization layer.

## Recommended exact entry point

For most client code, start with the stable factory ID `adaptive-exact`:

```csharp
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Catalog;
using ULSAlgorithms.Models;

var problem = new UlsProblem(
    demands:             [20.0, 30.0, 25.0, 40.0],
    setupCosts:          [200.0, 200.0, 200.0, 200.0],
    unitProductionCosts: [0.0, 0.0, 0.0, 0.0],
    holdingCosts:        [4.0, 4.0, 4.0, 0.0]);

IUlsSolver solver =
    UlsSolverFactory.Create("adaptive-exact");

var result = solver.Solve(problem);

Console.WriteLine(result.Status);
Console.WriteLine(result.ObjectiveValue);
```

The adaptive exact strategy selects the linear Wagner-Whitin specialization
when its no-speculative-motive condition applies and otherwise uses the
configured general exact fallback.

## Browse and create strategies

```csharp
foreach (var strategy in UlsSolverCatalog.All)
{
    Console.WriteLine(
        $"{strategy.Id} | {strategy.Name} | {strategy.TimeComplexity}");
}

IUlsSolver wagelmans =
    UlsSolverFactory.Create("wagelmans-general");

IUlsSolver silverMeal =
    UlsSolverFactory.Create("silver-meal");
```

Stable IDs are part of the public compatibility contract.

## Configure a strategy

```csharp
using ULSAlgorithms.Selection;

IUlsSolver solver =
    UlsSolverFactory.Create(
        "adaptive-exact",
        new UlsSolverCreationOptions
        {
            AdaptiveGeneralFallback =
                UlsGeneralExactFallback.FedergruenTzurGeneral
        });
```

The same mechanism exposes Lyu-Lee parallel settings, external optimization
execution options and cutting-plane engineering options. Unsupported options
are rejected rather than silently ignored.

## Reproducible JSON configuration

The library provides a versioned serializable configuration:

```csharp
var configuration =
    new UlsSolverConfiguration
    {
        SolverId = "adaptive-exact",
        Options = new UlsSolverCreationOptions
        {
            AdaptiveGeneralFallback =
                UlsGeneralExactFallback.WagelmansGeneral
        }
    };

configuration.SaveJson("solver-config.json");

var loaded =
    UlsSolverConfiguration.LoadJson("solver-config.json");

IUlsSolver solver =
    UlsSolverFactory.Create(loaded);
```

A typical JSON file is:

```json
{
  "schemaVersion": 1,
  "solverId": "adaptive-exact",
  "options": {
    "adaptiveGeneralFallback": "wagelmansGeneral"
  }
}
```

The schema is validated strictly: unknown schema versions, unknown solver IDs,
integer enum values and incompatible strategy options are rejected.

## External optimization engines

For solver-backed methods, automatic discovery uses the established priority:

```text
CPLEX -> Gurobi -> Xpress -> COIN-OR CBC
```

An explicit engine can be requested through `LinearModelSolveOptions`.

## Distribution

Each validated GitHub release contains:

- binary ZIP;
- documentation ZIP;
- NuGet package;
- NuGet portable-symbol package (`.snupkg`);
- build metadata;
- binary and release manifests;
- SHA-256 sidecars.

The `.nupkg` is validated twice: its archive/metadata structure is checked, then
an isolated temporary .NET 10 consumer restores the exact local package,
compiles against it and solves a deterministic ULS smoke instance.

The `.snupkg` provides the portable PDB for source-aware debugging. The project
uses the Source Link tooling included in modern .NET SDKs and publishes
repository metadata with the package.

## Validation and performance

The project uses:

- deterministic literature-style tests;
- an independent quadratic Wagner-Whitin oracle;
- randomized exact cross-validation;
- feasibility and objective reconstruction;
- edge-case and cancellation tests;
- BenchmarkDotNet performance campaigns;
- runtime/documentation catalog synchronization;
- the repository public-API compatibility baseline;
- official .NET package validation during `dotnet pack`;
- an isolated real NuGet consumer restore/build/run smoke;
- repository-wide Release builds and the complete test suite on both Windows
  and Linux in CI;
- an additional Linux portability smoke path;
- real COIN-OR CBC end-to-end qualification for all six solver-backed
  strategies;
- a Cobertura-compatible CI coverage artifact without an arbitrary pass threshold;
- reproducible versioned release manifests and SHA-256 checksums.

Benchmark results are evidence for the tested hardware, runtime and workload;
they are not treated as universal performance theorems.

## Documentation

The generated documentation portal contains:

- one panel and page per public strategy;
- descriptions and applicability conditions;
- complexity information;
- scientific references and DOI links;
- mathematical formulations where relevant;
- examples and API reference.

## Citation

Academic users can cite the software with the repository-level `CITATION.cff`.
The citation metadata is also included in the NuGet package.

## API stability

The compatibility policy is documented in
[`API-STABILITY.md`](API-STABILITY.md). Version 1.0.0 establishes the validated public API baseline as the stable
1.x compatibility contract.

## License

ULSAlgorithms is released under the MIT License. See [`LICENSE`](LICENSE).

## Author

David Lemoine — Lemoine-OR
