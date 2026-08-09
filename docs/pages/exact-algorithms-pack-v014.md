\page exact_algorithms_pack_v014 Exact Algorithms Pack II - v0.14.0

# Exact Algorithms Pack II - v0.14.0

Version 0.14.0 adds three historical exact strategies:

| Solver | Publication | Architecture | Time | Memory |
|---|---|---|---:|---:|
| `SaydamMcKnewFastWagnerWhitinSolver` | Saydam & McKnew 1987 | precomputed triangular WW | O(T²) | O(T²) |
| `JacobsKhumawalaBranchAndBoundSolver` | Jacobs & Khumawala 1987 | subproblem branch/dominance | O(T²) | O(T) |
| `ZangwillNetworkSolver` | Zangwill 1969 | backward DAG shortest path | O(T²) | O(T) |

All three are separate public `IUlsSolver` strategies and return
`UlsSolveStatus.Optimal`.

## Validation campaign

The pack adds:

- a strongly nonmonotone general-cost instance;
- explicit zero-demand periods;
- all-zero demand;
- cancellation;
- 4,000 random general ULS instances.

Every random instance is cross-validated against:

- the independent quadratic test oracle;
- `WagelmansGeneralSolver`.

## Provenance policy

The mathematical identities of the three publications are preserved. Where the
original program listing or graphical worksheet is not directly transcribed,
the Doxygen page explicitly calls the implementation a modern C#
reconstruction rather than pretending source-level equivalence.

Evans-Saydam-McKnew (1989), which extends the problem to concave procurement
costs, remains outside this pack because the current `UlsProblem` cost model
does not represent an arbitrary concave procurement-cost function.

Golany-Maman-Yadin (1992) also remains on the roadmap pending a sufficiently
detailed primary description of its planning-horizon decomposition rules.
