# v0.29.0 — repository-wide build and Linux validation

This qualification block closes the build-completeness gap identified before
the 1.0 API freeze.

## Problem

The Visual Studio solution intentionally contains the main library, tests,
benchmarks and catalog exporter, but several executable tooling projects are
outside the solution. Historically `Build-All.ps1` built the solution and then
ran test projects. A newly added or modified tool project could therefore fail
to compile without breaking the normal validated build.

## Qualification change

`Get-DotNetProjects.ps1` discovers every repository `.csproj` while excluding
generated/build-output directories.

`Build-All.ps1` now:

1. restores and builds the primary solution;
2. restores and builds every discovered project explicitly in Release;
3. runs every project under `tests` with `--no-build --no-restore`;
4. returns both discovered-project and test-project counts.

The build is deliberately redundant for solution-contained projects. With the
current small project set this is inexpensive and makes project coverage
obvious and future-proof.

## Cross-platform gate

The Linux CI job now runs the same repository-wide build and complete unit-test
suite before the adaptive portability smoke. The pre-release workflow requires
that Linux validation and the independent real-CBC integration job both pass
before the Windows release job can begin.

No public solver ID, optimization algorithm or public API contract is changed.
