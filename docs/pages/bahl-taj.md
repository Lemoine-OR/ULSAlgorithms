\page bahl_taj Bahl-Taj data-dependent Wagner-Whitin implementation

# Bahl-Taj data-dependent Wagner-Whitin implementation

## Public solver

`BahlTajPlanningHorizonSolver`

- Exact: **yes**
- Wagner-Whitin / no-speculative-motive costs: **required**
- Worst-case time: **O(n²)**
- Auxiliary working memory: **O(n)**
- Actual work: **data dependent**
- Base recurrence: **Evans 1985**
- Pruning principle: **Wagner-Whitin Planning Horizon Theorem**

## Scientific contribution

Bahl and Taj (1991) state that they modify Evans' efficient Wagner-Whitin code
by incorporating Wagner's setup/planning-horizon theorem.

Primary reference:

H. C. Bahl and S. Taj (1991).

*A data-dependent efficient implementation of the Wagner-Whitin algorithm for
lot-sizing.*

Computers & Industrial Engineering, 20(2), 289-291.

DOI: https://doi.org/10.1016/0360-8352(91)90033-3

Their published abstract reports that the modified code is faster by a factor
of `N/4` in their best empirical case, while being only approximately 1-2%
slower in their worst empirical case. This is a reported experimental result,
not an asymptotic complexity claim.


## Applicability

The library implementation deliberately applies Bahl-Taj pruning only when the
Wagner-Whitin / no-speculative-motive condition holds:

\f[
p_t+h_t \ge p_{t+1}.
\f]

This conservative applicability check is important. The planning-horizon
monotonicity used to discard earlier predecessor periods must not be applied
blindly to arbitrary speculative-cost instances, where a later horizon can
make an earlier production period attractive again.

`BahlTajPlanningHorizonSolver.IsApplicable(problem)` exposes the check, and
`Solve` throws `NotSupportedException` when it is violated.

## Relation to Evans

Evans (1985) evaluates the classical forward dynamic program without storing
the complete triangular cost matrix. Regeneration interval costs are updated
incrementally.

Reference:

J. R. Evans (1985).

*An Efficient Implementation of the Wagner-Whitin Algorithm for Dynamic
Lot-Sizing.*

Journal of Operations Management, 5(2), 229-235.

DOI: https://doi.org/10.1016/0272-6963(85)90009-9

`BahlTajPlanningHorizonSolver` retains this low-storage incremental structure.

## Planning Horizon Theorem

Wagner and Whitin's forward recursion computes the best last setup period for
successive prefixes of the planning horizon.

Their Planning Horizon Theorem states that if the optimum for a prefix ending
at period `t*` has its relevant setup at `t** <= t*`, then later prefix
optimizations need consider only setup periods at or after `t**`.

Reference:

H. M. Wagner and T. M. Whitin (1958).

*Dynamic Version of the Economic Lot Size Model.*

Management Science, 5(1), 89-96.

DOI: https://doi.org/10.1287/mnsc.5.1.89

Therefore the lower bound on candidate setup periods is monotone:

\f[
L_{t+1} \ge L_t .
\f]

If the optimal last setup moves forward frequently, old candidates disappear
quickly and the actual number of evaluations is far below the full triangular
`n(n+1)/2` scan.

## Zero-demand handling

A subtle implementation issue occurs when a period has zero demand.

For such a period,

\f[
F(t)=F(t-1)
\f]

may hold simply because no action is required. That zero-length transition is
not evidence that a setup was made in the zero-demand period. Advancing the
planning-horizon bound on that basis could incorrectly remove an earlier
period that remains optimal for later positive demand.

ULSAlgorithms therefore:

1. extends the prefix at zero cost when the new period has zero demand;
2. preserves the previous planning-horizon lower bound;
3. still introduces the zero-demand period as a candidate production period
   for future positive demand.

This behavior is explicitly regression-tested.

## Tie handling

When several **actual setup periods** attain the same optimal value for a
positive-demand prefix, the implementation keeps the latest minimizer.

It is itself an optimal predecessor, so the Planning Horizon Theorem permits
using it as the strongest available planning-horizon lower bound.

## Complexity

### Worst case

If the optimal last setup remains near the beginning of the horizon, little
or no pruning occurs:

\f[
1+2+\cdots+n = O(n^2).
\f]

### Favorable data

If a new late setup becomes optimal almost every period, the planning-horizon
lower bound advances with the horizon and only a small number of active
candidates survive.

The amount of work can then approach linear behavior.

This data dependence is the essential distinction between Bahl-Taj and Evans.

## Memory

The solver rents five O(n) work arrays:

- dynamic-programming values;
- predecessor indices;
- current regeneration-interval costs;
- current delivered unit costs;
- cumulative demands by active candidate.

No O(n²) matrix is allocated.

## Relationship to the other public solvers

| Solver | Main technique | Time |
|---|---|---:|
| `WagnerWhitinClassicalSolver` | explicit classical DP matrix | O(n²) |
| `WagnerWhitinEvansSolver` | low-storage incremental DP | O(n²) |
| `BahlTajPlanningHorizonSolver` | Evans + planning-horizon pruning | data-dependent, O(n²) worst case |
| `WagnerWhitinSolver` | Wagelmans convex-envelope WW specialization | O(n) |
| `WagelmansGeneralSolver` | backward geometric envelope | O(n log n) |
| `FedergruenTzurSolver` | forward balanced predecessor tree | O(n log n) |
| `AggarwalParkSolver` | recursive Monge matrix search | O(n log n) |

Bahl-Taj remains public even though later algorithms have better worst-case
complexity, because ULSAlgorithms preserves historically distinct algorithms
for reproducibility and modern benchmarking.

## Validation

The v0.9.0 campaign adds:

- the published 12-period Wagelmans test instance;
- explicit zero-demand planning-horizon regression cases;
- all-zero demand;
- explicit rejection of a speculative-cost instance;
- **5,000 random Wagner-Whitin instances** cross-validated against:
  - an independent O(n²) oracle,
  - Evans 1985,
  - the Wagelmans O(n) Wagner-Whitin solver;
- **1,000 frequent-setup Wagner-Whitin instances** compared with the
  Wagelmans O(n) solver;
- cancellation tests.

BenchmarkDotNet compares Bahl-Taj, Evans and the Wagelmans linear solver in
both favorable frequent-setup data and unfavorable long-cycle data.
