param(
    [Parameter(Mandatory = $false)]
    [string]$ArtifactsRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')
if ([string]::IsNullOrWhiteSpace($ArtifactsRoot)) {
    $ArtifactsRoot = Join-Path $root 'Documentation\artifacts\validated'
}

if (-not (Test-Path -LiteralPath $ArtifactsRoot)) {
    throw "Validated artifact directory does not exist: $ArtifactsRoot"
}

$dlls = @(Get-ChildItem -LiteralPath $ArtifactsRoot -Recurse -File -Filter '*.dll' | Where-Object { $_.Name -like 'ULSAlgorithms*.dll' })
if ($dlls.Count -eq 0) {
    throw 'No ULSAlgorithms*.dll assembly was found in validated artifacts.'
}

foreach ($dll in $dlls) {
    $assemblyName = [System.Reflection.AssemblyName]::GetAssemblyName($dll.FullName)
    if ($null -eq $assemblyName.Version) {
        throw "Assembly version is missing: $($dll.FullName)"
    }
    $info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll.FullName)
    if ([string]::IsNullOrWhiteSpace($info.ProductVersion)) {
        throw "ProductVersion is missing: $($dll.FullName)"
    }
    Write-Host "Validated metadata: $($dll.Name) | AssemblyVersion=$($assemblyName.Version) | ProductVersion=$($info.ProductVersion)"
}
