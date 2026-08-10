# ULSAlgorithms API Stability Policy

## Scope

ULSAlgorithms 1.0.0 marks the beginning of the stable public API.

The compatibility contract covers:

- exported public .NET types and members in the `ULSAlgorithms` assembly;
- the `IUlsSolver` contract and result model;
- the stable solver identifiers published by `UlsSolverCatalog`;
- public configuration schema version 1 for `UlsSolverConfiguration`.

## Semantic versioning after 1.0.0

### Major version
A major version is required for an intentional incompatible change, including:

- removing or renaming a public type or member;
- changing a public member signature incompatibly;
- removing or renaming an existing stable solver ID;
- changing the meaning of an existing serialized configuration field
  incompatibly;
- dropping a previously supported configuration schema without a migration
  path.

### Minor version
A minor version may add compatible functionality, including:

- new public algorithms and new stable solver IDs;
- new optional members;
- new optional configuration fields with backward-compatible defaults;
- new solver adapters;
- new documentation and diagnostics.

### Patch version
A patch version contains compatible corrections, numerical fixes, performance
improvements, documentation corrections and release-engineering changes.

## Automated compatibility baseline

`eng/public-api/ULSAlgorithms.PublicApi.txt` is generated from the public
assembly and the runtime solver catalog.

The validator requires every baseline entry to remain present. New public
members are allowed; removal or signature replacement of a baseline entry
fails validation.

Update the baseline only for an intentional compatibility decision:

```powershell
.\tools\Update-PublicApiSnapshot.ps1
.\tools\Test-PublicApi.ps1
```

A baseline update must be reviewed as an API change, not as routine generated
output.

## Serialized configuration compatibility

`UlsSolverConfiguration.SchemaVersion` is independent from the package version.

Schema version 1 is the first stable JSON schema. Readers reject unknown schema
versions rather than guessing their meaning. Compatible optional fields may be
added to a schema only when older configurations keep the same behavior.

## Scientific strategy IDs

Stable IDs are intended for source code, configuration files, experiment
descriptions and reproducibility artifacts. An ID is therefore part of the
compatibility contract once published.
