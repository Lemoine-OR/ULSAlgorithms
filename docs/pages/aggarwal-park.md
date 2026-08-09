\page aggarwal_park Aggarwal-Park recursive Monge matrix search

# Aggarwal-Park recursive Monge matrix search

## Public solver

`AggarwalParkSolver`

- Exact: **yes**
- General non-negative ULS costs: **yes**
- Time: **O(n log n)**
- Auxiliary memory: **O(n)**
- Direction: **forward**
- Main technique: **recursive matrix searching in implicit Monge matrices**

## Scientific origin

Aggarwal and Park introduced Monge-array methods as a general abstraction for
accelerating dynamic programs arising in uncapacitated economic lot-sizing
models.

Primary reference:

A. Aggarwal and J. K. Park (1993).

*Improved Algorithms for Economic Lot Size Problems.*

Operations Research, 41(3), 549-571.

DOI: https://doi.org/10.1287/opre.41.3.549

Their paper explicitly focuses on uncapacitated problems and shows how Monge
arrays can be exploited to obtain faster algorithms for several variants of
the Manne-Wagner-Whitin model.

A contemporary algorithmic comparison identifies the implementation used for
general ELS as the **recursive matrix searching algorithm developed by
Aggarwal and Park**:

S. van Hoesel, A. Wagelmans and B. Moerman (1994).

*Using Geometric Techniques to Improve Dynamic Programming Algorithms for the
Economic Lot-Sizing Problem and Extensions.*

European Journal of Operational Research, 75(2), 312-331.

DOI: https://doi.org/10.1016/0377-2217(94)90077-9

That paper also states that the Aggarwal-Park divide-and-conquer strategy
provides O(n log n) time for the general case when the relevant cost sequences
are not monotone.

## Forward transformed recurrence

Define cumulative demand

\f[
D_t=\sum_{k=0}^{t-1} d_k
\f]

and transformed variable production costs

\f[
r_j=p_j+\sum_{k=j}^{T-2} h_k.
\f]

The classical forward ELS recurrence becomes

\f[
F_t =
\min_{0\le j<t}
\left\{
F_j+f_j-r_jD_j+r_jD_t
\right\}.
\f]

For a fixed predecessor `j`, define

\f[
L_j(x)=F_j+f_j-r_jD_j+r_jx.
\f]

Then `F_t` is the minimum of the predecessor lines at `x=D_t`.

## Why a Monge matrix appears

Consider a rectangular block in which every predecessor `j` precedes every
target `t`. Order the predecessor columns by nonincreasing transformed cost
`r_j`; targets are already ordered by nondecreasing cumulative demand `D_t`.

The implicit matrix entry is

\f[
M_{t,j}=F_j+f_j-r_jD_j+r_jD_t.
\f]

For rows `t1 < t2` and columns with `r1 >= r2`,

\f[
M_{t1,1}+M_{t2,2}
\le
M_{t1,2}+M_{t2,1}.
\f]

Hence every cross-recursion block is a Monge matrix and its minimizing column
is monotone across rows.

## Divide-and-conquer implementation

The causal restriction `j < t` makes the complete matrix triangular. The
implementation recursively splits the dynamic-programming states by time:

1. solve the left half completely;
2. relax all right-half states using left-half predecessors;
3. solve the right half;
4. restore transformed-cost ordering for the enclosing recursion.

At each split, the left predecessor set is already ordered by transformed
marginal cost. The cross block is therefore searched as an implicit Monge
matrix.

This is a modern array-based realization of Aggarwal-Park's recursive
matrix-search principle. It intentionally does **not** call the Wagelmans
convex-hull implementation or the Federgruen-Tzur predecessor-tree
implementation.

## SMAWK

Row minima inside each rectangular Monge block are found with SMAWK.

Implementation details:

- matrix entries are evaluated on demand;
- no O(n²) matrix is materialized;
- rows are represented as arithmetic subsequences during SMAWK recursion;
- only reduced column indices are stored;
- equal minima retain the left-most matrix column;
- all large buffers use `ArrayPool<T>`.

A cross block with `r` rows and `c` columns therefore takes O(r+c) matrix
evaluations. Summed across the divide-and-conquer levels, this gives
O(n log n) total time.

## Memory

The implementation stores only:

- cumulative demand;
- transformed costs;
- dynamic-programming values;
- predecessor indices;
- finalized predecessor-line intercepts;
- two O(n) ordering buffers;
- row-minimum indices;
- O(n) SMAWK workspace.

No quadratic matrix is allocated.

## Zero-demand periods

The original expositions commonly assume strictly positive demand for
convenience. `ULSAlgorithms` supports zero-demand periods explicitly.

A zero-demand horizon extension can be taken without opening a setup. The
divide-and-conquer state finalization therefore includes a zero-cost
`F(t-1) -> F(t)` transition whenever `d[t-1] = 0`.

This also preserves the possibility of producing in a zero-demand period for
later positive demand when that is economically optimal.

## Validation

The v0.8.0 test campaign includes:

- the 12-period published Wagelmans example;
- strongly nonmonotone transformed production costs;
- explicit zero-demand production-period tests;
- all-zero demand;
- **3,000 random general instances** compared with:
  - the independent O(n²) oracle,
  - Wagelmans general,
  - Federgruen-Tzur general,
  - Evans 1985;
- **1,000 zero-demand-heavy random instances** compared with the independent
  quadratic oracle;
- **1,000 no-speculative-motive instances** compared with:
  - Wagelmans linear,
  - Federgruen-Tzur linear.

## Historical computational perspective

The 1994 van Hoesel-Wagelmans-Moerman experiments compared four general-cost
implementations: backward geometric, forward geometric, Aggarwal-Park matrix
search, and the quadratic Wagner-Whitin implementation.

Their reported Aggarwal-Park implementation was slower than the two geometric
O(n log n) algorithms, and they attributed this to the more complicated data
manipulations required by matrix searching. That historical result is one
reason ULSAlgorithms keeps Aggarwal-Park as a separate strategy and benchmarks
it rather than treating asymptotic complexity alone as a recommendation.

Modern contiguous arrays, pooled workspaces and implicit matrix evaluation may
change the constant-factor comparison, which is precisely what the benchmark
suite is intended to measure.
