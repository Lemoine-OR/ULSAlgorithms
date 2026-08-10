# v0.29.0 — CBC end-to-end qualification

This first v0.29 qualification pack adds a real, license-free solver-backed
integration gate before the 1.0 API is frozen.

## What is exercised

The Linux qualification installs the Ubuntu `coinor-cbc` package and executes:

- `aggregate-inventory-formulation`
- `facility-location-formulation`
- `shortest-path-formulation`
- `inventory-eliminated-formulation`
- `general-ls-cutting-plane`
- `wagner-whitin-ls-cutting-plane`

CBC is requested explicitly with fallback disabled.

Every result must:

- return `UlsSolveStatus.Optimal`;
- contain a reconstructed `UlsSolution`;
- return a finite objective;
- agree with `adaptive-exact`;
- agree with the deterministic known objective `680`;
- record COIN-OR CBC as the selected execution engine.

The release workflow repeats the same qualification before publishing.

## Scope

This pack intentionally adds no new optimization algorithm and changes no
public solver ID or public API contract. It is release qualification only.
