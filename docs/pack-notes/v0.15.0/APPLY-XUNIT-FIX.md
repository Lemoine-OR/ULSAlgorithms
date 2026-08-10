# ULSAlgorithms v0.15.0 — xUnit compile fix

Cause:
The two new test files omitted `using Xunit;`, while this repository uses
explicit xUnit imports rather than a global using.

Changed only:
- tests/ULSAlgorithms.Tests/Optimization/SolverSelectionTests.cs
- tests/ULSAlgorithms.Tests/CuttingPlanes/CutGenerationReportTests.cs

Fix:
Add `using Xunit;` to both files.

No production source, documentation logic, solver infrastructure or
`version.json` is modified.
