\page lot_for_lot Lot-for-Lot

# Lot-for-Lot

Public class: `LotForLotSolver`.

The rule replenishes each positive demand exactly in its demand period.

- family: heuristic / MRP baseline;
- status returned: `Feasible`;
- time: O(T);
- working memory: O(T);
- applicable to general time-varying `UlsProblem` costs.

It is intentionally retained as a baseline because many lot-sizing comparisons
use L4L/LFL as the zero-holding-inventory extreme.
