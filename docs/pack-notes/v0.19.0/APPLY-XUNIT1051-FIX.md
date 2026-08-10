# ULSAlgorithms v0.19.0 — xUnit1051 fix

Fixes the xUnit analyzer warning xUnit1051 in
SolverBackedFormulationSolverTests.

The asynchronous solver call now receives:

TestContext.Current.CancellationToken

No production code and no version metadata are changed.
