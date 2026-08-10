# ULSAlgorithms v0.25.0 — Zero-extra-scan adaptive exact dispatch

Base: v0.24.0 / commit `b39fc64407d69fad95ca1d6101d3e50648735e3a`

## Purpose

v0.24.0 introduced `AdaptiveExactUlsSolver` and deliberately avoided an
empirical crossover threshold before measurements were available.

The calibration campaign showed two useful facts:

1. the adaptive selector added measurable overhead in the NSM case because
   v0.24.0 checked the Wagner-Whitin applicability condition before calling
   `WagnerWhitinSolver`, whose public `Solve` method checked the same condition
   again;
2. `WagelmansGeneralSolver` was faster than `FedergruenTzurSolver` at every
   measured horizon, so there was no evidence supporting a Wagelmans /
   Federgruen-Tzur crossover threshold on the measured workload.

v0.25.0 therefore removes the *extra* planning-horizon scan without weakening
the safety check performed by the direct Wagner-Whitin solver.

## Engineering change

`UlsProblem` is immutable after construction. The no-speculative-motive
condition

`p[t] + h[t] >= p[t+1]`

is now computed while the constructor already scans the copied input arrays.
The resulting internal boolean profile is reused by `AdaptiveExactUlsSolver`.

Consequences:

- adaptive dispatch itself is O(1);
- direct `WagnerWhitinSolver.Solve` keeps its own applicability check;
- no public solver signature changes;
- no additional allocation is introduced;
- the public strategy count remains 42;
- `WagelmansGeneralSolver` remains the default general fallback;
- `FedergruenTzurSolver` remains a public independent exact strategy and an
  explicit adaptive fallback for reproducible research.

`UlsProblemAnalyzer` also reuses the cached NSM value while retaining its one
linear pass for the other structural characteristics.

## Calibration evidence

The following values come from local BenchmarkDotNet campaigns used to
calibrate and validate v0.25.0. They are hardware/workload-specific engineering
evidence, not universal performance theorems.

### Before v0.25.0 — adaptive selector overhead

| Horizon | Scenario | Direct | Adaptive | Ratio |
|---:|---|---:|---:|---:|
| 100 | General | 3,491.8 ns | 3,405.4 ns | 0.98 |
| 100 | NSM | 2,619.5 ns | 2,831.6 ns | 1.08 |
| 1,000 | General | 39,176.9 ns | 40,890.1 ns | 1.04 |
| 1,000 | NSM | 25,970.2 ns | 27,497.6 ns | 1.06 |
| 10,000 | General | 484,393.4 ns | 489,604.7 ns | 1.01 |
| 10,000 | NSM | 362,536.9 ns | 381,941.5 ns | 1.05 |

The NSM overhead was consistent with one avoidable O(T) applicability scan.

### General exact solvers

| Horizon | Evans 1985 | Wagelmans general | Federgruen-Tzur | FT / Wagelmans |
|---:|---:|---:|---:|---:|
| 50 | 6.723 us | 1.866 us | 4.164 us | 2.23 |
| 100 | 24.840 us | 3.571 us | 7.389 us | 2.07 |
| 250 | 138.003 us | 8.685 us | 17.669 us | 2.03 |
| 500 | 556.442 us | 17.662 us | 35.309 us | 2.00 |
| 1,000 | 2,193.122 us | 37.904 us | 69.449 us | 1.83 |

No measured crossover supports changing the default general fallback.

## Post-change validation — v0.25.0

After caching the NSM condition in `UlsProblem`, the adaptive benchmark was
rerun with the same BenchmarkDotNet campaign.

### Adaptive exact dispatch after the optimization

| Horizon | Scenario | Direct | Adaptive | Ratio |
|---:|---|---:|---:|---:|
| 100 | General | 3,433.1 ns | 3,459.8 ns | 1.01 |
| 100 | NSM | 2,672.1 ns | 2,672.3 ns | 1.00 |
| 1,000 | General | 40,179.5 ns | 40,263.3 ns | 1.00 |
| 1,000 | NSM | 25,746.1 ns | 25,643.0 ns | 1.00 |
| 10,000 | General | 480,207.0 ns | 493,627.3 ns | 1.03 |
| 10,000 | NSM | 365,232.2 ns | 366,674.1 ns | 1.00 |

The NSM `AdaptiveExact / DirectExact` ratios moved from
`1.08 / 1.06 / 1.05` to `1.00 / 1.00 / 1.00` for horizons
`100 / 1,000 / 10,000`.

This validates the intended optimization: the adaptive selector no longer adds
a measurable planning-horizon scan in the NSM case.

The general-case ratios remain close to 1.00. The 10,000-period ratio of 1.03 is
within the measurement variability visible in the corresponding BenchmarkDotNet
error and standard-deviation values and does not justify introducing a new
selection threshold.

### Allocation behavior

`DirectExact` and `AdaptiveExact` retained identical managed allocations in the
validation campaign:

| Horizon | Direct | Adaptive |
|---:|---:|---:|
| 100 | 1,896 B | 1,896 B |
| 1,000 | 17,192 B | 17,192 B |
| 10,000 | 170,192 B | 170,192 B |

The optimization therefore removes selector scan overhead without adding
managed-memory cost.

### Analyzer effect

Because `UlsProblemAnalyzer` now reuses the cached NSM condition, its measured
cost also decreased:

| Horizon | Before | After |
|---:|---:|---:|
| 100 | ~320.5 ns | ~200.9 ns |
| 1,000 | ~3,053.9 ns | ~1,731.3 ns |
| 10,000 | ~30,535.7 ns | ~17,037.0 ns |

This is a secondary engineering benefit; the primary v0.25.0 objective remains
zero-extra-scan adaptive dispatch.

## Final selection policy

The measured policy retained by v0.25.0 is:

1. if the cached no-speculative-motive condition holds, use
   `WagnerWhitinSolver` in O(T);
2. otherwise use `WagelmansGeneralSolver` in O(T log T);
3. keep `FedergruenTzurSolver` available as an independent public exact
   strategy and explicit fallback for research and reproducibility;
4. do not introduce an empirical crossover threshold without new evidence.

## Scientific references

- A. Wagelmans, S. van Hoesel, A. Kolen (1992), “Economic Lot Sizing:
  An O(n log n) Algorithm That Runs in Linear Time in the Wagner-Whitin Case”,
  *Operations Research* 40(S1), S145-S156.
  DOI: `10.1287/opre.40.1.S145`.
- A. Federgruen, M. Tzur (1991), “A Simple Forward Algorithm to Solve General
  Dynamic Lot Sizing Models with n Periods in O(n log n) or O(n) Time”,
  *Management Science* 37(8), 909-925.
  DOI: `10.1287/mnsc.37.8.909`.

## Validation status

Before publication of v0.25.0:

- complete solution rebuild: passed locally;
- complete test suite: passed locally;
- adaptive benchmark: rerun after optimization;
- NSM adaptive/direct ratio: approximately 1.00 at all three measured horizons;
- managed allocations: unchanged between direct and adaptive execution.

The documentation build and link validation should be rerun after applying this
final note update and before committing the release.
