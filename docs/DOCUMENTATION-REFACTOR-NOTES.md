# ULSAlgorithms documentation refactor after v0.22.0

This is a documentation-only restructuring. It does not change `version.json`, the public C# API or algorithm behavior.

## What changes

- GitHub README becomes a card-based landing page listing all 37 public strategies.
- GitHub Pages home becomes a searchable/filterable card browser.
- Four user-facing method families: direct exact, mathematical optimization, cutting planes, heuristics.
- One generated dedicated HTML page per algorithm.
- One stable generated Doxygen API landing page per algorithm.
- Uniform algorithm page structure: description, specifications, operation, minimal C# example, scientific source, API/source links.
- Getting Started and API guide are simplified around `UlsProblem`, `IUlsSolver`, `UlsSolveResult`, `UlsSolution`.
- Release-pack navigation is removed.

## Removal step

After extracting the overlay into the repository root, run:

```powershell
powershell.exe -ExecutionPolicy Bypass -File .\docs\Apply-Documentation-Refactor.ps1
```

This removes five obsolete user-facing pack pages. Internal `docs/pack-notes` are retained as reproducibility metadata and are not rendered into the user portal.

## Validation

Then run the normal documentation workflow locally or push only after the user has validated the generated site. This refactor intentionally does not create a new package release.
