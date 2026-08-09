# ULSAlgorithms 0.13.0 - Classical Heuristics Pack II

Apply this package after release v0.12.0.

Copy the package contents directly into the repository root and replace
`version.json` with the included 0.13.0 file.

Added public heuristic solvers:

- `FreelandColleySolver`
- `PattersonLaForgeIncrementalPartPeriodSolver`
- `WemmerlovModifiedPartPeriodBalancingSolver`
- `WemmerlovPpbLookAheadLookBackSolver`
- `WemmerlovModifiedPpbLookAheadLookBackSolver`

Added internal support:

- `WemmerlovPpbCore`

No existing solver source file is replaced.

Validation order:

1. Rebuild Release.
2. Run all tests.
3. Do not commit if any warning or error remains.
4. If a test fails, report the exact test name and full assertion/exception.
5. Commit + push only after the complete suite is green.
6. Wait for Build and Test + Build Documentation.
7. Publish v0.13.0 through Create Release.
