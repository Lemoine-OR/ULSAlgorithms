# Documentation refactor — generated Doxygen pages fix

This patch fixes the missing generated algorithm pages reported after the
post-v0.22.0 documentation refactor.

## Root cause

`docs/Doxyfile` deliberately excludes `*/Documentation/*` to keep build output
out of Doxygen input discovery.  The refactored build script generated the
catalog and 37 per-algorithm Markdown pages under `Documentation/generated` and
then passed that directory as a Doxygen `INPUT` directory.  Doxygen therefore
excluded the generated sources.

## Fix

`docs/build-documentation.ps1` now enumerates every generated `.md` file and
passes each one explicitly in `INPUT`, preserving the existing Doxyfile
exclusion for all other build artifacts.

The script also validates that exactly `algorithm count + 1` generated Markdown
sources exist before invoking Doxygen.

No C# code, algorithm behavior, public API or version metadata is changed.
