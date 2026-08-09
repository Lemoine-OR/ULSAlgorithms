# ULSAlgorithms 0.7.0 - Federgruen-Tzur O(n) specializations

Apply this package after release v0.6.0.

Copy the package contents directly into the repository root and replace
`version.json` with the included 0.7.0 file.

Added public solvers:

- `FedergruenTzurNoSpeculativeMotiveSolver`
- `FedergruenTzurNondecreasingSetupSolver`

Added internal structure:

- `FedergruenTzurLinearCandidateDeque`

Both algorithms come from Federgruen & Tzur (1991), Sections 3 and 4, and are
implemented as distinct public strategies rather than redirects to the general
solver.

Validation order:

1. Rebuild Release.
2. Run all tests.
3. Do not commit if any warning or error remains.
4. Commit + push.
5. Wait for Build and Test + Build Documentation.
6. Publish v0.7.0 through Create Release.
