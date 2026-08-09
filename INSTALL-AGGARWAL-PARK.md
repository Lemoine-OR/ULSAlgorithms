# ULSAlgorithms 0.8.0 - Aggarwal-Park Monge matrix search

Apply this package after release v0.7.0.

Copy the package contents directly into the repository root and replace
`version.json` with the included 0.8.0 file.

Added public solver:

- `AggarwalParkSolver`

Added internal matrix-search engine:

- `AggarwalParkMatrixSearch`

Implementation characteristics:

- exact forward ELS dynamic program;
- Aggarwal-Park recursive divide-and-conquer decomposition;
- implicit Monge matrices;
- SMAWK row-minimum search;
- O(n log n) time;
- O(n) auxiliary memory;
- ArrayPool-backed workspaces;
- no quadratic cost matrix.

Validation order:

1. Rebuild Release.
2. Run all tests.
3. Do not commit if any warning or error remains.
4. If a test fails, report the exact test name and assertion/exception.
5. Commit + push only after the full suite is green.
6. Wait for Build and Test + Build Documentation.
7. Publish v0.8.0 through Create Release.

Scientific references are embedded in both source XML documentation and the
Doxygen page.
