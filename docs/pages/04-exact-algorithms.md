\page exact_algorithms Exact Algorithms

# Exact Algorithms

ULSAlgorithms groups exact methods by **algorithmic architecture**, not only by chronology.

## Wagner–Whitin dynamic-programming family

This family keeps the classical regeneration-interval recurrence visible.

Representative strategies include:

- `WagnerWhitinClassicalSolver`;
- `WagnerWhitinEvansSolver`;
- `SaydamMcKnewFastWagnerWhitinSolver`;
- `BahlTajPlanningHorizonSolver`;
- `HeadyZhuEconomicPartPeriodSolver`;
- `SadjadiAryanezhadSadeghiSolver`.

The implementations deliberately expose different memory/performance trade-offs.

## Geometric and data-structure accelerated DP

These methods reduce the cost of evaluating dynamic-programming candidates through convex-hull, tree, Monge or related structure.

Representative strategies:

- `WagnerWhitinSolver`;
- `WagelmansGeneralSolver`;
- `FedergruenTzurSolver`;
- `FedergruenTzurNoSpeculativeMotiveSolver`;
- `FedergruenTzurNondecreasingSetupSolver`;
- `AggarwalParkSolver`.

These are especially relevant when ULS is used repeatedly as a subproblem.

## Linear specialized methods

`ChowdhuryBakiAzabSolver` implements the published linear active-diagonal method under its supported stationary-cost assumptions.

Other O(T) implementations exploit specific cost structure and should never be selected without checking applicability.

## Network / shortest path

`ZangwillNetworkSolver` represents zero-inventory boundaries as nodes of an acyclic network and solves a backward shortest-path recurrence.

## Branch and bound

`JacobsKhumawalaBranchAndBoundSolver` exposes the branch/subproblem viewpoint and dominance logic of the historical simplified exact procedure.

## Parallel dynamic programming

`LyuLeeParallelSolver` evaluates predecessor candidates in parallel using a modern shared-memory reconstruction of the published architecture.

## Choosing among exact methods

Use @ref algorithm_selection for recommendations and @ref complexity_applicability for the full matrix.
