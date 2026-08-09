\page sadjadi_aryanezhad_sadeghi Sadjadi-Aryanezhad-Sadeghi improved Wagner-Whitin

# Sadjadi-Aryanezhad-Sadeghi improved Wagner-Whitin

Public class: `SadjadiAryanezhadSadeghiSolver`.

Reference:

S. J. Sadjadi, M. B. Gh. Aryanezhad and H. A. Sadeghi (2009),
*An Improved WAGNER-WHITIN Algorithm*,
International Journal of Industrial Engineering & Production Research,
20(3), 117-123.

## Fixed-cost method

The paper first assumes constant setup, purchase and holding costs and retains
the forward Wagner-Whitin recursion while eliminating branches using the
Derived Part Period threshold

\f[
DPP=A/H.
\f]

When extending a candidate setup one period earlier would carry more than DPP
units of already-covered future demand, the corresponding branch and all
earlier branches are discarded.

The algorithm also uses the Wagner-Whitin Planning Horizon Theorem.

## Relationship to Heady-Zhu

The concepts overlap strongly with the earlier Heady-Zhu implementation.
ULSAlgorithms nevertheless keeps the 2009 publication as its own public
strategy because the project intentionally preserves distinct published
methods for reproducibility and benchmarking.

This class is independently coded rather than delegating to
`HeadyZhuEconomicPartPeriodSolver`.

## Applicability

The current implementation corresponds to the paper's first fixed-cost model:

- constant setup cost;
- constant unit production/purchase cost;
- constant relevant unit holding cost;
- no backlogging.

The paper also proposes an extension with varying costs and later discusses
backlogging. Those variants require a wider problem model and are not
silently folded into this class.

## Validation

The exact 12-period example from the paper is included:

- `A=54`;
- `H=0.4`;
- `DPP=135`;
- demands `10,62,12,130,154,129,88,52,124,160,238,41`.

3,000 deterministic random fixed-cost instances are cross-validated against
the independent quadratic oracle and the Heady-Zhu public solver.
