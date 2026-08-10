\page serializable_solver_configuration Serializable Solver Configuration

# Serializable Solver Configuration

ULSAlgorithms v0.28.0 adds a versioned JSON configuration format intended for
reproducible experiments, benchmark campaigns and applications that must select
a strategy without recompilation.

## Minimal example

```csharp
using ULSAlgorithms.Catalog;
using ULSAlgorithms.Selection;

var configuration =
    new UlsSolverConfiguration
    {
        SolverId = "adaptive-exact",
        Options =
            new UlsSolverCreationOptions
            {
                AdaptiveGeneralFallback =
                    UlsGeneralExactFallback.WagelmansGeneral
            }
    };

configuration.SaveJson("solver-config.json");

var loaded =
    UlsSolverConfiguration.LoadJson("solver-config.json");

var solver =
    UlsSolverFactory.Create(loaded);
```

The generated JSON is human-readable:

```json
{
  "schemaVersion": 1,
  "solverId": "adaptive-exact",
  "options": {
    "adaptiveGeneralFallback": "wagelmansGeneral"
  }
}
```

## Schema policy

`schemaVersion` is independent from the package version.

Version 1 is the first public configuration schema. Unknown versions are
rejected explicitly so that a configuration is never interpreted with guessed
semantics.

## Strict parsing

The reader rejects:

- unknown JSON properties;
- integer representations of enums;
- unknown stable solver IDs;
- option sets not supported by the selected solver;
- invalid nested optimization/cutting-plane options.

Enums are written as lower camel-case strings.

## Solver-backed example

```json
{
  "schemaVersion": 1,
  "solverId": "general-ls-cutting-plane",
  "options": {
    "optimizationExecution": {
      "solver": "coinOrCbc",
      "allowFallbackWhenExplicit": false,
      "feasibilityTolerance": 1e-8,
      "zeroTolerance": 1e-9,
      "integralityTolerance": 1e-6,
      "nearIntegerTolerance": 1e-9,
      "exportModelPath": "",
      "keepTemporaryFiles": false,
      "temporaryRootPath": ""
    },
    "cuttingPlane": {
      "maximumIterations": 20,
      "violationTolerance": 1e-7,
      "minimumEfficacy": 0.0,
      "selectionPolicy": "topByViolation",
      "maximumCutsPerIteration": 10
    }
  }
}
```

## Reproducibility recommendation

Archive the configuration next to the instance set and benchmark metadata:

```text
experiment/
  instances/
  solver-config.json
  benchmark-metadata.json
```

A published experiment should also record the ULSAlgorithms release tag and
build/commit metadata.
