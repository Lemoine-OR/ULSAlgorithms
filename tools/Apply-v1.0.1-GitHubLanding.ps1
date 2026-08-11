Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$changelogPath = Join-Path $root 'CHANGELOG.md'

if (-not (Test-Path -LiteralPath $changelogPath -PathType Leaf)) {
    throw 'CHANGELOG.md is missing.'
}

$text = [IO.File]::ReadAllText($changelogPath)

$heading = '## [1.0.1] - 2026-08-11'
if ($text -match '(?m)^## \[1\.0\.1\]\s+-\s+2026-08-11\s*$') {
    Write-Host 'CHANGELOG.md already contains the v1.0.1 entry; no change required.'
    exit 0
}

$anchor = '## [1.0.0] - 2026-08-11'
$anchorCount = ([regex]::Matches($text, [regex]::Escape($anchor))).Count
if ($anchorCount -ne 1) {
    throw "Expected exactly one '$anchor' anchor, found $anchorCount."
}

$entry = @'
## [1.0.1] - 2026-08-11

### Changed
- Restored the GitHub repository README as a visual project landing page while preserving the stable v1.0 public API and implementation.
- Restored the ULSAlgorithms logo, build/documentation/release badges, quick project links and the four-family visual overview.
- Restored clickable method panels and updated them to the complete v1.0 catalog of 42 public strategies.
- Added stable solver IDs directly to the GitHub method panels so catalog/factory usage is visible from the repository home page.
- Kept the v1.0 factory, serializable configuration, external-solver, validation, distribution, citation and API-stability information on the landing page.

### Compatibility
- Documentation-only patch: no algorithm, public .NET API member, stable solver ID, numerical policy, optimization adapter or serialized configuration contract is changed.
- The stable v1.0 compatibility baseline remains unchanged.

'@

$text = $text.Replace($anchor, $entry + $anchor)
[IO.File]::WriteAllText(
    $changelogPath,
    $text,
    [Text.UTF8Encoding]::new($false))

Write-Host 'Inserted the v1.0.1 documentation-only changelog entry.'
