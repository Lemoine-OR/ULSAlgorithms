# v0.29.0 — final API / NuGet / package hardening

## Purpose

This is the final qualification block planned before publishing v0.29.0 and
performing the short 1.0 freeze audit.

It supplements the repository's existing public-API baseline with the official
.NET package validator, proves that the generated NuGet package is consumable by
a clean client project, adds modern symbol packaging, and adds machine-readable
citation metadata.

## Official .NET package validation

`ULSAlgorithms.csproj` now enables:

```xml
<EnablePackageValidation>true</EnablePackageValidation>
```

This runs the .NET SDK package-validation task after `Pack`.

The repository's existing 1089-contract public API snapshot remains the
pre-1.0 breaking-change gate. `PackageValidationBaselineVersion` is
intentionally not set in v0.29.0: after the stable 1.0.0 package is published
to a package feed, 1.0.0 can become the official package baseline for later 1.x
releases.

## Real NuGet consumer smoke

Archive inspection alone cannot prove that a package is actually consumable.

`Test-NuGetConsumer.ps1` therefore:

1. creates an isolated temporary `net10.0` console project;
2. references the exact generated ULSAlgorithms package version;
3. restores from the local package directory;
4. compiles against the package, not the repository project;
5. runs `adaptive-exact`;
6. requires `Optimal` and the deterministic objective 680;
7. deletes the temporary project.

The smoke is executed from `Package-NuGet.ps1`, so both normal validated builds
and release preparation must pass it.

## Source Link and portable symbols

Modern .NET SDKs include Source Link build tooling. The product project now
publishes repository metadata and produces the recommended `.snupkg` format:

```xml
<PublishRepositoryUrl>true</PublishRepositoryUrl>
<EmbedUntrackedSources>true</EmbedUntrackedSources>
<IncludeSymbols>true</IncludeSymbols>
<SymbolPackageFormat>snupkg</SymbolPackageFormat>
```

The symbol package is validated independently and becomes a first-class release
asset with its own SHA-256 sidecar and release-manifest entry.

No extra Source Link NuGet dependency is added to the product.

## Citation

`CITATION.cff` records stable software-level citation metadata without embedding
a release version that would become stale on every tag. Automation validates
the required CFF fields, and the file is included in the main `.nupkg`.

## Release artifacts

A validated release now requires both:

```text
ULSAlgorithms.<version>.nupkg
ULSAlgorithms.<version>.snupkg
```

and SHA-256 sidecars for both.

## Compatibility

This block does not change:

- any stable solver ID;
- any public C# type or member;
- any algorithm or heuristic;
- any mathematical formulation;
- any solver-selection behavior.

It only strengthens packaging, consumption, debugging/citation metadata and
release validation.
