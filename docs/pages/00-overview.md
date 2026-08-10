\page overview Overview

# ULSAlgorithms overview

ULSAlgorithms is a research-oriented C#/.NET library dedicated to the deterministic **Uncapacitated Lot-Sizing problem (ULS)**.

Its design principle is simple: **scientifically distinct algorithms remain distinct public strategies**.

The library therefore does not hide the literature behind a single solver façade. Classical and accelerated Wagner–Whitin implementations, geometric dynamic programs, planning-horizon methods, network algorithms, branch-and-bound procedures, parallel methods and classical heuristics can all be selected explicitly through the same `IUlsSolver` interface.

## Design goals

1. **Scientific traceability** — algorithms retain the identity of the publication or classical rule they implement.
2. **Comparable APIs** — exact methods and heuristics share a Strategy-pattern contract.
3. **Performance** — data structures and asymptotic complexity matter because these solvers may be used as subroutines.
4. **Validation** — sophisticated exact methods are cross-checked against independent reference implementations.
5. **Reproducibility** — versions, commits, binaries, documentation and checksums are tied together by the release workflow.

## Documentation map

| Need | Read |
|---|---|
| Use the library now | @ref getting_started |
| Understand the mathematical model | @ref problem_and_notation |
| See every public algorithm | @ref algorithm_catalog |
| Choose a solver | @ref algorithm_selection |
| Compare complexity and assumptions | @ref complexity_applicability |
| Understand validation | @ref validation_benchmarks |
| Cite the literature | @ref scientific_references |
| Extend the library | @ref contributing_algorithms |

The public landing portal is available at [ULSAlgorithms documentation](../index.html).
