\page algorithm_selection Algorithm Selection Guide

# Algorithm Selection Guide

There is no single universally best public strategy. The appropriate choice depends on the cost structure, horizon size, frequency of repeated solves and whether optimality is required.

| Use case | Recommended starting point | Why |
|---|---|---|
| General exact ULS | `WagelmansGeneralSolver` or `FedergruenTzurSolver` | Strong asymptotic performance for general costs |
| General exact reference / auditing | `WagnerWhitinEvansSolver` or `ZangwillNetworkSolver` | Simple independent architectures |
| Wagner–Whitin costs / no speculative motive | `WagnerWhitinSolver` | Linear-time specialization |
| Very large restricted instances | Linear specialized methods | O(T) when assumptions hold |
| Many-core experiment | `LyuLeeParallelSolver` | Parallel predecessor evaluation |
| Low-memory quadratic reference | `WagnerWhitinEvansSolver` | O(T) working memory |
| Memory-for-speed historical WW | `SaydamMcKnewFastWagnerWhitinSolver` | Precomputed triangular costs |
| Fast classical heuristic | `SilverMealSolver`, `GroffSolver`, PPB family | O(T) heuristics |
| Baseline MRP policy | `LotForLotSolver` | Transparent reference policy |

## Important rule

**Applicability precedes speed.**

A theoretically faster method can be invalid if its structural assumptions do not match the instance. Public restricted solvers expose applicability checks where appropriate and tests explicitly cover rejection cases.

See @ref complexity_applicability for the complete matrix.
