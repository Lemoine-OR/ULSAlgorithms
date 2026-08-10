# ULSAlgorithms v0.28.0 — 1.0 Readiness and Release Hardening

Base: v0.27.0 / commit `0f0f701fbf72ccdc075c8486e1a549a1ae8a4500`

## Purpose

v0.28.0 is the final planned pre-1.0 hardening release.

It adds no algorithm and does not change the 42 public strategy IDs. Its role
is to make the existing library suitable for a stable 1.x compatibility
promise.

## Serializable solver configuration

New public type:

`ULSAlgorithms.Catalog.UlsSolverConfiguration`

Schema version 1 stores:

- `solverId`;
- `UlsSolverCreationOptions`;
- adaptive fallback;
- Lyu-Lee parallel controls;
- nested `LinearModelSolveOptions`;
- nested `LsCuttingPlaneOptions`.

The JSON reader is strict:

- unknown schema versions are rejected;
- unknown fields are rejected;
- integer enum encodings are rejected;
- unknown solver IDs are rejected;
- irrelevant/incompatible options are rejected through the same capability
  validation as the runtime factory.

`UlsSolverFactory.Create(UlsSolverConfiguration)` is the common construction
entry point.

## Public API compatibility baseline

A new reflection-based exporter records:

- every exported public type;
- public constructors;
- public properties;
- public fields/constants;
- public events;
- public methods;
- enum values;
- all stable runtime solver IDs.

The committed baseline is a minimum compatibility contract: additions are
allowed, while the disappearance/replacement of an existing baseline contract
fails validation.

The first baseline is generated locally after applying this pack:

```powershell
.\tools\Update-PublicApiSnapshot.ps1
.\tools\Test-PublicApi.ps1
```

The generated file must be reviewed and committed.

## Documentation hardening

The v0.27 documentation run exposed warnings that did not fail CI because
Doxygen had `WARN_AS_ERROR = NO`.

v0.28.0:

- fixes Doxygen inline mathematical notation;
- removes duplicate page/section labels;
- removes the accidental `\S` command in the general separator comment;
- upgrades generated algorithm references to fully qualified class names;
- changes Doxygen to `WARN_AS_ERROR = FAIL_ON_WARNINGS`.

The two documentation-generator substitutions are intentionally applied by
the guarded one-time script:

```powershell
.\tools\Apply-v0.28.0-DocumentationHardening.ps1
```

The automation preflight rejects a repository in which that migration has not
been applied.

## Scientific metadata

The runtime catalog continues to require provenance for all 42 public
strategies.

The Aggarwal-Park entry is corrected to:

A. Aggarwal and J. K. Park (1993),
"Improved Algorithms for Economic Lot Size Problems",
Operations Research 41(3), 549-571,
DOI `10.1287/opre.41.3.549`.

Tests validate normalized DOI metadata when a DOI is recorded.

## MIT license

The repository and package are released under the MIT License.

## NuGet package

The library project now records:

- MIT license expression;
- package README;
- project repository metadata inherited from `Directory.Build.props`;
- ULS/operations-research package tags.

`build/Package-NuGet.ps1` creates and validates:

`ULSAlgorithms.<version>.nupkg`

The validator checks the assembly, XML documentation, README, LICENSE and
nuspec metadata. The release adds the package and its SHA-256 sidecar to the
release manifest.

## Portability

The existing Windows validation remains authoritative for the complete release
pipeline and external-solver integration.

A second GitHub Actions job runs a .NET 10 Linux compile/runtime smoke using:

- versioned JSON configuration;
- `adaptive-exact`;
- a self-contained exact solve without an external optimizer.

This prevents accidental Windows-only dependencies from entering the core
library path.

## Coverage reporting

The test project adds `coverlet.collector` and the Windows CI job emits an
`XPlat Code Coverage` artifact after the normal validated test run.

Coverage is reported for auditability but has no arbitrary percentage gate:
scientific exactness remains based on independent oracles, cross-validation
and feasibility/objective reconstruction rather than a line-coverage target.

## Policy files

Added:

- `CHANGELOG.md`
- `API-STABILITY.md`
- `LICENSE`

From 1.0.0 onward, public API compatibility and stable solver IDs follow
Semantic Versioning.

## Expected local validation sequence

1. Extract overlay.
2. Apply the guarded documentation-generator migration.
3. Generate and review the initial public API baseline.
4. Rebuild solution.
5. Run all tests.
6. Run `Test-SolverCatalog.ps1`.
7. Run `Test-PublicApi.ps1`.
8. Run `Test-PowerShellSyntax.ps1`.
9. Build documentation with zero warnings.
10. Run `Build-Validated.ps1` to validate binary and NuGet packaging.
11. Commit and push only when all checks are green.

## Compatibility

No existing solver ID changes.
No existing concrete solver constructor is removed.
No existing `IUlsSolver` signature is intentionally changed.
No default exact-selection policy changes.
No benchmark crossover policy changes.
No algorithm count changes: 42.
