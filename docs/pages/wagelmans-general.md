\page wagelmans_general Wagelmans general O(n log n) solver

# Wagelmans general O(n log n) solver

## Scope

`WagelmansGeneralSolver` is an exact solver for the general single-item
uncapacitated economic lot-sizing problem represented by `UlsProblem`.

Unlike the linear `WagnerWhitinSolver`, it does **not** require the
no-speculative-motive / Wagner-Whitin cost condition.

The current public model accepts finite non-negative demands and costs. The
original 1992 paper treats a more general signed-cost formulation.

## Complexity

- Time: **O(n log n)**
- Auxiliary working memory: **O(n)**
- Result memory: **O(n)**

## Transformation

Let `D[k]` denote cumulative demand through the first `k` periods. Holding
costs are eliminated by defining a transformed marginal production cost

\f[
r_t = p_t + \sum_{j=t}^{T-2} h_j .
\f]

The final holding-cost coefficient is irrelevant because terminal inventory is
zero. Omitting it is a common additive shift and does not alter the minimizing
production periods.

The backward dynamic program can then be written in the equivalent form

\f[
B_t =
f_t - r_t D_t
+
\min_{s>t}
\left\{
B_s + r_t D_s
\right\}.
\f]

Each continuation state `s` is therefore a line in the query variable `r_t`:

\f[
L_s(x) = B_s + D_s x.
\f]

Cumulative-demand slopes are monotone when periods are processed backwards.
The lower envelope is consequently updated with a stack in amortized O(1)
time per inserted state. Since general `r_t` values need not be monotone, the
active segment is found by binary search in O(log n) time per period.

## Implementation choices

The implementation follows the **backward** geometric algorithm. This is a
deliberate choice: van Hoesel, Wagelmans and Moerman (1994) explain that the
backward recursion requires only a stack to maintain the envelope, whereas
the forward general-cost implementation requires a balanced search tree.
Their computational experiments also identify the backward algorithm as
particularly effective.

Implementation details in ULSAlgorithms:

- contiguous arrays only in the hot path;
- `ArrayPool<T>` for temporary O(n) buffers;
- no LINQ inside the algorithm;
- binary search over envelope activation abscissae;
- explicit handling of zero-demand periods;
- finite-double checks on cumulative and transformed quantities;
- direct reconstruction of the original production and inventory plan.

## Relationship to the existing solvers

`WagelmansGeneralSolver` is **not** a replacement for any existing class.

| Solver | Published basis | Time | Working memory | General costs |
|---|---|---:|---:|---|
| `WagnerWhitinClassicalSolver` | Wagner & Whitin (1958) | O(n²) | O(n²) | yes |
| `WagnerWhitinEvansSolver` | Evans (1985) | O(n²) | O(n) | yes |
| `WagnerWhitinSolver` | Wagelmans et al. WW specialization (1992) | O(n) | O(n) | WW condition |
| `WagelmansGeneralSolver` | Wagelmans et al. general algorithm (1992) | O(n log n) | O(n) | yes |

## Validation

The tests cross-validate the general solver against:

1. an independent O(n²) forward dynamic-programming oracle;
2. `WagnerWhitinClassicalSolver`;
3. `WagnerWhitinEvansSolver`;
4. `WagnerWhitinSolver` on instances satisfying the Wagner-Whitin condition.

Random test campaigns include both zero-demand periods and strongly
nonmonotone transformed production costs.

## References

1. A. Wagelmans, S. van Hoesel and A. Kolen (1992).
   *Economic Lot Sizing: An O(n log n) Algorithm That Runs in Linear Time in
   the Wagner-Whitin Case*.
   Operations Research 40(S1), S145-S156.
   DOI: https://doi.org/10.1287/opre.40.1.S145

2. S. van Hoesel, A. Wagelmans and B. Moerman (1994).
   *Using Geometric Techniques to Improve Dynamic Programming Algorithms for
   the Economic Lot-Sizing Problem and Extensions*.
   European Journal of Operational Research 75(2), 312-331.
   DOI: https://doi.org/10.1016/0377-2217(94)90077-9
