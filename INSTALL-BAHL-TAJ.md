# ULSAlgorithms 0.9.0 - Bahl-Taj planning-horizon solver

Apply this package after release v0.8.0.

Copy the package contents directly into the repository root and replace
`version.json` with the included 0.9.0 file.

Added public solver:

- `BahlTajPlanningHorizonSolver`

Algorithmic identity:

- Evans-style incremental low-storage recurrence;
- Wagner-Whitin Planning Horizon Theorem;
- data-dependent candidate pruning;
- explicit Wagner-Whitin / no-speculative-motive applicability check;
- O(n²) worst-case time;
- O(n) auxiliary memory.

Validation order:

1. Rebuild Release.
2. Run all tests.
3. Do not commit if any warning or error remains.
4. If a test fails, report the exact test name and full assertion/exception.
5. Commit + push only after the complete suite is green.
6. Wait for Build and Test + Build Documentation.
7. Publish v0.9.0 through Create Release.

Scientific references are embedded in source XML documentation and the
Doxygen page.
