# ULSAlgorithms v0.22.0 — Literature Heuristics Pack III

Adds four public heuristic strategies:

- PartPeriodSimplifiedSolver
- SegerstedtReformulatedSilverMealSolver
- ChiuModifiedLeastUnitCostSolver
- ChiuTingModifiedPartPeriodBalancingSolver

Also corrects the documentation of PartPeriodBalancingSolver so PPS/LTC
(no EPP overshoot) and nearest-EPP PPB are not conflated.

Catalog after this pack:

- 22 exact strategies
- 15 heuristics
- 37 public IUlsSolver strategies

Each new algorithm has dedicated tests and BenchmarkDotNet coverage.

Exact algorithms whose detailed paper rules are not available are intentionally
deferred rather than reconstructed from abstracts.

## Apply

Extract into:

D:\Dev\UlsAlgorithm\ULSAlgorithms

Replace version.json when prompted.

Then:

1. Release → Rebuild Solution
2. Run All Tests
3. Do not commit yet
4. Report warnings/errors and test status
