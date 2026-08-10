# ULSAlgorithms v0.23.0 — Literature Heuristics

Adds four public heuristic strategies:

- HoChangSolisNetLeastPeriodCostSolver
- HoChangSolisImprovedNetLeastPeriodCostSolver
- McLarenOrderMomentSolver
- KarniMaximumPartPeriodGainSolver

Scientific sources:

- Ho, Chang & Solis (2006), Journal of the Operational Research Society
  57(8), 1005-1013, DOI 10.1057/palgrave.jors.2602076.
- McLaren (1977), Purdue University Ph.D. dissertation; operational
  reconstruction documented by Baciarello et al. (2013),
  DOI 10.5772/56004.
- Karni (1981), Production and Inventory Management 22(2), 91-98;
  reconstruction documented by Baciarello et al. (2013),
  DOI 10.5772/56004.

Engineering choices:

- nLPC and nLPC(i): published stopping rules evaluated incrementally in O(T).
- MOM: O(T) forward implementation of the Order Moment Target rule.
- MPG: O(T log T) priority-queue implementation of the global merge rule.

Catalog after the pack:

- 22 exact strategies
- 19 heuristics
- 41 public IUlsSolver strategies

The documentation portal will generate one dedicated page per new algorithm
from docs/algorithm-catalog.json.

Apply to:

D:\Dev\UlsAlgorithm\ULSAlgorithms

Then:

1. Release -> Rebuild Solution
2. Run All Tests
3. Build the documentation if desired
4. Do not commit yet
5. Report errors/warnings/test status
