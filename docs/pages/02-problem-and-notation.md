\page problem_and_notation ULS Problem and Notation

# ULS Problem and Notation

ULSAlgorithms represents the classical finite-horizon deterministic
uncapacitated lot-sizing problem without backlogging.

For period \f$t=1,\ldots,T\f$:

- \f$d_t\f$: demand;
- \f$f_t\f$: fixed setup cost;
- \f$p_t\f$: unit production cost;
- \f$h_t\f$: end-of-period unit holding cost;
- \f$x_t\f$: production quantity;
- \f$I_t\f$: end-of-period inventory;
- \f$y_t\in\{0,1\}\f$: setup decision.

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

The API assumes zero initial inventory and no backlogging. Exact algorithms in
the library reconstruct standard zero-ending-inventory solutions.

## Cost conventions

`HoldingCosts[t]` is the cost of carrying one unit in **end-of-period**
inventory after period `t`.

The final holding-cost coefficient is consequently not used by a standard
zero-ending-inventory solution, although the API keeps a horizon-length vector
for a regular memory layout.

## Regeneration intervals

Many exact methods exploit the zero-inventory-ordering structure. A
replenishment at period \f$i\f$ may cover a regeneration interval
\f$i,\ldots,j\f$.

That perspective supports several algorithmic interpretations in this
repository:

- dynamic programming;
- geometric lower-envelope methods;
- shortest paths in an acyclic network;
- branch-and-bound/subproblem dominance;
- planning-horizon pruning.

See @ref exact_algorithms.
