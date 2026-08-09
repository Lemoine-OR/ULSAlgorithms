\page jacobs_khumawala Jacobs-Khumawala simplified optimal lot sizing

# Jacobs-Khumawala simplified optimal lot sizing

Public class:

`JacobsKhumawalaBranchAndBoundSolver`

Reference:

F. R. Jacobs and B. M. Khumawala (1987),
*A Simplified Procedure for Optimal Single-Level Lot Sizing*,
Production and Inventory Management 28(3), 39-43.

## Publication identity

The published abstract describes:

- a simple branch-and-bound procedure;
- exact single-item, single-level lot sizing;
- computational equivalence to Wagner-Whitin;
- a graphical representation intended to make the method easier to apply;
- decomposition into subproblems;
- elimination of subproblems that cannot lead to an optimum.

## C# reconstruction

A node is a zero-inventory boundary.

A branch from boundary `i` to boundary `j` corresponds to one replenishment in
period `i` satisfying demand through `j-1`.

For each boundary the algorithm stores only its cheapest partial-plan label.
Any more expensive branch reaching the same boundary is dominated because the
remaining subproblem is identical.

An explicit Lot-for-Lot solution provides an initial upper bound. Branches
whose accumulated cost already exceeds that incumbent are fathomed.

Because boundary subproblems are processed in topological order, labels are
final when expanded.

## Complexity

- O(T²) branch evaluations;
- O(T) auxiliary memory;
- exact result.

This class is a modern programmatic reconstruction of the publication's
branch/subproblem representation. It does not claim to reproduce the original
paper's graphical worksheet line by line.
