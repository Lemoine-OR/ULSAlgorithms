# ULSAlgorithms v0.20.0 — LinearConstraintSense ambiguity fix

Fixes the single ambiguous reference in LsCutModelBuilderTests.cs.

The test now explicitly uses:

ULSAlgorithms.CuttingPlanes.LinearConstraintSense.GreaterOrEqual

The production LsCutModelBuilder already uses explicit aliases for the
CuttingPlanes and Optimization.Modeling constraint-sense enums.

No production logic and no version metadata are changed.
The release remains v0.20.0.
