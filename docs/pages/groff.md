\page groff_heuristic Groff lot-sizing rule

# Groff lot-sizing rule

Public class: `GroffSolver`.

Reference:

G. K. Groff (1979),
*A Lot-Sizing Rule for Time-Phased Component Demand*,
Production and Inventory Management 20(1), 47-53.

For a cycle beginning at `t`, demand at offset `n` is included while

\f[
d_{t+n}\,n(n+1)\leq \frac{2A}{h}.
\f]

The rule is a marginal-cost approximation derived from the setup/holding
trade-off. It scans each generated cycle forward and returns a feasible,
non-optimal ULS solution.
