# ULSAlgorithms v0.17.0 — Classical Mathematical Formulations

Adds:

1. Aggregate inventory-balance formulation.
2. Disaggregated / facility-location formulation.
3. Regeneration shortest-path formulation.
4. Inventory-eliminated formulation.

Also adds a solver-independent linear-model representation under
`Optimization/Modeling`.

## Scope

This release builds mathematical models only. It does not invoke a mathematical
optimizer. Solver execution will be added separately so formulation validation
remains independent of commercial solver availability.

## Apply

Extract at:

D:\Dev\UlsAlgorithm\ULSAlgorithms

Replace version.json when prompted.

Then:

1. Release → Rebuild Solution.
2. Run All Tests.
3. Optionally build documentation.
4. Do not commit until the complete suite is green.
