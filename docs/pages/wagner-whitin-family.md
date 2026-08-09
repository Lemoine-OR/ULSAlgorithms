\page wagner_whitin_family Wagner-Whitin implementation family

# Wagner-Whitin implementation family

ULSAlgorithms intentionally exposes multiple historically important exact
implementations instead of replacing older algorithms with the newest one.

## 1. WagnerWhitinClassicalSolver

This is the transparent classical shortest-path dynamic program.

- Exact: yes
- Time: O(n^2)
- Working memory: O(n^2)
- Implementation choice: complete triangular regeneration-cost matrix

Reference:

H. M. Wagner and T. M. Whitin (1958),
*Dynamic Version of the Economic Lot Size Model*,
Management Science 5(1), 89-96.
DOI: https://doi.org/10.1287/mnsc.5.1.89

## 2. WagnerWhitinEvansSolver

Evans recognized that the Wagner-Whitin recurrence can be evaluated without
materializing the complete regeneration-cost matrix. Candidate interval costs
are updated incrementally as the horizon is extended.

- Exact: yes
- Time: O(n^2)
- Auxiliary working memory: O(n)
- Implementation choice: incremental interval costs, array-backed state

Reference:

J. R. Evans (1985),
*An Efficient Implementation of the Wagner-Whitin Algorithm for Dynamic
Lot-Sizing*,
Journal of Operations Management 5(2), 229-235.
DOI: https://doi.org/10.1016/0272-6963(85)90009-9

Evans describes the algorithm as a shortest-path computation on an acyclic
network and emphasizes low core-storage requirements. The C# implementation
uses the same low-storage recurrence, adapted to contiguous arrays and
`ArrayPool<T>`.

## 3. WagnerWhitinSolver

This is the high-performance implementation introduced in ULSAlgorithms 0.3.0.

- Exact: yes
- Time: O(n) for the Wagner-Whitin/no-speculative-motive case
- Auxiliary working memory: O(n)
- Implementation choice: monotone lower convex envelope

Reference:

A. Wagelmans, S. van Hoesel and A. Kolen (1992),
*Economic Lot Sizing: An O(n log n) Algorithm That Runs in Linear Time in the
Wagner-Whitin Case*,
Operations Research 40(S1), S145-S156.
DOI: https://doi.org/10.1287/opre.40.1.S145

## Why all three are retained

The library has both scientific and computational goals. Keeping all three
implementations makes it possible to:

- reproduce historical algorithm comparisons;
- measure the cost of matrix storage versus Evans' low-storage recurrence;
- quantify the asymptotic advantage of the linear Wagelmans specialization;
- cross-validate independent exact implementations;
- teach the evolution of exact dynamic lot-sizing algorithms without reducing
  the public API to a single black-box solver.

## Validation

The test suite performs:

- the published 12-period Wagelmans example;
- 1,000 random general-cost instances comparing the classical and Evans
  implementations against an independent quadratic oracle;
- 500 random Wagner-Whitin instances comparing all three public exact solvers.

The benchmark project compares all three implementations on identical
Wagner-Whitin instances.
