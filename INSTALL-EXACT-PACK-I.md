# ULSAlgorithms 0.11.0 - Exact Algorithms Pack I

Apply this package after release v0.10.0.

Copy the package contents directly into the repository root and replace
`version.json` with the included 0.11.0 file.

Added public solvers:

- `ChowdhuryBakiAzabSolver`
- `SadjadiAryanezhadSadeghiSolver`
- `LyuLeeParallelSolver`

Added:

- dedicated unit/cross-validation tests for all three methods;
- three BenchmarkDotNet benchmark classes;
- one Doxygen page per method;
- pack overview documentation.

Validation order:

1. Rebuild Release.
2. Run all tests.
3. Do not commit if any warning or error remains.
4. If a test fails, report the exact test name and full assertion/exception.
5. Commit + push only after the complete suite is green.
6. Wait for Build and Test + Build Documentation.
7. Publish v0.11.0 through Create Release.

Important provenance decision:

Evans-Saydam-McKnew (1989) and Golany-Maman-Yadin (1992) have not been
included in this pack because the accessible records did not provide enough
algorithmic detail for a faithful implementation. They remain on the roadmap.
