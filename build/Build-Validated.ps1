Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$tools = Join-Path $root 'tools'

& (Join-Path $tools 'Test-Automation.ps1')
$build = & (Join-Path $PSScriptRoot 'Build-All.ps1')

if ($build.Bootstrap) {
    Write-Host 'Bootstrap validation completed successfully. No binaries are expected yet.'
    return
}

$package = & (Join-Path $PSScriptRoot 'Package-ValidatedBinaries.ps1')
if ($null -eq $package) {
    throw 'Validated binary packaging unexpectedly returned no result.'
}

Write-Host "Validated binary package: $($package.BinaryZip)"
