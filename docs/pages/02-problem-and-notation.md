\page problem_and_notation ULS Problem and Notation

# ULS Problem and Notation

ULSAlgorithms represents the classical finite-horizon deterministic uncapacitated lot-sizing problem without backlogging.

For period \(t=1,\ldots,T\):

- \(d_t\): demand;
- \(f_t\): fixed setup cost;
- \(p_t\): unit production cost;
- \(h_t\): end-of-period unit holding cost;
- \(x_t\): production quantity;
- \(I_t\): end-of-period inventory;
- \(y_t\in\{0,1\}\): setup decision.

A standard mixed-integer representation is

\f[
\min \sum_{t=1}^{T}
\left(
f_t y_t + p_t x_t + h_t I_t
\right)
\f]

subject to

\f[
I_{t-1}+x_t=d_t+I_t
\qquad t=1,\ldots,T,
\f]

\f[
x_t\ge 0,\qquad I_t\ge 0,\qquad y_t\in\{0,1\},
\f]

and a setup-linking condition such as

\f[
x_t \le M_t y_t.
\f]

The API assumes zero initial inventory and no backlogging. Exact algorithms in the library reconstruct standard zero-ending-inventory solutions.

## Cost conventions

`HoldingCosts[t]` is the cost of carrying one unit in **end-of-period** inventory after period `t`.

The final holding-cost coefficient is consequently not used by a standard zero-ending-inventory solution, although the API keeps a horizon-length vector for a regular memory layout.

## Regeneration intervals

Many exact methods exploit the zero-inventory-ordering structure. A replenishment at period \(i\) may cover a regeneration interval \(i,\ldots,j\).

That perspective supports several algorithmic interpretations in this repository:

- dynamic programming;
- geometric lower-envelope methods;
- shortest paths in an acyclic network;
- branch-and-bound/subproblem dominance;
- planning-horizon pruning.

See @ref exact_algorithms.
