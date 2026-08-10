\page releases_reproducibility Releases and Reproducibility

# Releases and Reproducibility

ULSAlgorithms uses repository-wide semantic versioning through Nerdbank.GitVersioning.

## Continuous validation

Every push to `main` runs the validated build and test pipeline.

Documentation is generated separately through Doxygen and published to GitHub Pages.

## Public release workflow

A public release reruns validation before publication and records:

- release version;
- build version;
- Git commit;
- binary ZIP;
- documentation ZIP;
- build metadata;
- release manifest;
- SHA-256 checksums.

The release workflow creates the Git tag. Tags should not be created manually.

## Documentation identity

The portal injects both the version and the short Git commit into the generated site.

This makes a documentation snapshot traceable to the exact source revision from which it was produced.

## Links

- [Latest GitHub release](https://github.com/Lemoine-OR/ULSAlgorithms/releases/latest)
- [All releases](https://github.com/Lemoine-OR/ULSAlgorithms/releases)
- [GitHub Actions](https://github.com/Lemoine-OR/ULSAlgorithms/actions)
