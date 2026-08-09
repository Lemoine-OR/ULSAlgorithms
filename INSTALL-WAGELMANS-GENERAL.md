# ULSAlgorithms 0.5.0 - Wagelmans general O(n log n)

Apply this package after release v0.4.0.

Copy the package contents directly into the repository root and replace
`version.json` with the included 0.5.0 file.

Added:

- `WagelmansGeneralSolver`
- general-cost cross-validation tests
- comparative exact-solver benchmark
- large-horizon scaling benchmark
- scientific Doxygen page

The existing Wagner-Whitin classical, Evans, and linear Wagelmans solvers are
retained unchanged.

Local validation:

1. Rebuild Release.
2. Run all tests.
3. Do not commit if any warning or error remains.
4. Commit + push.
5. Wait for Build and Test + Build Documentation.
6. Publish v0.5.0 with Create Release.

Next planned exact implementation: Federgruen-Tzur (1991), as its own public
solver rather than as a replacement for Wagelmans.
