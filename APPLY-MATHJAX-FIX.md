# ULSAlgorithms documentation formula fix

Cause:
Doxygen generated references to `form_*.png` for LaTeX formulas, but the
Windows documentation environment does not include the complete LaTeX /
Ghostscript formula-rendering toolchain. The strict local-link validator
therefore correctly reported the missing formula assets.

Fix:
`docs/Doxyfile` now enables the official Doxygen MathJax path:

- `USE_MATHJAX = YES`
- `MATHJAX_VERSION = MathJax_3`
- `MATHJAX_FORMAT = SVG`
- `MATHJAX_RELPATH = https://cdn.jsdelivr.net/npm/mathjax@3`

The documentation link validator remains strict; no `form_*.png` exception is
added.

Apply this overlay at the repository root, then rebuild documentation:

powershell.exe -ExecutionPolicy Bypass -File ".\docs\build-documentation.ps1"

Expected final result:
- no missing `form_*.png` links;
- documentation link validation passes;
- formulas render as scalable MathJax SVG in the browser.

Do not commit until the generated portal has been visually checked.
