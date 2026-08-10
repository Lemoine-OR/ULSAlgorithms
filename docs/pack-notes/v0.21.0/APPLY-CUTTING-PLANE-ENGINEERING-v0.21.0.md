# ULSAlgorithms v0.21.0 — Cutting-Plane Engineering

Adds:

- CutSelectionPolicy:
  - AllViolated
  - MostViolatedPerL
  - TopByViolation
  - TopByEfficacy
- MinimumEfficacy
- MaximumCutsPerIteration
- CutDisposition.NotSelected
- CuttingPlaneIterationStatistics
- CuttingPlaneConvergenceReport
- root bound / root gap closure metrics
- pure BenchmarkDotNet LsSeparationBenchmarks

The default remains AllViolated, preserving v0.20.0 behavior.

No new mathematical algorithm is introduced in this release; the public
strategy count remains 22 exact + 11 heuristic = 33.

## Apply

Extract into:

D:\Dev\UlsAlgorithm\ULSAlgorithms

Replace version.json when prompted.

Then:

1. Release → Rebuild Solution
2. Run All Tests
3. Do not commit yet
4. Report warnings/errors and test status
