\page heuristics Heuristics

# Heuristics

Heuristics share `IUlsSolver` with exact methods but return `UlsSolveStatus.Feasible`, never `Optimal`.

## Baselines and fixed-cycle rules

- `LotForLotSolver`
- `PeriodicOrderQuantitySolver`

These provide useful MRP-style reference policies.

## Average-cost rules

- `SilverMealSolver`
- `LeastUnitCostSolver`

Silver–Meal divides relevant cost by covered periods. LUC uses covered units.

## Part-period family

- `PartPeriodBalancingSolver`
- `PattersonLaForgeIncrementalPartPeriodSolver`
- `WemmerlovModifiedPartPeriodBalancingSolver`

The methods differ in how the setup/holding balance is evaluated; they are intentionally separate implementations.

## Marginal-cost rules

- `GroffSolver`
- `FreelandColleySolver`

These use local incremental holding/setup comparisons.

## Look-Ahead / Look-Back

- `WemmerlovPpbLookAheadLookBackSolver`
- `WemmerlovModifiedPpbLookAheadLookBackSolver`

These strategies apply Wemmerlöv's local LALB adjustment to PPB variants.

## Use as subproblem heuristics

Because all heuristics implement the same strategy interface and use contiguous ULS input arrays, they are suitable as fast incumbent generators or comparison methods inside larger optimization workflows.

Always check the cost assumptions in @ref complexity_applicability.
