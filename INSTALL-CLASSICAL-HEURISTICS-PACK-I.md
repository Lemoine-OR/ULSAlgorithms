# ULSAlgorithms 0.12.0 - Classical Heuristics Pack I

Apply this package after release v0.11.0.

Copy the package contents directly into the repository root and replace
`version.json` with the included 0.12.0 file.

Added public heuristic solvers:

- `LotForLotSolver`
- `SilverMealSolver`
- `LeastUnitCostSolver`
- `PartPeriodBalancingSolver`
- `GroffSolver`
- `PeriodicOrderQuantitySolver`

Important API behavior:

- all implement `IUlsSolver`;
- all have `Kind == UlsSolverKind.Heuristic`;
- successful heuristic solves return `UlsSolveStatus.Feasible`;
- no heuristic claims optimality.

Added internal support:

- `ClassicHeuristicGuard`
- `HeuristicSolutionBuilder`

Validation order:

1. Rebuild Release.
2. Run all tests.
3. Do not commit if any warning or error remains.
4. If a test fails, report the exact test name and full assertion/exception.
5. Commit + push only after the complete suite is green.
6. Wait for Build and Test + Build Documentation.
7. Publish v0.12.0 through Create Release.
