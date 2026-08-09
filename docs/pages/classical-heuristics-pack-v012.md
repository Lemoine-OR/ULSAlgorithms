\page classical_heuristics_pack_v012 Classical Heuristics Pack I - v0.12.0

# Classical Heuristics Pack I - v0.12.0

Version 0.12.0 introduces six public heuristic strategies sharing `IUlsSolver`:

| Solver | Rule | Returned status |
|---|---|---|
| `LotForLotSolver` | one replenishment per positive-demand period | Feasible |
| `SilverMealSolver` | least cost per period | Feasible |
| `LeastUnitCostSolver` | least relevant cost per unit | Feasible |
| `PartPeriodBalancingSolver` | closest holding/setup balance | Feasible |
| `GroffSolver` | marginal setup/holding criterion | Feasible |
| `PeriodicOrderQuantitySolver` | EOQ-derived order interval | Feasible |

## Contract rule

Heuristics deliberately return `UlsSolveStatus.Feasible`, not `Optimal`.
The common strategy interface therefore remains mathematically meaningful when
exact and heuristic solvers are interchanged.

## Shared implementation

`HeuristicSolutionBuilder` independently reconstructs production, inventory,
setup decisions and all objective components and verifies:

- no backlog;
- zero terminal inventory;
- finite nonnegative costs.

Stationary-cost heuristics explicitly reject time-varying setup/production/
relevant holding costs instead of silently changing the classical rule.

## Validation campaign

The pack contains:

- deterministic method-specific examples;
- the published-style five-period Groff example;
- explicit EPP and POQ interval checks;
- all-zero demand;
- applicability rejection;
- cancellation;
- 2,500 random stationary-cost instances.

For every random instance each heuristic solution is independently checked for
material-balance feasibility and compared with `WagnerWhitinSolver`; no
heuristic is allowed to report a cost below the exact optimum.
