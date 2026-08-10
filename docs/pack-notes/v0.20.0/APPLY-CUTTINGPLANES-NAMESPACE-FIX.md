# ULSAlgorithms v0.20.0 — CuttingPlanes namespace fix

Fixes namespace resolution for types declared directly in:

ULSAlgorithms.CuttingPlanes

from child namespaces:

- ULSAlgorithms.CuttingPlanes.Separation
- ULSAlgorithms.CuttingPlanes.Separation.Internal
- ULSAlgorithms.CuttingPlanes.Internal
- corresponding xUnit test namespaces

Explicitly imports:

using ULSAlgorithms.CuttingPlanes;

This resolves references to:

- CutSeparationMethod
- LsCutDefinition
- CutCoefficient
- LinearConstraintSense (cut-traceability enum where needed)

No production algorithm logic and no version metadata are changed.
The release remains v0.20.0.
