Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')

& (Join-Path $PSScriptRoot 'Test-PowerShellSyntax.ps1')
& (Join-Path $PSScriptRoot 'Test-VersioningPreflight.ps1')

$required = @(
    '.github/workflows/build.yml',
    '.github/workflows/documentation.yml',
    '.github/workflows/release.yml',
    'build/Build-All.ps1',
    'build/Build-Validated.ps1',
    'build/Package-ValidatedBinaries.ps1',
    'build/Prepare-ReleaseAssets.ps1',
    'docs/Doxyfile',
    'docs/build-documentation.ps1',
    'tools/Install-Doxygen.ps1',
    'tools/Get-ULSAlgorithmsVersion.ps1',
    'tools/Test-ReleaseArtifacts.ps1'
)

foreach ($relative in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $relative))) {
        throw "Required automation file is missing: $relative"
    }
}

$legacyProductName = 'LotSizing' + 'DataModel'
$stale = Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '[\\/]\.git[\\/]' -and $_.Extension -in @('.ps1','.yml','.yaml','.props','.targets','.json') } |
    Select-String -Pattern $legacyProductName -SimpleMatch -ErrorAction SilentlyContinue
if ($stale) {
    $stale | ForEach-Object { Write-Error "Stale legacy-product reference: $($_.Path):$($_.LineNumber): $($_.Line.Trim())" }
    throw 'Automation still contains references to the previous repository product name.'
}

$target = & (Join-Path $PSScriptRoot 'Get-BuildTarget.ps1')
if ($null -eq $target) {
    Write-Host 'Bootstrap mode: repository automation is valid; no C# solution/project exists yet.'
}
else {
    Write-Host "Build target detected: $($target.Path)"
    $version = & (Join-Path $PSScriptRoot 'Get-ULSAlgorithmsVersion.ps1')
    Write-Host "NBGV package version: $($version.PackageVersion)"
}

Write-Host 'Automation preflight passed.'
