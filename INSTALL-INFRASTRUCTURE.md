# ULSAlgorithms GitHub infrastructure bootstrap

This bundle intentionally contains **no ULS implementation and no C# project**.
It installs the repository-level versioning, CI, documentation, packaging and release automation first.

## Install

1. Clone `https://github.com/Lemoine-OR/ULSAlgorithms` in Visual Studio.
2. Copy every file and folder from this bundle into the repository root.
3. Keep the existing `README.md` already created on GitHub.
4. Commit with:

   `Bootstrap GitHub CI, documentation and release infrastructure`

5. Push to `main`.

## Expected first run

Because there is intentionally no `.sln`, `.slnx` or `.csproj` yet:

- **Build and Test** validates the automation and reports bootstrap mode; it succeeds without compiling code.
- **Build Documentation** builds a Doxygen site from the README and can deploy it to GitHub Pages.
- **Create Release** must not be run yet; it refuses to publish while there is no buildable project.

## GitHub Pages

In the GitHub repository, open **Settings → Pages** and set **Build and deployment → Source** to **GitHub Actions**.

## Version policy

`version.json` starts at `0.1.0` and always uses three explicit SemVer components.
Examples:

- patch: `0.1.0` → `0.1.1`
- new compatible feature set: `0.1.x` → `0.2.0`
- stable public API: later move to `1.0.0`

The manual **Create Release** workflow builds and validates everything before creating the Git tag and GitHub Release.
