# Changelog

All notable changes to ULSAlgorithms are documented in this file.

The project follows Semantic Versioning from the 1.0.0 release onward.

## [Unreleased]

### Added
- Real COIN-OR CBC end-to-end qualification for all six public solver-backed ULS strategies.
- Linux CI and pre-release qualification install the Ubuntu `coinor-cbc` package and force explicit CBC execution without fallback.
- Repository-wide `.csproj` discovery so build validation cannot silently omit tool, smoke, benchmark, test or library projects.
- Audited scientific-provenance baseline covering all 42 public strategy IDs.

### Changed
- `Build-All.ps1` now restores and compiles every discovered `.NET` project explicitly in Release configuration, in addition to the primary solution build.
- Linux validation now performs the full repository build and complete unit-test suite before running the portability smoke.
- The release workflow requires the same full Linux validation before the Windows publication job may start.
- Scientific metadata now records the published Evans (1985) DOI and the DeMatteis (1968) DOI used by Part-Period Balancing.
- Lyu-Lee complexity metadata now distinguishes `O(T²)` total implementation work from the ideal `O(T²/p)` parallel candidate-evaluation span.

### Validation
- The CBC qualification compares all four mathematical formulations and both `(l,S)` cutting-plane strategies against the self-contained `adaptive-exact` oracle on a deterministic instance with known objective 680.
- The qualification verifies `Optimal` status, finite objective agreement, a reconstructed ULS solution, and recorded CBC provenance.
- Every current repository `.csproj` is part of the build gate, including `PublicApiExporter`, `PortabilitySmoke` and `CbcIntegrationSmoke`, even when a project is not listed in `ULSAlgorithms.sln`.
- Scientific metadata tests now lock reference, DOI, complexity, applicability and implementation characterization for every public strategy before the 1.0 compatibility freeze.

## [0.28.0] - 2026-08-10

### Added
- Versioned, human-readable JSON solver configurations for reproducible experiments.
- `UlsSolverConfiguration` with parse/load/save/validation helpers.
- `UlsSolverFactory.Create(UlsSolverConfiguration)`.
- Public API compatibility baseline tooling.
- Stable solver IDs included in the public compatibility baseline.
- MIT license.
- Validated NuGet package as a release artifact.
- Linux portability smoke test for the self-contained exact path.
- API stability policy and final pre-1.0 release-hardening documentation.

### Changed
- Documentation warnings become release-blocking.
- Doxygen references use fully qualified public type names.
- Scientific metadata for Aggarwal-Park now records the complete 1993 Operations Research citation and DOI.
- README uses the catalog/factory as the recommended entry point.
- Release manifests include the NuGet package and its SHA-256 sidecar.

### Compatibility
- No public solver strategy was removed or renamed.
- All 42 public strategy IDs remain unchanged.
- Existing direct constructors and `UlsSolverFactory.Create(string)` remain supported.

## [0.27.0] - 2026-08-10

- Added strict constructor-level configuration to the runtime solver factory.
- Reused existing adaptive, parallel, optimization and cutting-plane option models.
- Added configuration-capability metadata to the canonical runtime catalog.

## [0.26.0] - 2026-08-10

- Added the canonical runtime solver catalog and stable-ID factory.
- Added runtime/documentation catalog synchronization.

## [0.25.0] - 2026-08-10

- Removed the extra adaptive applicability scan by caching the immutable no-speculative-motive characteristic.
- Retained the public Wagner-Whitin safety validation.
- Benchmarks confirmed effectively zero adaptive dispatch overhead on the measured NSM workloads.

## [0.24.0]

- Added automatic exact strategy selection.

## Earlier 0.x releases

Earlier development releases established the exact algorithms, classical
heuristics, mathematical formulations, `(l,S)` cutting-plane methods,
validation campaigns, benchmarking infrastructure, documentation portal and
reproducible release pipeline. See the GitHub release history for the exact
artifact record of those versions.
