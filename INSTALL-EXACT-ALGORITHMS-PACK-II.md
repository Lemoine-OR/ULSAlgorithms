# ULSAlgorithms 0.14.0 - Exact Algorithms Pack II

Apply this package after release v0.13.0.

Copy the package contents directly into the repository root and replace
`version.json` with the included 0.14.0 file.

Added public exact solvers:

- `SaydamMcKnewFastWagnerWhitinSolver`
- `JacobsKhumawalaBranchAndBoundSolver`
- `ZangwillNetworkSolver`

Added internal support:

- `UlsRegenerationCost`

Added:

- cross-validation tests;
- one BenchmarkDotNet benchmark class;
- one Doxygen page per algorithm;
- pack overview documentation.

Validation order:

1. Rebuild Release.
2. Run all tests.
3. Do not commit if any warning or error remains.
4. If a test fails, report the exact test name and full assertion/exception.
5. Commit + push only after the complete suite is green.
6. Wait for Build and Test + Build Documentation.
7. Publish v0.14.0 through Create Release.
