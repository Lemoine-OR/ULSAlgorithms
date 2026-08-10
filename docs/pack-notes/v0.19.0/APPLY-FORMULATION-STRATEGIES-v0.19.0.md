# ULSAlgorithms v0.19.0 — Solver-Backed Formulation Strategies

Adds four exact IUlsSolver strategies:

- AggregateInventoryFormulationSolver
- FacilityLocationFormulationSolver
- ShortestPathFormulationSolver
- InventoryEliminatedFormulationSolver

Also adds:

- IAsyncUlsSolver;
- SolverBackedUlsSolveResult;
- public UlsSolutionValidator;
- formulation-to-UlsSolution reconstruction;
- objective agreement check;
- solver/formulation provenance in the returned result.

No external solver is required to compile or run the new unit tests.

## Apply

Extract into:

D:\Dev\UlsAlgorithm\ULSAlgorithms

Replace version.json when prompted.

Then:

1. Release → Rebuild Solution
2. Run All Tests
3. Do not commit yet
4. Report warnings/errors and the test result
