# ULSAlgorithms v0.24.0 — Automatic exact strategy selection

Base: v0.23.0 / commit 2b1f766b7867e8cc4b1b93b32476492fb312b8f0

## Purpose

This pack adds a lightweight exact-strategy selection layer without changing
`IUlsSolver` or any existing solver signature.

### New public API

- `UlsProblemCharacteristics`
- `UlsProblemAnalyzer`
- `UlsGeneralExactFallback`
- `AdaptiveExactUlsSolver`

### Selection rule

1. If the Wagner-Whitin / no-speculative-motive condition
   `p[t] + h[t] >= p[t+1]` holds for all adjacent periods, use
   `WagnerWhitinSolver` (Wagelmans et al. 1992 linear-time specialization).
2. Otherwise use a general exact O(n log n) algorithm.
3. Default general fallback: `WagelmansGeneralSolver`.
4. `FedergruenTzurSolver` is available as an explicit alternative for
   reproducible hardware/workload benchmarking.

No empirical crossover threshold is hard-coded in this release. The included
BenchmarkDotNet campaign measures selector overhead and is intended to support a
later evidence-based calibration if useful.

## Scientific references

- Wagelmans, A.; van Hoesel, S.; Kolen, A. (1992). Economic Lot Sizing: An
  O(n log n) Algorithm That Runs in Linear Time in the Wagner-Whitin Case.
  Operations Research, 40(S1), S145-S156. DOI: 10.1287/opre.40.1.S145.
- Federgruen, A.; Tzur, M. (1991). A Simple Forward Algorithm to Solve General
  Dynamic Lot Sizing Models with n Periods in O(n log n) or O(n) Time.
  Management Science, 37(8), 909-925. DOI: 10.1287/mnsc.37.8.909.

## Deliberately deferred

The historical exact procedures of Golany-Maman-Yadin (1992) and Aryanezhad
(1992) are not implemented in this pack because the accessible publisher
metadata/abstracts do not expose enough algorithmic detail to reconstruct their
procedures faithfully. They should only be added from the full primary papers.

## Apply

Extract this ZIP at the repository root and allow the files to merge into the
existing directory tree. Then rebuild the complete solution and run all tests.

Expected new files:

- `src/ULSAlgorithms/Selection/UlsProblemCharacteristics.cs`
- `src/ULSAlgorithms/Selection/UlsGeneralExactFallback.cs`
- `src/ULSAlgorithms/Selection/AdaptiveExactUlsSolver.cs`
- `tests/ULSAlgorithms.Tests/Selection/AdaptiveExactUlsSolverTests.cs`
- `benchmarks/ULSAlgorithms.Benchmarks/AdaptiveExactSolverBenchmarks.cs`

Replaced file:

- `version.json` -> 0.24.0
