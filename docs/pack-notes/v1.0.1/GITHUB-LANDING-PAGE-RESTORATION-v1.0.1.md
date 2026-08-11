# ULSAlgorithms v1.0.1 — GitHub landing-page restoration

## Purpose

v1.0.1 is a documentation-only patch release.

The v1.0.0 qualification left the repository README technically complete but
removed the visual GitHub landing-page structure that existed before the final
1.0 hardening work. This patch restores that project presentation without
changing the stable public API or any solver implementation.

## Restored

- ULSAlgorithms project logo;
- Build and Test, Documentation and Latest Release badges;
- .NET 10, MIT and stable-1.x badges;
- quick links to project/documentation, algorithms, Getting Started, latest
  release and source;
- four-family visual overview;
- recommended `adaptive-exact` entry point;
- clickable two-column panels for all 42 public strategies;
- stable factory IDs directly visible in every strategy panel;
- project, source, distribution, validation, citation and API-stability links;
- Lemoine-OR project footer.

## Stable public surface

No product source file is modified.

The following remain unchanged:

- 17 direct exact strategies;
- 4 mathematical formulations;
- 2 `(l,S)` cutting-plane strategies;
- 19 heuristics;
- 42 total public strategy IDs;
- public .NET API baseline;
- `IUlsSolver`;
- `UlsSolverConfiguration` schema version 1;
- numerical policies;
- solver adapter behavior;
- release qualification gates.

The expected unit-test inventory remains 272 tests.

## GitHub About panel

Repository metadata is separate from the source commit. After the source patch
is pushed, set:

Website:
`https://lemoine-or.github.io/ULSAlgorithms/`

Recommended topics:
`operations-research`, `lot-sizing`, `optimization`, `dynamic-programming`,
`heuristics`, `csharp`, `dotnet`.
