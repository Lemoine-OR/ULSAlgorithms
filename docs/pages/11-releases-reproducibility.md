\page releases_reproducibility Releases and Reproducibility

# Releases and Reproducibility

ULSAlgorithms uses repository-wide semantic versioning through
Nerdbank.GitVersioning. Version 1.0.0 establishes the stable 1.x compatibility
contract described in `API-STABILITY.md`.

## Continuous validation

Every push to `main` runs the validated build and test pipeline.

The repository-wide validation builds every discovered .NET project in Release
configuration and runs the complete unit-test suite on both Windows and Linux.
Linux additionally executes the portability smoke and a real COIN-OR CBC
end-to-end qualification of all six solver-backed public strategies.

Documentation is generated separately through Doxygen, validated and published
to GitHub Pages.

## Public release workflow

A public release reruns the pre-release qualification before publication and
records:

- release version;
- build version;
- Git commit;
- binary ZIP and SHA-256 sidecar;
- documentation ZIP and SHA-256 sidecar;
- NuGet package (`.nupkg`) and SHA-256 sidecar;
- NuGet portable-symbol package (`.snupkg`) and SHA-256 sidecar;
- build metadata;
- binary manifest;
- release manifest and SHA-256 sidecar.

The release workflow also validates:

- the public API compatibility baseline;
- runtime/documentation solver-catalog synchronization;
- official .NET package validation;
- the structure of the main NuGet package and symbol package;
- an isolated consumer that restores, compiles and executes against the exact
  generated `.nupkg`;
- the final release-manifest coverage of required assets.

The release workflow creates the Git tag. Tags should not be created manually.

## Stable 1.x contract

Starting with v1.0.0, existing public .NET contracts, stable solver IDs and
serialized configuration schema version 1 are compatibility commitments.

Compatible additions may be made in minor releases. Intentional incompatible
changes require a new major version.

## Documentation identity

The portal injects both the version and the short Git commit into the generated
site.

This makes a documentation snapshot traceable to the exact source revision
from which it was produced.

## Links

- [Latest GitHub release](https://github.com/Lemoine-OR/ULSAlgorithms/releases/latest)
- [All releases](https://github.com/Lemoine-OR/ULSAlgorithms/releases)
- [GitHub Actions](https://github.com/Lemoine-OR/ULSAlgorithms/actions)
