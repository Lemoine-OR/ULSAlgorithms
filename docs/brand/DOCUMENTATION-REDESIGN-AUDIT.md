# ULSAlgorithms documentation redesign — audit and rationale

## What was wrong with the current public documentation

The v0.14.0 public site was essentially the default Doxygen shell with the README used as its main page.

That created four structural problems:

1. **No real landing portal.** The public root exposed a source-generated Doxygen page instead of a designed project entry point.
2. **No user journey.** Algorithm pages existed, but there was no hierarchy from problem definition → algorithm families → selection → complexity → scientific references.
3. **No visual identity.** `PROJECT_LOGO`, `PROJECT_ICON` and a custom Doxygen stylesheet were absent from the ULSAlgorithms Doxyfile.
4. **The GitHub repository landing page was almost empty.** The README contained only the project title and a single description sentence.

There was also repository-root clutter from temporary overlay install notes and SHA manifests used during incremental development.

## Design target

The redesign follows the successful pattern already used by LotSizingDataModel:

- a custom GitHub Pages portal at the documentation root;
- a styled Doxygen technical site below it;
- explicit logo and icon assets;
- link validation;
- version and commit injection;
- a professional README.

ULSAlgorithms then goes further because an algorithm library needs a different information architecture from a data-model framework.

## New information architecture

1. Overview
2. Getting Started
3. ULS Problem & Notation
4. Algorithm Catalog
5. Exact Algorithms
6. Heuristics
7. Algorithm Selection
8. Complexity & Applicability
9. Validation & Benchmarks
10. API Reference
11. Scientific References
12. Releases & Reproducibility
13. Adding an Algorithm

The machine-readable `docs/algorithm-catalog.json` is the single source of truth for the algorithm inventory. The build generates the public counts and comparison table from it.
