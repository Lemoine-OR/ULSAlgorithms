# ULSAlgorithms v1.1.0 — Reproducible computational study release

ULSAlgorithms v1.1.0 is the library release aligned with the reproducible
computational study of exact algorithms, mathematical programming formulations,
and constructive heuristics for the uncapacitated lot-sizing problem.

## Adaptive exact strategy provenance

`AdaptiveExactUlsSolver` now returns an `AdaptiveExactUlsSolveResult` that
preserves the identity of the exact algorithm selected at runtime.

The adaptive strategy continues to dispatch according to the structural
properties already cached by `UlsProblem`:

- Wagner–Whitin linear specialization when the no-speculative-motive
  condition holds;
- the configured general exact fallback otherwise;
- Wagelmans general is the default general fallback.

This provenance makes adaptive-selection decisions directly observable by
benchmarking and reproducibility infrastructure without rerunning an instance.

## Solver-backed numerical robustness

The mathematical-programming execution layer has been hardened for benchmark
and production use.

Changes include:

- improved CPLEX status interpretation;
- stricter normalization of solver-returned variable values;
- explicit propagation of solver execution information;
- fixed-integer polishing for solver-backed ULS formulations when required;
- independent validation of the resulting mathematical solution before an
  optimal result is accepted.

These changes are intended to distinguish numerical solver artifacts from
actual mathematical infeasibility or implementation errors.

## Scientific provenance

The audited scientific metadata associated with public ULS strategies has been
reviewed and corrected where necessary.

In particular, bibliographic metadata for the Bahl–Taj and
Chowdhury–Baki–Azab exact algorithms is aligned with the references documented
in the scientific reference catalogue.

The automated scientific-provenance baseline remains responsible for detecting
future accidental metadata regressions.

## Regression coverage

New regression tests cover:

- fixed-integer polishing;
- CPLEX status mapping;
- linear-variable numerical normalization;
- adaptive exact execution provenance.

The complete ULSAlgorithms test suite contains 288 passing tests for this
release.

## Reproducible computational study

This release is the ULSAlgorithms implementation used by the companion
ULSBenchmark experimental infrastructure for the reproducible computational
study:

*A Reproducible Computational Study of Exact Algorithms, Mathematical
Programming Formulations, and Heuristics for the Uncapacitated Lot-Sizing
Problem.*

The study evaluates the 42 public ULS strategies on 600 independently generated
benchmark instances.

The benchmark distinguishes direct exact algorithms, solver-backed
mathematical-programming formulations and cutting-plane approaches, and
constructive heuristics.
