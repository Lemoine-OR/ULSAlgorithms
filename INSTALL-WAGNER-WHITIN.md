# ULSAlgorithms 0.3.0 - Wagner-Whitin package

This package adds the first exact ULS algorithm.

## Public implementation

`WagnerWhitinSolver` uses the linear-time Wagner-Whitin specialization of:

A. Wagelmans, S. van Hoesel, A. Kolen (1992),
"Economic Lot Sizing: An O(n log n) Algorithm That Runs in Linear Time in the
Wagner-Whitin Case", Operations Research 40(S1), S145-S156.
DOI: 10.1287/opre.40.1.S145.

The quadratic Evans/Wagner-Whitin recurrence is deliberately not exposed as
the production solver. An independent O(n^2) forward DP exists only in the
test assembly and is used as a correctness oracle.

## Installation

Before applying this package, publish/validate v0.2.0 if that release has not
yet been completed.

Then copy the bundle contents directly into the repository root. Replace
`version.json` with the included 0.3.0 version.

## Validation order

1. Rebuild the solution in Release.
2. Run all tests.
3. Run the `WagnerWhitinBenchmarks` benchmark manually if desired.
4. Commit and push only after local validation.
5. Wait for Build and Test + Build Documentation to be green.
6. Run Create Release for v0.3.0.

No general O(n log n) ELS solver and no heuristic is included in this package.
