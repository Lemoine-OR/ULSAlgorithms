# ULSAlgorithms v0.18.0 — Compile + numerical normalization fix

This patch fixes:

1. missing parent-namespace visibility for SolverKind in the new execution layer;
2. internal CPLEX solution-parser access from the test assembly via
   InternalsVisibleTo("ULSAlgorithms.Tests");
3. numerical normalization of solver-returned values before validation,
   objective reconstruction and later ULS mapping.

The numerical policy intentionally matches LotSizingDataModel:

- zero tolerance: 1e-8;
- integrality tolerance: 1e-7;
- continuous near-integer tolerance: 1e-8;
- small residuals are cleaned;
- materially fractional / materially negative values are not silently repaired.

No version change is made: this remains v0.18.0.
