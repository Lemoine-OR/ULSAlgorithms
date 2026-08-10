param(
    [Parameter(Mandatory = $false)]
    [string]$ReleaseDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')
if ([string]::IsNullOrWhiteSpace($ReleaseDirectory)) {
    $ReleaseDirectory = Join-Path $root 'Documentation\release'
}

if (-not (Test-Path -LiteralPath $ReleaseDirectory)) {
    throw "Release directory does not exist: $ReleaseDirectory"
}

$version = & (Join-Path $PSScriptRoot 'Get-ULSAlgorithmsVersion.ps1')
$v = $version.PackageVersion
$required = @(
    "ULSAlgorithms-$v-binaries.zip",
    "ULSAlgorithms-$v-binaries.zip.sha256",
    "ULSAlgorithms-$v-documentation.zip",
    "ULSAlgorithms-$v-documentation.zip.sha256",
    "ULSAlgorithms.$v.nupkg",
    "ULSAlgorithms.$v.nupkg.sha256",
    'build-metadata.json',
    'binaries-manifest.json',
    'release-manifest.json',
    'release-manifest.json.sha256'
)

foreach ($name in $required) {
    $path = Join-Path $ReleaseDirectory $name
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing release asset: $name"
    }
    if ((Get-Item -LiteralPath $path).Length -eq 0) {
        throw "Release asset is empty: $name"
    }
}

$shaFiles = @(Get-ChildItem -LiteralPath $ReleaseDirectory -File -Filter '*.sha256')
foreach ($shaFile in $shaFiles) {
    $line = (Get-Content -LiteralPath $shaFile.FullName -Raw).Trim()
    if ($line -notmatch '^([0-9a-fA-F]{64})\s+\*?(.+)$') {
        throw "Invalid SHA-256 file format: $($shaFile.Name)"
    }
    $expected = $matches[1].ToLowerInvariant()
    $targetName = $matches[2].Trim()
    $targetPath = Join-Path $ReleaseDirectory $targetName
    if (-not (Test-Path -LiteralPath $targetPath)) {
        throw "SHA-256 sidecar references a missing file: $targetName"
    }
    $actual = (Get-FileHash -LiteralPath $targetPath -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actual -ne $expected) {
        throw "SHA-256 mismatch for $targetName. Expected $expected, got $actual."
    }
}

& (Join-Path $PSScriptRoot 'Test-NuGetPackage.ps1') `
    -PackagePath (Join-Path $ReleaseDirectory "ULSAlgorithms.$v.nupkg")

$manifest = Get-Content -LiteralPath (Join-Path $ReleaseDirectory 'release-manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$manifestNames = @($manifest.assets | ForEach-Object { [string]$_.name })
foreach ($name in $required | Where-Object { $_ -notin @('release-manifest.json','release-manifest.json.sha256') }) {
    if ($manifestNames -notcontains $name) {
        throw "Release manifest does not record required asset: $name"
    }
}

Write-Host 'Release artifact validation passed.'
