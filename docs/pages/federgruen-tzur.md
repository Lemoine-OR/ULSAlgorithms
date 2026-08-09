\page federgruen_tzur Federgruen-Tzur forward O(n log n) solver

# Federgruen-Tzur forward O(n log n) solver

## Scope

`FedergruenTzurSolver` is a distinct exact implementation of the forward
algorithm introduced by Federgruen and Tzur (1991). It does not delegate to
the backward Wagelmans solver.

The solver handles the general non-negative-cost `UlsProblem` model, including
instances with speculative inventory motives.

## Dynamic-programming geometry

For period `i`, define cumulative demand `D(i)`, cumulative holding cost
`H(i)`, and

\f[
C(i)=c_i-H(i-1).
\f]

Federgruen and Tzur show that, for a fixed candidate last setup period `i`,
the cost as a function of a future cumulative demand is affine after removing
the common `S(t)` term:

\f[
F(i,t)=S(t)+B_i+C(i)D(t).
\f]

Therefore, the difference between any two candidates is a linear function of
`D(t)` and has at most one root. Federgruen and Tzur denote that cumulative
demand threshold by `G(k,l)`.

The Minimal Optimal Predecessor list is precisely the lower envelope of these
lines for cumulative demands at or beyond the current horizon.

## Data structure

The original paper explains that maintaining the ranked candidate list as a
simple array would require O(n) movement during insertions and deletions and
explicitly proposes a **balanced binary tree**, giving O(log n) access and
update operations.

ULSAlgorithms implements this recommendation with a custom array-backed AVL
tree:

- one node index per planning period;
- no managed candidate object allocation;
- pooled primitive arrays;
- AVL left/right/parent/height arrays;
- linked predecessor/successor arrays for the envelope order;
- local deletion of geometrically dominated candidates;
- removal of candidate intervals that lie entirely before the current
  cumulative demand.

The envelope thresholds stored by the implementation are the `G(k,l)` roots
of the paper.

## Complexity

For the general case:

- Time: **O(n log n)**
- Auxiliary memory: **O(n)**
- Exact: **yes**
- Direction: **forward**

Each period is inserted at most once and deleted at most once. Tree insertion
and deletion are logarithmic.

Federgruen and Tzur also derive two O(n) specializations:

1. nondecreasing setup costs;
2. no speculative motive for carrying inventory.

Those variants are intentionally being implemented as separate public solvers
rather than being hidden inside the general class, so their behavior and
performance can be benchmarked independently.

## Implementation notes

The implementation uses the affine formulation obtained directly from
equations (1d) and (2) of the paper. For the zero-based ULSAlgorithms API, the
candidate line has slope

\f[
C_i=p_i-H_{i-1}
\f]

and an intercept algebraically equivalent to Federgruen-Tzur's
`F(i,t)-S(t)` expression.

Zero-demand periods are supported. Extending a horizon across a zero-demand
period without ordering is represented as a zero-length dynamic-programming
arc and does not create a zero-quantity setup in the final solution.

## Validation

The test suite performs:

- a strongly nonmonotone/speculative-motive instance;
- zero-demand edge cases;
- the 12-period published Wagelmans example;
- **2,000 random general instances** cross-validated against:
  - an independent O(n²) forward oracle,
  - `WagnerWhitinClassicalSolver`,
  - `WagnerWhitinEvansSolver`,
  - `WagelmansGeneralSolver`;
- **1,000 Wagner-Whitin instances** cross-validated against the independent
  linear-time Wagelmans specialization.

BenchmarkDotNet compares Federgruen-Tzur directly with Evans and Wagelmans and
contains a separate scaling benchmark up to 100,000 periods.

## Primary reference

A. Federgruen and M. Tzur (1991).

*A Simple Forward Algorithm to Solve General Dynamic Lot Sizing Models with n
Periods in O(n log n) or O(n) Time.*

Management Science, 37(8), 909-925.

DOI: https://doi.org/10.1287/mnsc.37.8.909

The paper reports O(n log n) time and O(n) space for the general model. It
also reports that, in its numerical experiments, the Minimal Optimal
Predecessor list remained extremely small (never more than five elements for
the tested instances up to 5,000 periods), and that the new algorithm
outperformed Evans' 1985 Wagner-Whitin implementation.
