Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')
$versionPath = Join-Path $root 'version.json'
$propsPath = Join-Path $root 'Directory.Build.props'
$targetsPath = Join-Path $root 'Directory.Build.targets'

foreach ($path in @($versionPath, $propsPath, $targetsPath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Required versioning file is missing: $path"
    }
}

$config = Get-Content -LiteralPath $versionPath -Raw | ConvertFrom-Json
$version = [string]$config.version
if ($version -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') {
    throw "Explicit three-component SemVer required in version.json; found '$version'."
}

$props = Get-Content -LiteralPath $propsPath -Raw
if ($props -notmatch 'Nerdbank\.GitVersioning') {
    throw 'Directory.Build.props does not reference Nerdbank.GitVersioning.'
}
if ($props -notmatch 'https://github\.com/Lemoine-OR/ULSAlgorithms') {
    throw 'Directory.Build.props does not contain the expected repository URL.'
}

$targets = Get-Content -LiteralPath $targetsPath -Raw
if ($targets -notmatch 'WriteULSAlgorithmsVersion') {
    throw 'Directory.Build.targets does not expose WriteULSAlgorithmsVersion.'
}

Write-Host "Versioning preflight passed. Declared version: $version"
