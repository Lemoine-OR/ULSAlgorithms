\page freeland_colley Freeland-Colley heuristic

# Freeland-Colley heuristic

Public class: `FreelandColleySolver`.

Reference:

J. R. Freeland and J. L. Colley Jr. (1982),
*A Simple Heuristic Method for Lot-Sizing in a Time-Phased Reorder System*,
Production and Inventory Management 23(1), 15-22.

## Rule

For a replenishment beginning in period `s`, demand in period `t>s` is included
while its **local incremental holding cost** satisfies

\f[
h(t-s)d_t \le A.
\f]

The important distinction from IPPA is that Freeland-Colley tests the next
period's local marginal carrying cost; it does not sum all preceding
part-period contributions in the stopping test.

## Complexity

The implementation advances monotonically through the horizon:

- time O(T);
- auxiliary working memory O(T);
- result status `Feasible`.
