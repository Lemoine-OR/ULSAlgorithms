# ULSAlgorithms v0.11.0 compile fix

This patch changes only:

`src/ULSAlgorithms/Exact/Parallel/LyuLeeParallelSolver.cs`

Cause:
The solver lives in namespace `ULSAlgorithms.Exact.Parallel`, so the
unqualified expression `Parallel.For(...)` was resolved against that namespace
instead of `System.Threading.Tasks.Parallel`.

Fix:
`Parallel.For(...)`
becomes
`global::System.Threading.Tasks.Parallel.For(...)`

The missing ULSAlgorithms.dll metadata errors are downstream consequences of
the main project failing to compile; they should disappear once this compile
error is fixed.

After overlaying the patch:
1. Clean/Rebuild Release.
2. Run all tests.
3. Do not commit until the full suite is green.
