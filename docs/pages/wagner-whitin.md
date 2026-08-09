\page wagner_whitin_algorithm Wagner-Whitin linear-time solver

# Wagner-Whitin linear-time solver

## Problem variant

`WagnerWhitinSolver` solves the deterministic single-item uncapacitated
lot-sizing problem with zero initial inventory, no backlogging, fixed setup
costs, linear production costs, and linear holding costs.

The public solver accepts the classical Wagner-Whitin case and the broader
**no-speculative-motive** class satisfying

\f[
p_t + h_t \ge p_{t+1}
\qquad t=0,\ldots,T-2.
\f]

Equivalently, the transformed marginal production costs are nonincreasing over
time.

## Exactness

The solver is **exact**. A successful call returns
`UlsSolveStatus.Optimal`.

## Complexity

- Time: **O(T)**
- Auxiliary working memory: **O(T)**
- Output memory: **O(T)**

Internal working arrays are rented from `ArrayPool<T>` so repeated use as a
subproblem avoids most temporary heap allocations.

## Algorithmic implementation

The implementation uses the backward dynamic-programming recurrence through
the geometric lower-convex-envelope interpretation of Wagelmans, van Hoesel
and Kolen. In the no-speculative-motive case, transformed marginal costs are
monotone, so both inserted hull slopes and query coordinates are monotone.
The lower envelope can therefore be maintained and queried with an
array-backed monotone hull in amortized constant time per period.

The C# code is an equivalent monotone-convex-hull implementation of the
linear-time specialization; it is not a line-by-line transcription of the
paper pseudocode.

For numerical stability, the recurrence evaluates the covered-demand
difference directly instead of subtracting two large line values whenever
possible. All cumulative values are checked for finite `double` results.

## Validation strategy

The test suite contains an independent `O(T^2)` forward dynamic-programming
oracle. One thousand deterministic random Wagner-Whitin instances are
cross-validated against the linear solver.

The published 12-period example from Wagelmans, van Hoesel and Kolen (1992)
is also reproduced. The expected production periods are 1, 3, 5, 8, 10 and
11, and the optimal original objective value is 864.

## References

1. H. M. Wagner and T. M. Whitin (1958).
   *Dynamic Version of the Economic Lot Size Model*.
   Management Science, 5(1), 89-96.
   DOI: https://doi.org/10.1287/mnsc.5.1.89

2. J. R. Evans (1985).
   *An Efficient Implementation of the Wagner-Whitin Algorithm for Dynamic
   Lot-Sizing*.
   Journal of Operations Management, 5(2), 229-235.
   DOI: https://doi.org/10.1016/0272-6963(85)90009-9

3. A. Wagelmans, S. van Hoesel and A. Kolen (1992).
   *Economic Lot Sizing: An O(n log n) Algorithm That Runs in Linear Time in
   the Wagner-Whitin Case*.
   Operations Research, 40(S1), S145-S156.
   DOI: https://doi.org/10.1287/opre.40.1.S145

## Why Evans (1985) is not the public implementation

Evans provides a compact and efficient implementation of the classical
Wagner-Whitin dynamic program, but its worst-case running time remains
quadratic. Wagelmans, van Hoesel and Kolen subsequently established a
linear-time algorithm for the Wagner-Whitin case. Because ULSAlgorithms is
intended for intensive reuse as a subproblem library, the public
`WagnerWhitinSolver` uses the later linear-time result.

The quadratic recurrence remains useful as an independent correctness oracle
inside the test project.
