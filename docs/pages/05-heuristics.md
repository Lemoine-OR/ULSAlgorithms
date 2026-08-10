\page heuristics Heuristics

# Heuristics

Heuristics implement the same `IUlsSolver` interface as exact methods but return `UlsSolveStatus.Feasible`, never `Optimal`.

## Baseline and periodic rules

- `LotForLotSolver`
- `PeriodicOrderQuantitySolver`

## Average-cost rules

- `SilverMealSolver`
- `SegerstedtReformulatedSilverMealSolver`
- `LeastUnitCostSolver`
- `ChiuModifiedLeastUnitCostSolver`

## Part-period rules

- `PartPeriodSimplifiedSolver`
- `PartPeriodBalancingSolver`
- `ChiuTingModifiedPartPeriodBalancingSolver`
- `PattersonLaForgeIncrementalPartPeriodSolver`
- `WemmerlovModifiedPartPeriodBalancingSolver`
- `WemmerlovPpbLookAheadLookBackSolver`
- `WemmerlovModifiedPpbLookAheadLookBackSolver`

## Marginal-cost rules

- `GroffSolver`
- `FreelandColleySolver`

Each method has its own card and dedicated documentation page in the portal. Those pages use one common structure: description, specifications, operation, minimal code, scientific source and API link.
