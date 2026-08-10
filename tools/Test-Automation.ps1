Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')

& (Join-Path $PSScriptRoot 'Test-PowerShellSyntax.ps1')
& (Join-Path $PSScriptRoot 'Test-VersioningPreflight.ps1')

$required = @(
    '.github/workflows/build.yml',
    '.github/workflows/documentation.yml',
    '.github/workflows/release.yml',
    'API-STABILITY.md',
    'CHANGELOG.md',
    'LICENSE',
    'README.md',
    'build/Build-All.ps1',
    'build/Build-Validated.ps1',
    'build/Package-NuGet.ps1',
    'build/Package-ValidatedBinaries.ps1',
    'build/Prepare-ReleaseAssets.ps1',
    'docs/Doxyfile',
    'docs/algorithm-catalog.json',
    'docs/build-documentation.ps1',
    'eng/public-api/ULSAlgorithms.PublicApi.txt',
    'tools/Install-Doxygen.ps1',
    'tools/Get-ULSAlgorithmsVersion.ps1',
    'tools/Test-DocumentationGeneratorHardening.ps1',
    'tools/Test-NuGetPackage.ps1',
    'tools/Test-PublicApi.ps1',
    'tools/Update-PublicApiSnapshot.ps1',
    'tools/Test-ReleaseArtifacts.ps1',
    'tools/Test-SolverCatalog.ps1',
    'tools/ULSAlgorithms.CatalogExporter/ULSAlgorithms.CatalogExporter.csproj',
    'tools/ULSAlgorithms.PublicApiExporter/ULSAlgorithms.PublicApiExporter.csproj',
    'tools/ULSAlgorithms.PortabilitySmoke/ULSAlgorithms.PortabilitySmoke.csproj'
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

& (Join-Path $PSScriptRoot 'Test-DocumentationGeneratorHardening.ps1')

$target = & (Join-Path $PSScriptRoot 'Get-BuildTarget.ps1')
if ($null -eq $target) {
    Write-Host 'Bootstrap mode: repository automation is valid; no C# solution/project exists yet.'
}
else {
    Write-Host "Build target detected: $($target.Path)"
    $version = & (Join-Path $PSScriptRoot 'Get-ULSAlgorithmsVersion.ps1')
    Write-Host "NBGV package version: $($version.PackageVersion)"

    & (Join-Path $PSScriptRoot 'Test-SolverCatalog.ps1')
    & (Join-Path $PSScriptRoot 'Test-PublicApi.ps1')
}

Write-Host 'Automation preflight passed.'
