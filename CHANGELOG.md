## 1.1.0

### Reproducible computational study release

- Added execution provenance to `AdaptiveExactUlsSolver` through
  `AdaptiveExactUlsSolveResult`, exposing the exact strategy selected at runtime.
- Hardened solver-backed numerical execution, including CPLEX status mapping,
  variable-value normalization and fixed-integer polishing.
- Added regression coverage for CPLEX status interpretation, numerical
  normalization and fixed-integer polishing.
- Audited and corrected scientific metadata used by the public solver catalog.
- Validated the complete library with 288 passing tests.
- Aligned this release with the companion reproducible computational study of
  42 public ULS strategies on 600 benchmark instances.

# Changelog

All notable changes to ULSAlgorithms are documented in this file.

The project follows Semantic Versioning from the 1.0.0 release onward.

`CHANGELOG.md` was introduced in v0.28.0. Entries for v0.1.0 through v0.23.0
below were reconstructed from the immutable tagged repository history and the
tag-to-tag changes so that the complete public 0.x development history remains
available in one place.

## [Unreleased]

## [1.0.1] - 2026-08-11

### Changed
- Restored the GitHub repository README as a visual project landing page while preserving the stable v1.0 public API and implementation.
- Restored the ULSAlgorithms logo, build/documentation/release badges, quick project links and the four-family visual overview.
- Restored clickable method panels and updated them to the complete v1.0 catalog of 42 public strategies.
- Added stable solver IDs directly to the GitHub method panels so catalog/factory usage is visible from the repository home page.
- Kept the v1.0 factory, serializable configuration, external-solver, validation, distribution, citation and API-stability information on the landing page.

### Compatibility
- Documentation-only patch: no algorithm, public .NET API member, stable solver ID, numerical policy, optimization adapter or serialized configuration contract is changed.
- The stable v1.0 compatibility baseline remains unchanged.
## [1.0.0] - 2026-08-11

### Stability
- Establishes the first stable public API contract for ULSAlgorithms.
- Freezes the public .NET API baseline, existing stable solver IDs and `UlsSolverConfiguration` schema version 1 as the minimum compatibility contract for the 1.x line.
- Retains the complete 42-strategy catalog: 17 direct exact algorithms, 4 mathematical formulations, 2 `(l,S)` cutting-plane methods and 19 heuristics.
- Retains the validated solver-backed execution architecture for CPLEX, Gurobi, Xpress and COIN-OR CBC.
- Introduces no new algorithm, formulation, heuristic, numerical policy or public API member relative to v0.29.0.

### Validation
- Reuses the complete v0.29.0 qualification gate unchanged: repository-wide Release builds on Windows and Linux, 272 unit tests, Linux portability smoke and real CBC end-to-end qualification of all six solver-backed strategies.
- Requires runtime/documentation catalog synchronization and public API compatibility validation before publication.
- Requires official .NET package validation, isolated real NuGet consumer restore/build/run validation, `.nupkg` validation and portable-symbol `.snupkg` validation.
- Requires validated binary/documentation archives, release manifests and SHA-256 sidecars before publication.

### Compatibility
- Existing public .NET types and members form the stable 1.x compatibility baseline.
- Existing public solver IDs are stable compatibility identifiers.
- Serialized configuration schema version 1 is stable.
- Future compatible additions may ship in minor releases; intentional incompatible changes require a new major version.

## [0.29.0] - 2026-08-11

### Added
- Real COIN-OR CBC end-to-end qualification for all six public solver-backed ULS strategies.
- Linux CI and pre-release qualification install the Ubuntu `coinor-cbc` package and force explicit CBC execution without fallback.
- Repository-wide `.csproj` discovery so build validation cannot silently omit tool, smoke, benchmark, test or library projects.
- Audited scientific-provenance baseline covering all 42 public strategy IDs.
- Repository-level `CITATION.cff`, also embedded in the NuGet package.
- Portable NuGet symbol package (`.snupkg`) with PDB validation.
- Isolated real-consumer smoke that restores, compiles and executes against the exact generated `.nupkg`.

### Changed
- `Build-All.ps1` now restores and compiles every discovered .NET project explicitly in Release configuration, in addition to the primary solution build.
- Linux validation now performs the full repository build and complete unit-test suite before running the portability smoke.
- The release workflow requires the same full Linux validation before the Windows publication job may start.
- Scientific metadata now records the published Evans (1985) DOI and the DeMatteis (1968) DOI used by Part-Period Balancing.
- Lyu-Lee complexity metadata now distinguishes `O(T²)` total implementation work from the ideal `O(T²/p)` parallel candidate-evaluation span.
- Solver-adapter and solver-execution documentation now describes the implemented end-to-end formulation/cutting-plane architecture instead of historical future-state text.
- Numerical documentation now matches the scaled row-feasibility policy used by the independent portable-model checker.
- README validation claims now reflect the repository-wide Windows/Linux build gate and real CBC qualification.
- The product package enables official .NET package validation and explicit repository/Source Link packaging metadata.

