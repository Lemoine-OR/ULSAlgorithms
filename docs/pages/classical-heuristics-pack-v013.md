\page classical_heuristics_pack_v013 Classical Heuristics Pack II - v0.13.0

# Classical Heuristics Pack II - v0.13.0

Version 0.13.0 adds five distinct published heuristic strategies:

| Solver | Source | Main criterion |
|---|---|---|
| `FreelandColleySolver` | Freeland & Colley 1982 | local incremental holding vs setup |
| `PattersonLaForgeIncrementalPartPeriodSolver` | Patterson & LaForge 1985 | cumulative incremental holding vs setup |
| `WemmerlovModifiedPartPeriodBalancingSolver` | Wemmerlöv 1983 | corrected PPB, v=0.5 |
| `WemmerlovPpbLookAheadLookBackSolver` | Wemmerlöv 1983 | PPB + LALB |
| `WemmerlovModifiedPpbLookAheadLookBackSolver` | Wemmerlöv 1983 | corrected PPB + LALB |

All implement `IUlsSolver`, have `Kind == Heuristic`, and return
`UlsSolveStatus.Feasible`.

## Validation

The pack includes:

- deterministic tests separating Freeland-Colley from IPPA;
- a constant-demand test showing the v=0.5 correction changes the PPB cycle;
- an explicit Look-Ahead case;
- an explicit Look-Back case;
- zero-demand applicability tests;
- cancellation;
- 3,000 random positive stationary-cost instances.

For every random instance, each heuristic plan is independently checked for
material-balance feasibility and its cost is checked against the exact
`WagnerWhitinSolver` optimum.
