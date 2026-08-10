\page literature_heuristics_v022 Literature Heuristics Pack III

# Literature Heuristics Pack III — v0.22.0

v0.22.0 adds four distinct public heuristic strategies and corrects an
important terminology conflation between Part-Period Simplified and
Part-Period Balancing.

## 1. Part-Period Simplified

Class:

```text
PartPeriodSimplifiedSolver
```

The Economic Part Period is

\f[
EPP=\frac{A}{h}.
\f]

Starting in period \f$i\f$, PPS selects the largest \f$j\f$ satisfying

\f[
\sum_{k=i}^{j}(k-i)d_k \le EPP.
\f]

It therefore stops before the first overshoot.

This is deliberately different from `PartPeriodBalancingSolver`, which compares
the points on both sides of the EPP and chooses the nearest one.

References:

- J. J. DeMatteis, *An Economic Lot-Sizing Technique I: The Part-Period
  Algorithm*, IBM Systems Journal 7(1), 30-38, 1968.
- L. Baciarello, M. D'Avino, R. Onori, M. M. Schiraldi,
  *Lot Sizing Heuristics Performance*, 2013,
  DOI `10.5772/56004`.

Complexity:

```text
time   O(T)
space  O(T) output-cycle buffer
```

## 2. Reformulated Silver-Meal

Class:

```text
SegerstedtReformulatedSilverMealSolver
```

Let non-zero demand events \f$\widehat X_i\f$ occur in actual calendar periods
\f$t_i\f$, with the current lot beginning at \f$t_0\f$.

The reformulated average period cost is

\f[
C_n=
\frac{
A+h\sum_{i=0}^{n}(t_i-t_0)\widehat X_i
}{
t_n-t_0+1
}.
\f]

Only non-zero demand events are candidate extension points. The lot stops at the
first increase in this quantity.

This preserves elapsed calendar time in the denominator while avoiding the
classical Silver-Meal distortion in which zero-demand periods can artificially
lower intermediate average costs.

Reference:

A. Segerstedt, B. Abdul-Jalbar, B. Samuelsson,
*Reformulated Silver-Meal and Similar Lot Sizing Techniques*,
Axioms 12(7), 661, 2023,
DOI `10.3390/axioms12070661`.

Complexity:

```text
time   O(T)
space  O(T) output-cycle buffer
```

## 3. Chiu modified Least Unit Cost

Class:

```text
ChiuModifiedLeastUnitCostSolver
```

The ordinary LUC policy is constructed first.

If the last two replenishment periods are \f$p\f$ and \f$q\f$ and the complete
last lot quantity is \f$Q_q\f$, merging the final lot into the preceding one
removes one setup but increases holding by

\f[
h(q-p)Q_q.
\f]

The merge is accepted only when that additional holding cost is strictly less
than the setup cost saved.

Reference:

Y. P. Chiu,
*A modification of the least unit cost lot-sizing heuristic*,
Journal of Statistics and Management Systems 7(1), 197-207, 2004,
DOI `10.1080/09720510.2004.10701115`.

Complexity:

```text
time   O(T)
space  O(T) output-cycle buffer
```

## 4. Chiu–Ting modified Part-Period Balancing

Class:

```text
ChiuTingModifiedPartPeriodBalancingSolver
```

This method first generates the standard nearest-EPP PPB plan. It then applies
the same final-lot cost-benefit test to determine whether the last order should
be eliminated and merged into the preceding replenishment.

Reference:

S. W. Chiu, C.-K. Ting, Y. P. Chiu,
*A modified version of the part period lot-sizing heuristic*,
International Journal for Engineering Modelling 18(1-2), 59-64, 2005.

Complexity:

```text
time   O(T)
space  O(T) output-cycle buffer
```

## Validation

All four methods:

- implement `IUlsSolver`;
- return `UlsSolveStatus.Feasible`, never `Optimal`;
- use the same stationary-cost applicability guard as the corresponding
  classical rules;
- are cross-checked against an exact Wagner-Whitin solver on randomized
  stationary-cost instances;
- have dedicated BenchmarkDotNet comparisons against their closest classical
  counterpart.

## Deferred exact literature methods

This release deliberately does **not** add a class under the name of an exact
paper when the published algorithmic rules have not yet been recovered in
sufficient detail.

In particular, Golany–Maman–Yadin planning-horizon decomposition and the
Aryanezhad optimality-condition algorithm remain research targets rather than
being reconstructed from abstracts.
