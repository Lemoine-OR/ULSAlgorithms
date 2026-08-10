# ULSAlgorithms v0.16.0 — Concrete Solver Adapters

This overlay adds real machine discovery for:

1. IBM ILOG CPLEX
2. Gurobi Optimizer
3. FICO Xpress MP
4. COIN-OR CBC

The priority is identical to LotSizingDataModel.

## Important scope

This release validates solver presence, runtime loading, version and licensing
where applicable. It does not yet add a generic mathematical-model solve API.

## Apply

Extract at:

D:\Dev\UlsAlgorithm\ULSAlgorithms

Replace version.json when prompted.

Then:

1. Release → Rebuild Solution.
2. Run All Tests.
3. Optionally build documentation:
   powershell.exe -ExecutionPolicy Bypass -File ".\docs\build-documentation.ps1"
4. Do not commit until the full suite is green.

No CPLEX/Gurobi/Xpress/CBC SDK package is required to compile ULSAlgorithms.
