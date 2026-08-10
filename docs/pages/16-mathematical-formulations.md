\page mathematical_formulations Mathematical Programming Formulations

# Mathematical Programming Formulations

ULSAlgorithms v0.17.0 introduces four solver-independent formulations of the
classical deterministic uncapacitated lot-sizing problem.

The four-family taxonomy follows:

N. Brahimi, S. Dauzère-Pérès, N. M. Najid, A. Nordli,
"Single item lot sizing problems",
European Journal of Operational Research 168(1), 1-16, 2006,
DOI 10.1016/j.ejor.2004.01.054.

## 1. Aggregate inventory-balance formulation

Variables:

- `x[t] >= 0`: production quantity;
- `y[t] in {0,1}`: setup;
- `I[t] >= 0`: end-of-period inventory.

Balance:

\f[
I_{t-1} + x_t - I_t = d_t.
\f]

The implementation uses the tight ULS upper bound:

\f[
x_t \le D_{t,T} y_t,
\qquad
D_{t,T} = \sum_{k=t}^{T} d_k.
\f]

Final inventory is fixed to zero by its upper bound.

This formulation is valid for every `UlsProblem`.

Historical ULS source:

H. M. Wagner, T. M. Whitin,
"Dynamic Version of the Economic Lot Size Model",
Management Science 5(1), 89-96, 1958,
DOI 10.1287/mnsc.5.1.89.

## 2. Disaggregated / facility-location formulation

`q[t,k]` denotes the amount of demand in period `k` produced in period `t`,
with `t <= k`.

\f[
\sum_{t=1}^{k} q_{tk} = d_k
\f]

and

\f[
q_{tk} \le d_k y_t.
\f]

The unit coefficient of `q[t,k]` is the delivered unit cost:

\f[
p_t + \sum_{r=t}^{k-1} h_r.
\f]

Zero-demand assignment variables are omitted.

Facility-location reference:

J. Krarup, O. Bilde,
"Plant location, Set Covering and Economic Lot Size:
An O(mn)-Algorithm for Structured Problems",
1977,
DOI 10.1007/978-3-0348-5936-3_10.

## 3. Regeneration shortest-path formulation

Nodes represent zero-inventory boundaries. A replenishment arc `(t,j+1)`
represents one setup in period `t` serving demand through period `j`.

The arc cost is:

\f[
f_t + \sum_{k=t}^{j}
d_k \left(
p_t + \sum_{r=t}^{k-1} h_r
\right).
\f]

Zero-demand periods can be crossed through explicit zero-cost skip arcs.

Arc variables are continuous in `[0,1]`; the formulation is a unit-flow model
on an acyclic network.

This formulation is enabled only under the no-speculative-motive condition:

\f[
p_t + h_t \ge p_{t+1}.
\f]

Network reference:

W. I. Zangwill,
"A Backlogging Model and a Multi-Echelon Model of a Dynamic Economic Lot Size
Production System—A Network Approach",
Management Science 15(9), 506-527, 1969.

Related efficient Wagner-Whitin implementation:

J. R. Evans,
"An Efficient Implementation of the Wagner-Whitin Algorithm for Dynamic
Lot-Sizing",
Journal of Operations Management 5(2), 229-235, 1985,
DOI 10.1016/0272-6963(85)90009-9.

## 4. Inventory-eliminated formulation

Inventory is substituted algebraically:

\f[
I_t =
\sum_{i=1}^{t} x_i -
\sum_{i=1}^{t} d_i.
\f]

Therefore:

\f[
\sum_{i=1}^{t} x_i
\ge
\sum_{i=1}^{t} d_i,
\qquad t<T,
\f]

and final zero inventory is enforced by:

\f[
\sum_{i=1}^{T} x_i =
\sum_{i=1}^{T} d_i.
\f]

The holding-cost contribution is folded into the production coefficients plus
an objective constant. The formulation contains no explicit inventory
variables.

## Portable modeling layer

The four builders return a `UlsFormulation` containing:

- `LinearModel`;
- `UlsFormulationKind`;
- scientific source description;
- semantic variable map.

`LinearModel` is independent of CPLEX, Gurobi, Xpress and CBC. This is the
model representation that the solver-execution layer will consume.

## Scope

v0.17.0 builds and validates the four formulations but does not yet execute
them through a mathematical optimizer.

This separation is intentional: formulation correctness can be tested without
requiring any commercial solver installation. The subsequent execution layer
will use the automatic solver-selection infrastructure introduced in v0.15.0
and the concrete machine adapters introduced in v0.16.0.
