# ULSAlgorithms documentation UTF-8 fix

Cause:
Windows PowerShell 5.1 uses the legacy Windows code page for `Get-Content`
unless an encoding is explicitly supplied. The documentation sources are UTF-8.

As a result, characters such as:
- en dash: `–`
- em dash: `—`
- middle dot: `·`
- arrow: `→`
- `ö`
could be decoded as mojibake before the generated portal was written back as
UTF-8.

Fix:
`docs/build-documentation.ps1` now uses `-Encoding UTF8` when reading:
- `docs/algorithm-catalog.json`
- `docs/Doxyfile`
- `docs/portal/index.html`

The generated output continues to be written in UTF-8.

Apply at repository root, then rebuild documentation:

powershell.exe -ExecutionPolicy Bypass -File ".\docs\build-documentation.ps1"

No algorithm code, version file, tests or release workflow is changed.
