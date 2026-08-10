\page exact_algorithms Exact Algorithms

# Exact Algorithms

An exact strategy returns an optimal ULS solution when its documented applicability conditions are satisfied.

The direct exact methods are grouped by mechanism rather than by release:

## Dynamic programming and geometric acceleration

Wagner–Whitin, Evans, Wagelmans, Federgruen–Tzur, Aggarwal–Park and related linear or O(T log T) variants.

## Planning-horizon methods

Bahl–Taj, Heady–Zhu and Sadjadi–Aryanezhad–Sadeghi use data-dependent pruning or horizon reduction while preserving exactness under their stated assumptions.

## Parallel methods

`LyuLeeParallelSolver` exposes the parallel literature line as a separate public implementation.

## Network and combinatorial methods

`ZangwillNetworkSolver` uses a shortest-path interpretation. `JacobsKhumawalaBranchAndBoundSolver` represents the branch-and-bound literature line.

## Solver-backed exact strategies

Mathematical formulations and `(l,S)` cutting-plane solvers are exact too, but they are shown separately in the user documentation because they depend on an optimization engine.

For a method-by-method comparison, open @ref algorithm_catalog or use the card-based documentation portal.
