\page zangwill_network Zangwill network algorithm

# Zangwill network algorithm

Public class:

`ZangwillNetworkSolver`

Reference:

W. I. Zangwill (1969),
*A Backlogging Model and a Multi-Echelon Model of a Dynamic Economic Lot Size
Production System—A Network Approach*,
Management Science 15(9), 506-527.

## Network representation

ULS zero-inventory boundaries become nodes `0..T`.

An arc

\f[
(i,j)
\f]

means: replenish in period `i` and satisfy all demand through period `j-1`.

The arc cost contains:

- setup cost in `i`;
- production cost of all units produced in `i`;
- holding cost required to carry future-period demand.

The no-backlogging single-echelon ULS model is therefore a shortest path in a
DAG.

## Implementation

`ZangwillNetworkSolver` computes the shortest path **backwards** from node `T`
to node `0`.

A shared cumulative-cost evaluator makes each arc O(1), so:

- time O(T²);
- auxiliary memory O(T).

This backward network organization is intentionally distinct from the forward
Wagner-Whitin implementations already present in the library.
