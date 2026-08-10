\page validation_benchmarks Validation and Benchmarks

# Validation and Benchmarks

## Exactness is cross-checked, not assumed

New exact algorithms are validated against at least one mathematically independent reference whenever practical.

The test suite uses combinations of:

- deterministic literature-style instances;
- an independent quadratic Wagner–Whitin oracle;
- randomized general-cost campaigns;
- cross-validation against other exact methods;
- all-zero and zero-demand-period edge cases;
- cancellation;
- applicability rejection;
- feasibility and objective reconstruction.

This is particularly important for sophisticated data structures whose implementation can be asymptotically correct in design but still contain subtle indexing or dominance errors.

## Heuristic validation

Heuristic plans are checked for:

- no backlog;
- correct inventory balance;
- zero terminal inventory;
- finite objective components.

On random instances their objective cannot be lower than an independently computed exact optimum beyond numerical tolerance.

## BenchmarkDotNet

Performance suites are maintained under:

```text
benchmarks/ULSAlgorithms.Benchmarks/
```

Benchmarks compare algorithm families and scaling behavior rather than relying on isolated stopwatch timings.

When interpreting performance, consider:

- horizon length;
- setup/demand structure;
- applicability restrictions;
- allocation pressure;
- parallel scheduling overhead;
- warm-up and JIT effects.

## Reproducibility

Benchmark claims should always be associated with:

- the Git commit;
- build version;
- runtime;
- processor;
- benchmark parameters.

See @ref releases_reproducibility.
