# v0.29.0 — CBC status normalization fix

The first real CBC end-to-end qualification exposed a provider-status parsing
defect that unit tests with fake executors could not detect.

CBC 2.10.13 produced a valid solution file whose first line was:

```text
Optimal - objective value 680.00000000
```

but the execution layer returned `Infeasible`. The previous status normalizer
searched the complete CBC console output for the generic word `infeasible`
before checking the authoritative optimal solution header. CBC may emit
intermediate presolve/simplex infeasibility diagnostics during a solve that
later terminates optimally.

The fix makes terminal solution-file status authoritative. When a valid
candidate solution exists, generic earlier console diagnostics cannot override
that candidate. Regression tests cover optimal, infeasible, feasible-on-limit
and unbounded cases.

No public API or solver ID changes are introduced.
