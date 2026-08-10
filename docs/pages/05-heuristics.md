\page heuristics Heuristics

# Heuristics

Heuristics use the same `IUlsSolver` interface as exact methods, but return
`UlsSolveStatus.Feasible`: they construct a valid plan without claiming an
optimality proof.

## Baselines and fixed-cycle rules

- `LotForLotSolver`
- `PeriodicOrderQuantitySolver`

## Average-cost rules

- `SilverMealSolver`
- `SegerstedtReformulatedSilverMealSolver`
- `LeastUnitCostSolver`
- `ChiuModifiedLeastUnitCostSolver`
- `HoChangSolisNetLeastPeriodCostSolver`
- `HoChangSolisImprovedNetLeastPeriodCostSolver`

The two Ho-Chang-Solis methods replace Silver-Meal's calendar-period average by
a net average over non-zero-demand periods. `nLPC(i)` adds the authors'
improved tie-breaking stopping condition.

## Part-period and EOQ-derived rules

- `PartPeriodSimplifiedSolver`
- `PartPeriodBalancingSolver`
- `ChiuTingModifiedPartPeriodBalancingSolver`
- `PattersonLaForgeIncrementalPartPeriodSolver`
- `WemmerlovModifiedPartPeriodBalancingSolver`
- `McLarenOrderMomentSolver`

McLaren's Order Moment combines an EOQ-derived time-between-orders estimate
with a part-period target and a final marginal setup/holding test.

## Look-Ahead / Look-Back

- `WemmerlovPpbLookAheadLookBackSolver`
- `WemmerlovModifiedPpbLookAheadLookBackSolver`

## Marginal-cost rules

- `GroffSolver`
- `FreelandColleySolver`

## Global merge rules

- `KarniMaximumPartPeriodGainSolver`

Karni's MPG is different from the forward heuristics above. It starts from a
Lot-for-Lot plan and repeatedly removes the globally most attractive
replenishment boundary according to the part-period gain criterion.

## Common use

All public heuristics use the same call pattern:

```csharp
IUlsSolver solver = new SilverMealSolver();
UlsSolveResult result = solver.Solve(problem);
```

For a heuristic, check `result.Status == UlsSolveStatus.Feasible`.

The stationary-cost heuristics reject incompatible time-varying cost structures
instead of silently changing the published rule.
