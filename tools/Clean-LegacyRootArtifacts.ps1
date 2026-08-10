[CmdletBinding(SupportsShouldProcess = $true)]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

$legacyFiles = @(
    "AGGARWAL-PARK-MANIFEST.sha256",
    "APPLY-CI-FIX.md",
    "APPLY-COMPILE-FIX.md",
    "APPLY-GRAPHVIZ-CI-FIX.md",
    "APPLY-VERSION-JSON-FIX.md",
    "BAHL-TAJ-MANIFEST.sha256",
    "BUNDLE-MANIFEST.sha256",
    "CLASSICAL-HEURISTICS-PACK-I-MANIFEST.sha256",
    "CLASSICAL-HEURISTICS-PACK-II-MANIFEST.sha256",
    "CODE-BOOTSTRAP-MANIFEST.sha256",
    "CORE-API-MANIFEST.sha256",
    "EXACT-ALGORITHMS-PACK-II-MANIFEST.sha256",
    "EXACT-PACK-I-MANIFEST.sha256",
    "FEDERGRUEN-TZUR-LINEAR-MANIFEST.sha256",
    "FEDERGRUEN-TZUR-MANIFEST.sha256",
    "GRAPHVIZ-CI-FIX-MANIFEST.sha256",
    "HEADY-ZHU-MANIFEST.sha256",
    "INSTALL-AGGARWAL-PARK.md",
    "INSTALL-BAHL-TAJ.md",
    "INSTALL-CLASSICAL-HEURISTICS-PACK-I.md",
    "INSTALL-CLASSICAL-HEURISTICS-PACK-II.md",
    "INSTALL-CODE-BOOTSTRAP.md",
    "INSTALL-EXACT-ALGORITHMS-PACK-II.md",
    "INSTALL-EXACT-PACK-I.md",
    "INSTALL-FEDERGRUEN-TZUR-LINEAR.md",
    "INSTALL-FEDERGRUEN-TZUR.md",
    "INSTALL-HEADY-ZHU.md",
    "INSTALL-INFRASTRUCTURE.md",
    "INSTALL-WAGELMANS-GENERAL.md",
    "INSTALL-WAGNER-WHITIN.md",
    "INSTALL-WW-CLASSICAL-EVANS.md",
    "WAGELMANS-GENERAL-MANIFEST.sha256",
    "WAGNER-WHITIN-MANIFEST.sha256",
    "WW-CLASSICAL-EVANS-MANIFEST.sha256"
)

$removed = @()

foreach ($relative in $legacyFiles) {
    $path = Join-Path $root $relative

    if (Test-Path -LiteralPath $path -PathType Leaf) {
        if ($PSCmdlet.ShouldProcess($path, "Remove legacy overlay artifact")) {
            Remove-Item -LiteralPath $path -Force
            $removed += $relative
        }
    }
}

Write-Host ""
Write-Host "Legacy root cleanup complete."
Write-Host "Removed $($removed.Count) file(s)."
$removed | ForEach-Object { Write-Host " - $_" }