### Validation
- The CBC qualification compares all four mathematical formulations and both `(l,S)` cutting-plane strategies against the self-contained `adaptive-exact` oracle on a deterministic instance with known objective 680.
- The qualification verifies `Optimal` status, finite objective agreement, a reconstructed ULS solution, and recorded CBC provenance.
- Every current repository `.csproj` is part of the build gate, including `PublicApiExporter`, `PortabilitySmoke` and `CbcIntegrationSmoke`, even when a project is not listed in `ULSAlgorithms.sln`.
- Scientific metadata tests now lock reference, DOI, complexity, applicability and implementation characterization for every public strategy before the 1.0 compatibility freeze.
- Main NuGet package structure, symbol-package structure, citation metadata, local-package consumption and release-manifest coverage are validated before publication.
- The complete test suite contains 272 passing tests at the v0.29.0 qualification point.

### Compatibility
- No public solver strategy ID is removed or renamed.
- The 42-strategy catalog remains intact.
- No public .NET API member is intentionally removed by the v0.29.0 hardening work.

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
- Scientific metadata for Aggarwal-Park records the complete 1993 Operations Research citation and DOI.
- README uses the catalog/factory as the recommended entry point.
- Release manifests include the NuGet package and its SHA-256 sidecar.

### Compatibility
- No public solver strategy was removed or renamed.
- All 42 public strategy IDs remained unchanged.
- Existing direct constructors and `UlsSolverFactory.Create(string)` remained supported.

## [0.27.0] - 2026-08-10

### Added
- Strict constructor-level configuration through the runtime solver factory.
- `UlsSolverCreationOptions` and configuration-capability metadata for strategies that expose tunable construction.
- Configurable construction for adaptive exact fallback, Lyu-Lee parallelism, optimization execution and cutting-plane engineering.

### Validation
- Added dedicated configurable-factory tests.
- Kept the generated documentation catalog synchronized with runtime configuration capabilities.

## [0.26.0] - 2026-08-10

### Added
- Canonical runtime `UlsSolverCatalog` covering the complete public strategy inventory.
- Stable lower-kebab-case strategy IDs and `UlsSolverFactory`.
- Public descriptors for family, category, complexity, applicability, scientific provenance and source location.
- Catalog exporter and CI synchronization check for `docs/algorithm-catalog.json`.

### Validation
- Added catalog/factory tests and made runtime/documentation catalog drift build-detectable.

## [0.25.0] - 2026-08-10

### Changed
- Removed the extra adaptive applicability scan by caching the immutable no-speculative-motive characteristic in `UlsProblem`.
- Retained the public Wagner-Whitin safety validation.
- Reduced adaptive dispatch overhead without changing solver selection semantics.

### Validation
- Added cached-dispatch regression tests.
- Recorded BenchmarkDotNet results showing effectively zero adaptive dispatch overhead on the measured NSM workloads.

## [0.24.0]

### Added
- Automatic exact strategy selection through `AdaptiveExactUlsSolver`.
- Problem-characteristic detection for the no-speculative-motive condition.
- Configurable general exact fallback between the supported high-performance general solvers.
- Dedicated adaptive-selection benchmarks and tests.

## [0.23.0]

### Added
- Ho-Chang-Solis net Least Period Cost heuristic.
- Ho-Chang-Solis improved nLPC(i) heuristic.
- McLaren Order Moment heuristic.
- Karni Maximum Part-Period Gain heuristic.
- Dedicated tests and benchmarks for the new literature heuristics.

### Changed
- Refactored the generated documentation portal and algorithm pages around the consolidated strategy inventory.

## [0.22.0]

### Added
- Part-Period Simplified heuristic.
- Segerstedt reformulated Silver-Meal heuristic.
- Chiu modified Least Unit Cost heuristic.
- Chiu-Ting modified Part-Period Balancing heuristic.
- Literature-oriented validation tests and per-method benchmarks.

### Changed
- Refined the classical Part-Period Balancing implementation and heuristic documentation.

## [0.21.0]

### Added
- Cutting-plane cut-selection policies: all violated, most violated per `l`, top by violation and top by efficacy.
- Root-LP convergence reporting and per-iteration statistics.
- Pure `(l,S)` separator benchmarks independent of the external optimization engine.

### Changed
- Extended cutting-plane execution reports while preserving exact final-MILP semantics.

## [0.20.0]

### Added
- Exact general `(l,S)` cutting-plane strategy.
- Wagner-Whitin `(l,S)` cutting-plane strategy.
- General and Wagner-Whitin separation engines, cut-model construction and traceable separated cuts.
- Solver results carrying cutting-plane execution information.

### Validation
- Added separator, model-builder and end-to-end cutting-plane solver tests.

## [0.19.0]

### Added
- `IAsyncUlsSolver` for non-blocking solver-backed strategies.
- Four exact formulation strategies: aggregate inventory, facility location, shortest path and inventory eliminated.
- Formulation-specific solution reconstruction and `SolverBackedUlsSolveResult`.
- Independent ULS solution validation after portable-model execution.

### Changed
- Promoted the v0.17 mathematical formulations to normal synchronous and asynchronous ULS Strategy implementations.

## [0.18.0]

