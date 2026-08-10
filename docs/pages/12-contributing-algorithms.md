\page contributing_algorithms Adding an Algorithm

# Adding an Algorithm

New methods should enter ULSAlgorithms as first-class, independently testable strategies.

## Required implementation contract

1. Implement `IUlsSolver`.
2. Use a stable, publication-identifying `Name`.
3. Set the correct `UlsSolverKind`.
4. Honor `CancellationToken`.
5. State applicability assumptions explicitly.
6. Avoid hidden asymptotic degradation in helper routines.

## Documentation contract

A research-derived algorithm should document:

- exact or heuristic status;
- supported ULS variant;
- time complexity;
- working-memory complexity;
- structural assumptions;
- original publication;
- DOI when available;
- mathematical recurrence or decision rule;
- implementation data structures;
- numerical considerations;
- whether the code is a faithful implementation or modern reconstruction.

## Validation contract

An exact method should normally include:

- deterministic examples;
- edge cases;
- randomized cross-validation;
- comparison with an independent exact oracle;
- cancellation;
- applicability rejection when restricted.

A heuristic should include:

- feasibility reconstruction;
- edge cases;
- comparison with an exact optimum on random instances;
- cancellation;
- applicability rejection when restricted.

## Benchmark contract

Add a BenchmarkDotNet case whenever the method introduces a new performance trade-off or asymptotic claim.

## Catalog registration

Finally, add the method to:

```text
docs/algorithm-catalog.json
```

The documentation build automatically regenerates the portal statistics and the algorithm catalog from this file.
