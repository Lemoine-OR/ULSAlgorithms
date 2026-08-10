# ULSAlgorithms v0.20.0 — Classical (l,S) Cutting Planes

Adds two separate exact strategies:

- GeneralLsCuttingPlaneSolver
- WagnerWhitinLsCuttingPlaneSolver

and two public separators:

- GeneralLsCutSeparator
- WagnerWhitinLsCutSeparator

The general separator performs exact separation of the classical exponential
(l,S) family in O(T²). The Wagner-Whitin separator evaluates the O(T²)
prefix-S specialization.

Every generated candidate is stored in CutGenerationReport with its final
disposition and row name when added.

The algorithm performs root LP separation and then an exact strengthened MILP
solve with the same selected optimization engine.

## Apply

Extract into:

D:\Dev\UlsAlgorithm\ULSAlgorithms

Replace version.json when prompted.

Then:

1. Release → Rebuild Solution
2. Run All Tests
3. Do not commit yet
4. Report warnings/errors and test status
