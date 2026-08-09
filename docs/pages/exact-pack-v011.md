\page exact_pack_v011 Exact Algorithms Pack I - v0.11.0

# Exact Algorithms Pack I - v0.11.0

Version 0.11.0 adds three public exact strategies in one release:

| Solver | Publication | Domain | Complexity |
|---|---|---|---|
| `ChowdhuryBakiAzabSolver` | Chowdhury, Baki & Azab 2018 | WW, stationary h | O(T) |
| `SadjadiAryanezhadSadeghiSolver` | Sadjadi et al. 2009 | fixed costs | O(T²) worst case, pruned |
| `LyuLeeParallelSolver` | Lyu & Lee 2001 | general ULS | O(T²) work, parallel |

Each solver has its own source file, tests, Doxygen page and BenchmarkDotNet
benchmark. None replaces or aliases an existing strategy.

Evans-Saydam-McKnew (1989) and Golany-Maman-Yadin (1992) remain on the exact
algorithm roadmap. They are intentionally deferred until sufficiently detailed
primary algorithm descriptions are available for an auditable implementation.
