param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,

    [Parameter(Mandatory = $false)]
    [string]$PackageVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$path = (Resolve-Path -LiteralPath $PackagePath).Path
if ((Get-Item -LiteralPath $path).Length -eq 0) {
    throw "NuGet symbol package is empty: $path"
}

if ([IO.Path]::GetExtension($path) -ne '.snupkg') {
    throw "Expected a .snupkg symbol package: $path"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($path)

try {
    $entries = @($archive.Entries | ForEach-Object { $_.FullName })

    if ($entries -notcontains 'lib/net10.0/ULSAlgorithms.pdb') {
        throw 'NuGet symbol package is missing lib/net10.0/ULSAlgorithms.pdb.'
    }

    if ($entries -contains 'lib/net10.0/ULSAlgorithms.dll') {
        throw 'The .snupkg unexpectedly contains the product DLL instead of symbols only.'
    }

    $nuspecEntries = @(
        $archive.Entries |
        Where-Object { $_.FullName -like '*.nuspec' }
    )

    if ($nuspecEntries.Count -ne 1) {
        throw "Expected exactly one nuspec in symbol package, found $($nuspecEntries.Count)."
    }

    $reader = New-Object IO.StreamReader($nuspecEntries[0].Open())
    try {
        $nuspecText = $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
    }

    [xml]$nuspec = $nuspecText

    $idNode = $nuspec.SelectSingleNode(
        '/*[local-name()="package"]/*[local-name()="metadata"]/*[local-name()="id"]')
    $versionNode = $nuspec.SelectSingleNode(
        '/*[local-name()="package"]/*[local-name()="metadata"]/*[local-name()="version"]')
    $packageTypeNode = $nuspec.SelectSingleNode(
        '/*[local-name()="package"]/*[local-name()="metadata"]/*[local-name()="packageTypes"]/*[local-name()="packageType"]')

    if ($null -eq $idNode -or $idNode.InnerText -ne 'ULSAlgorithms') {
        throw 'Unexpected or missing symbol-package id.'
    }

    if (-not [string]::IsNullOrWhiteSpace($PackageVersion) -and
        ($null -eq $versionNode -or $versionNode.InnerText -ne $PackageVersion)) {
        throw "Unexpected symbol-package version. Expected '$PackageVersion'."
    }

    if ($null -eq $packageTypeNode -or
        $packageTypeNode.GetAttribute('name') -ne 'SymbolsPackage') {
        throw 'The .snupkg does not declare package type SymbolsPackage.'
    }
}
finally {
    $archive.Dispose()
}

Write-Host "NuGet symbol package validation passed: $path"