### Added
- Generic `LinearModelSolver` execution layer.
- Portable LP writer and provider executors for CPLEX, Gurobi, Xpress and COIN-OR CBC.
- Solver-independent solution parsing, numerical normalization and independent model-solution validation.
- Reproducibility options for exported models and retained temporary solver artifacts.

### Validation
- Added focused tests for LP serialization, solution parsers, normalization, executor registry and independent model validation.

## [0.17.0]

### Added
- Portable `LinearModel` representation for variables, objectives, terms and constraints.
- Aggregate-inventory formulation builder.
- Facility-location formulation builder.
- Shortest-path formulation builder.
- Inventory-eliminated formulation builder.
- Formulation catalog, kinds and semantic variable maps.

### Validation
- Added formulation-structure and portable-model tests.

## [0.16.0]

### Added
- Concrete discovery adapters for IBM ILOG CPLEX, Gurobi, FICO Xpress and COIN-OR CBC.
- Built-in default adapter registry and high-level optimization-solver discovery.
- Runtime/version/license diagnostics without mandatory compile-time dependencies on commercial solver assemblies.

### Validation
- Added concrete adapter discovery tests.

## [0.15.0]

### Added
- Solver-agnostic optimization integration abstractions, solver selection and capability metadata.
- Cut-generation data model and traceability reports for future/existing cutting-plane work.
- Repository documentation portal, scientific-reference pages, algorithm catalog and documentation-link checks.
- Graphical identity/assets and a broader Doxygen documentation structure.

### Changed
- Consolidated earlier root-level installation/manifests into the repository structure and cleaned legacy bootstrap artifacts.

## [0.14.0]

### Added
- Saydam-McKnew fast Wagner-Whitin exact solver.
- Jacobs-Khumawala simplified branch-and-bound exact solver.
- Zangwill network/shortest-path exact solver.
- Shared regeneration-cost support, validation tests and benchmarks for the exact-algorithm pack.

### Infrastructure
- Added verified Graphviz installation support for documentation CI and corrected versioning/CI issues encountered during the release.

## [0.13.0]

### Added
- Freeland-Colley heuristic.
- Patterson-LaForge Incremental Part-Period Algorithm (IPPA).
- Wemmerlöv corrected PPB.
- Wemmerlöv PPB with look-ahead/look-back.
- Wemmerlöv corrected PPB with look-ahead/look-back.
- Dedicated tests, documentation and benchmarks for the second classical-heuristics pack.

## [0.12.0]

### Added
- Lot-for-Lot.
- Silver-Meal.
- Least Unit Cost.
- Part-Period Balancing.
- Groff.
- Periodic Order Quantity.
- Shared guards/builders, tests, documentation and benchmarks for the first classical-heuristics pack.

## [0.11.0]

### Added
- Chowdhury-Baki-Azab exact solver.
- Sadjadi-Aryanezhad-Sadeghi exact solver.
- Lyu-Lee parallel exact solver.
- Tests, documentation and benchmarks for the first multi-algorithm exact pack.

## [0.10.0]

### Added
- Heady-Zhu improved Wagner-Whitin implementation.
- Dedicated correctness tests, documentation and benchmarks.

## [0.9.0]

### Added
- Bahl-Taj data-dependent planning-horizon Wagner-Whitin implementation.
- Dedicated correctness tests, documentation and benchmarks.

## [0.8.0]

### Added
- Aggarwal-Park exact solver.
- Matrix-search support, scaling benchmarks, correctness tests and scientific documentation.

## [0.7.0]

### Added
- Federgruen-Tzur linear no-speculative-motive specialization.
- Federgruen-Tzur linear restricted nondecreasing-setup specialization.
- Shared linear candidate-deque/core implementation.
- Dedicated tests, benchmarks and documentation for both linear variants.

## [0.6.0]

### Added
- General Federgruen-Tzur exact solver.
- Tree-accelerated candidate data structure.
- Correctness/scaling tests, benchmarks and scientific documentation.

## [0.5.0]

### Added
- General Wagelmans exact solver.
- Correctness and scaling benchmarks.
- Dedicated tests and scientific documentation.

## [0.4.0]

### Added
- Classical quadratic Wagner-Whitin solver.
- Evans low-storage Wagner-Whitin implementation.
- Shared zero-inventory-order solution reconstruction.
- Family-level tests, benchmarks and documentation.

## [0.3.0]

### Added
- First production Wagner-Whitin exact implementation.
- Independent quadratic Wagner-Whitin oracle for cross-validation.
- Dedicated correctness tests, benchmarks and method documentation.

## [0.2.0]

### Added
- Common `IUlsSolver` Strategy contract and `UlsSolverKind`.
- Immutable `UlsProblem` model and validation.
- `UlsSolution`, `UlsSolveResult` and solve-status model.
- Core API unit tests.

## [0.1.0]

### Added
- Initial .NET repository/solution bootstrap for library, tests and benchmarks.
- Shared build metadata and Nerdbank.GitVersioning configuration.
- Validated build, documentation and GitHub release automation.
- Reproducible binary/documentation release assets with manifests and SHA-256 checksums.

