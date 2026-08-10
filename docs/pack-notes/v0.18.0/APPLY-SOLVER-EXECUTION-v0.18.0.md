# ULSAlgorithms v0.18.0 — Solver Execution Layer

Adds real execution of the portable `LinearModel` through:

1. CPLEX
2. Gurobi
3. Xpress
4. COIN-OR CBC

Automatic selection remains CPLEX → Gurobi → Xpress → CBC.

## Key additions

- LinearModelSolver
- LinearModelSolveOptions / Result / Status
- ILinearModelSolverExecutor
- four provider executors
- PortableLpModelWriter
- Gurobi/CBC text solution parser
- CPLEX XML solution parser
- independent LinearModelSolutionValidator

## Apply

Extract into:

D:\Dev\UlsAlgorithm\ULSAlgorithms

Replace version.json when prompted.

Then:

1. Release → Rebuild Solution
2. Run All Tests
3. Do not commit yet
4. Report test count plus any warnings/errors

The normal unit-test suite does not require an installed commercial solver.
