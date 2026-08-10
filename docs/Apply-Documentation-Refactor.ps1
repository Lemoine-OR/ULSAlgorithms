[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

$obsoletePages = @(
    "docs/pages/classical-heuristics-pack-v012.md",
    "docs/pages/classical-heuristics-pack-v013.md",
    "docs/pages/exact-pack-v011.md",
    "docs/pages/exact-algorithms-pack-v014.md",
    "docs/pages/21-literature-heuristics-pack-v022.md"
)

Write-Host "Removing obsolete release-pack pages from user documentation..."
foreach ($relative in $obsoletePages) {
    $path = Join-Path $RepoRoot $relative
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
        Write-Host "  removed $relative"
    } else {
        Write-Host "  already absent $relative"
    }
}

Write-Host ""
Write-Host "Documentation refactor cleanup complete." -ForegroundColor Green
Write-Host "Release provenance under docs/pack-notes is intentionally retained; it is not part of the generated user navigation."
