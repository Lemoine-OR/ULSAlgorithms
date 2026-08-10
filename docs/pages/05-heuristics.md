\page heuristics Heuristics

# Heuristics

Heuristics share `IUlsSolver` with exact methods but return
`UlsSolveStatus.Feasible`, never `Optimal`.

## Baselines and fixed-cycle rules

- `LotForLotSolver`
- `PeriodicOrderQuantitySolver`

## Average-cost rules

- `SilverMealSolver`
- `SegerstedtReformulatedSilverMealSolver`
- `LeastUnitCostSolver`
- `ChiuModifiedLeastUnitCostSolver`

The Segerstedt reformulation keeps Silver-Meal's elapsed-calendar denominator
but evaluates only non-zero demand events as extension candidates.

The Chiu LUC variant adds a final cost-beneficial last-lot merge test.

## Part-period family

- `PartPeriodSimplifiedSolver`
- `PartPeriodBalancingSolver`
- `ChiuTingModifiedPartPeriodBalancingSolver`
- `PattersonLaForgeIncrementalPartPeriodSolver`
- `WemmerlovModifiedPartPeriodBalancingSolver`

`PartPeriodSimplifiedSolver` stops at the largest accumulation not exceeding the
EPP. `PartPeriodBalancingSolver` instead selects the closest side of the EPP.
They are intentionally separate public implementations.

## Marginal-cost rules

- `GroffSolver`
- `FreelandColleySolver`

## Look-Ahead / Look-Back

- `WemmerlovPpbLookAheadLookBackSolver`
- `WemmerlovModifiedPpbLookAheadLookBackSolver`

## Use as subproblem heuristics

All heuristics implement the same strategy interface and use contiguous ULS
input arrays, making them suitable as fast incumbent generators or comparison
methods inside larger optimization workflows.

Always check the cost assumptions in @ref complexity_applicability.

See also @ref literature_heuristics_v022.
