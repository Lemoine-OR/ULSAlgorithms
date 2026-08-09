\page heady_zhu Heady-Zhu Economic-Part-Period implementation

# Heady-Zhu Economic-Part-Period implementation

## Public solver

`HeadyZhuEconomicPartPeriodSolver`

- Exact: **yes**
- Algorithm family: **Wagner-Whitin**
- Planning-horizon pruning: **yes**
- Economic-Part-Period pruning: **yes**
- Worst-case time: **O(n²)**
- Auxiliary working memory: **O(n)**
- Practical work: **data dependent**
- Current implementation scope: **constant setup, production and relevant holding costs**

## Scientific provenance

Heady and Zhu describe an improved implementation of Wagner-Whitin based on
two ideas:

1. the Wagner-Whitin Planning Horizon Theorem;
2. the Economic-Part-Period concept.

Reference:

R. B. Heady and Z. Zhu (1994).

*An Improved Implementation of the Wagner-Whitin Algorithm.*

Production and Operations Management, 3(1), 55-63.

DOI: https://doi.org/10.1111/j.1937-5956.1994.tb00109.x

The publisher abstract reports that, over their rigorous test conditions, their
implementation was about twice as fast as the previously fastest algorithm,
used about half the array storage, and exhibited execution time approximately
linear in planning-horizon length. Those are empirical findings from the
paper, not worst-case asymptotic guarantees.

## Implementation-source transparency

The publicly accessible publisher record for Heady-Zhu exposes the abstract
but not the complete algorithm text. ULSAlgorithms therefore does not claim
that this class is a line-by-line reproduction of their original program.

For the exact fixed-cost Economic-Part-Period cutoff, the implementation also
uses the explicit later primary exposition:

S. J. Sadjadi, M. B. Gh. Aryanezhad and H. A. Sadeghi (2009).

*An Improved WAGNER-WHITIN Algorithm.*

International Journal of Industrial Engineering & Production Research,
20(3), 117-123.

That paper explicitly derives the fixed-cost quantity

\f[
DPP = \frac{A}{H},
\f]

and shows how it is used to stop Wagner-Whitin branch calculations.

This source distinction is intentional: Heady-Zhu is the historical origin
for the combined Planning-Horizon / Economic-Part-Period implementation
family, while Sadjadi et al. provide an openly accessible primary derivation
used to make this implementation auditable.

## Applicability

The current solver deliberately implements the fixed-cost specialization.

It requires:

\f[
A_t = A,
\qquad
p_t = p,
\qquad
h_t = h
\f]

for all economically relevant periods.

The final holding-cost entry is not constrained because terminal inventory is
zero and that coefficient is never charged.

`HeadyZhuEconomicPartPeriodSolver.IsApplicable(problem)` exposes the check.
`Solve` throws `NotSupportedException` outside this domain rather than silently
applying an unsafe cutoff.

## Economic-Part-Period dominance

Consider an order opened in period `i` that covers demand through period `j`.

Moving that setup one period earlier makes every unit already scheduled for
periods `i+1..j` wait one additional period. The incremental holding cost is

\f[
h \sum_{k=i+1}^{j} d_k.
\f]

If

\f[
h \sum_{k=i+1}^{j} d_k > A,
\f]

then carrying those future units one additional period is more expensive than
opening an additional setup.

Equivalently,

\f[
\sum_{k=i+1}^{j} d_k > \frac{A}{h}.
\f]

The candidate setup at `i` is then dominated by splitting the plan and opening
another setup in `i+1`. Any still earlier setup carries at least as much future
demand, so all remaining earlier candidates can also be discarded.

This yields a safe early termination of the backward predecessor scan.

## Planning Horizon Theorem

For every positive-demand prefix, the solver records the latest optimal last
setup period. Wagner and Whitin's Planning Horizon Theorem permits predecessor
periods before that point to be excluded from subsequent prefix optimizations.

The lower candidate bound is therefore monotone nondecreasing.

Zero-demand prefix extensions do not advance this bound because no setup is
created by the zero-cost transition.

## Complexity

Let `m_t` be the number of predecessor periods actually examined for prefix
`t`.

The work is

\f[
O\left(\sum_t m_t\right).
\f]

Worst case:

\f[
m_t = O(t)
\quad\Rightarrow\quad
O(n^2).
\f]

When the Economic-Part-Period cutoff and planning-horizon bound keep each
`m_t` small, observed work approaches linear growth.

The class therefore intentionally advertises **O(n²) worst case** rather than
turning the empirical approximately-linear behavior reported in the
literature into an unsupported complexity theorem.

## Memory

Only two rented O(n) work arrays are required:

- dynamic-programming value;
- predecessor period.

Regeneration-interval costs are accumulated in scalar variables during the
backward scan; no triangular matrix is stored.

## Published fixed-cost example

The test suite reproduces the 12-period fixed-cost example in the later
primary exposition:

- setup cost `A = 54`;
- holding cost `H = 0.4`;
- hence `DPP = 135`;
- demands:
  `10, 62, 12, 130, 154, 129, 88, 52, 124, 160, 238, 41`.

The exact optimum is cross-validated with the independent quadratic oracle.
The resulting objective (excluding a constant zero production cost) is
`501.2`.

## Validation

The v0.10.0 campaign adds:

- published fixed-cost Economic-Part-Period example;
- explicit `DPP = A/H` check;
- zero-demand and zero-holding-cost cases;
- applicability rejection for varying setup, production and relevant holding
  costs;
- **5,000 random fixed-cost instances** cross-validated against:
  - the independent O(n²) oracle,
  - Evans 1985,
  - Bahl-Taj 1991,
  - the Wagelmans O(n) Wagner-Whitin solver;
- **1,000 high-demand / low-setup instances** designed to exercise strong
  Economic-Part-Period pruning;
- cancellation tests.

## Related public solvers

| Solver | Distinguishing idea | Worst-case time |
|---|---|---:|
| `WagnerWhitinClassicalSolver` | explicit classical DP matrix | O(n²) |
| `WagnerWhitinEvansSolver` | incremental low-storage recurrence | O(n²) |
| `BahlTajPlanningHorizonSolver` | Evans + planning-horizon pruning | O(n²), data dependent |
| `HeadyZhuEconomicPartPeriodSolver` | planning horizon + EPP cutoff | O(n²), data dependent |
| `WagnerWhitinSolver` | Wagelmans convex-envelope specialization | O(n) |

## References

1. R. B. Heady and Z. Zhu (1994).
   *An Improved Implementation of the Wagner-Whitin Algorithm.*
   Production and Operations Management 3(1), 55-63.
   DOI: https://doi.org/10.1111/j.1937-5956.1994.tb00109.x

2. S. J. Sadjadi, M. B. Gh. Aryanezhad and H. A. Sadeghi (2009).
   *An Improved WAGNER-WHITIN Algorithm.*
   International Journal of Industrial Engineering & Production Research
   20(3), 117-123.

3. H. M. Wagner and T. M. Whitin (1958).
   *Dynamic Version of the Economic Lot Size Model.*
   Management Science 5(1), 89-96.
   DOI: https://doi.org/10.1287/mnsc.5.1.89
