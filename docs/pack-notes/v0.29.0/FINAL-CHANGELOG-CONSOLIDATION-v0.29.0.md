# v0.29.0 — final changelog consolidation

## Purpose

`CHANGELOG.md` was introduced only in v0.28.0. Its initial historical section
summarized earlier 0.x development in a single paragraph, which is not adequate
for the final pre-1.0 qualification record.

This consolidation reconstructs one explicit entry for every public tag from
v0.1.0 through v0.28.0 using the immutable Git tag history and tag-to-tag
repository differences. It also moves the fully qualified v0.29.0 work out of
`Unreleased` into the release-ready `0.29.0` section dated 2026-08-11.

## Historical reconstruction

The reconstructed history records the principal public additions introduced by
each tagged release:

- repository/bootstrap and common API;
- exact Wagner-Whitin family and high-performance exact methods;
- classical and literature heuristics;
- optimization discovery/execution;
- mathematical formulations;
- `(l,S)` cutting planes and engineering;
- adaptive selection, catalog/factory and serializable configuration;
- pre-1.0 API/package/release hardening.

The reconstruction intentionally summarizes released repository deltas rather
than inventing release notes that did not exist at the time.

## Regression protection

`tools/Test-ChangelogHistory.ps1` is added to the automation preflight.

It requires:

- an `Unreleased` section;
- exactly one explicit entry for every version `0.1.0` through `0.29.0`;
- descending release order;
- the release-ready `0.29.0` date;
- no return of the generic `Earlier 0.x releases` placeholder.

This prevents future overlays from silently collapsing the historical release
record again.

## Product compatibility

This block changes no product code, solver ID, API member, algorithm,
formulation, heuristic, solver integration or package format.
