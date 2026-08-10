# ULSAlgorithms 0.15.0 — Optimization Infrastructure & Cut Traceability

This package adds the common architecture required by future solver-backed ULS
methods. It does not yet implement a MILP formulation or an (l,S) separator.

## Added public infrastructure

### Optimization solver selection

- SolverKind
- SolverCapability
- SolverAvailabilityStatus
- SolverAvailabilityInfo
- IOptimizationSolverAdapter
- SolverAdapterRegistry
- SolverSelectionOptions
- SolverSelectionResult
- SolverSelectionService
- SolverExecutionInfo

Default automatic solver order:

1. CPLEX
2. Gurobi
3. Xpress
4. COIN-OR CBC

Concrete solver adapters remain responsible for their real installation,
library-load and license checks.

### Cutting-plane traceability

- CutFamily
- CutSeparationMethod
- LinearConstraintSense
- CutDisposition
- CutCoefficient
- LsCutDefinition
- CutRecord
- CutIterationReport
- CutGenerationReport
- CuttingPlaneExecutionReport

Every generated cut can therefore be reported even when it is not added.

## Validation sequence

1. Extract at:
   D:\Dev\UlsAlgorithm\ULSAlgorithms
2. Replace version.json when prompted.
3. Release → Rebuild Solution.
4. Run All Tests.
5. Build documentation if desired:
   powershell.exe -ExecutionPolicy Bypass -File ".\docs\build-documentation.ps1"
6. Do not commit yet.
7. Report test count, warnings and errors.

No external solver package is required to compile this foundation package.
The tests use in-memory fake adapters.
