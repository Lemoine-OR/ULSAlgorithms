\page least_unit_cost Least Unit Cost

# Least Unit Cost

Public class: `LeastUnitCostSolver`.

LUC chooses a candidate cycle using relevant cost per unit:

\f[
\frac{A+\text{holding}(s,t)}
     {\sum_{k=s}^{t}d_k}.
\f]

The lot is extended until this quantity first increases.

LUC is one of the best-known classical dynamic lot-sizing heuristics and is
closely related to Silver-Meal; the distinction is the denominator: units
rather than calendar periods.

The implementation returns `Feasible`, never `Optimal`.
