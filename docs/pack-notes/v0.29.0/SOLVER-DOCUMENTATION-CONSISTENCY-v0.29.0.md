# v0.29.0 — solver documentation consistency pass

## Purpose

This block removes the last known documentation contradictions before the 1.0
qualification release.

Two pages still described infrastructure from older development releases as if
the solver-backed strategy layer had not yet been implemented:

- `docs/pages/15-solver-adapters.md`;
- `docs/pages/17-solver-execution.md`.

That was inconsistent with the current runtime, where four formulation
strategies and two `(l,S)` cutting-plane strategies already execute through the
portable optimization layer.

## Changes

### Solver adapters

The adapter page now describes the adapters as production infrastructure used by
the current execution layer, not as discovery support for future formulations.

It retains the engine-specific discovery details and documents the completed
end-to-end path from `UlsProblem` to validated `UlsSolution`.

### Solver execution

The execution page no longer ends at the generic `LinearModel` layer. It now
documents the implemented formulation and cutting-plane integration.

The numerical section is also synchronized with the current independent
checker:

- zero tolerance: `1e-8`;
- feasibility tolerance: `1e-7`;
- integrality tolerance: `1e-7`;
- continuous near-integer tolerance: `1e-8`;
- bounds remain absolute;
- constraint feasibility uses the mixed absolute/relative row scale
  `max(1, |rhs|, sum |a_i x_i|)`.

### Formulation strategy page

The formulation page now uses current-state wording and explicitly records the
same scaled row-feasibility policy.

### README

The validation section now reflects the actual v0.29 qualification gate:

- repository-wide Release build on Windows and Linux;
- complete test suite on both platforms;
- Linux portability smoke;
- real CBC end-to-end qualification for all six solver-backed strategies.

## Compatibility

This block changes documentation only. It does not change:

- public solver IDs;
- public .NET API;
- mathematical formulations;
- cutting-plane algorithms;
- solver-selection policy;
- numerical implementation.
