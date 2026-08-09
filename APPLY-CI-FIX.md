# ULSAlgorithms v0.9.0 CI test-data fix

Overlay this package on the repository root.

It changes only:
`tests/ULSAlgorithms.Tests/Exact/WagnerWhitin/BahlTajPlanningHorizonSolverTests.cs`

Corrections:
- ZeroDemandPeriod_CanRemainCandidateForFutureProduction:
  unit production costs [1, 20] -> [1, 2], preserving expected total cost 21
  while satisfying p[0] + h[0] >= p[1].
- AllZeroDemand_ReturnsZeroWithoutSetups:
  production costs replaced by [8, 7, 6, 5], which satisfy the solver's
  applicability condition.
- Both tests now assert IsApplicable(problem) explicitly.

No production solver source is changed.
