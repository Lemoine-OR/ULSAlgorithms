param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$path = (Resolve-Path -LiteralPath $PackagePath).Path
if ((Get-Item -LiteralPath $path).Length -eq 0) {
    throw "NuGet package is empty: $path"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($path)

try {
    $entries = @($archive.Entries | ForEach-Object { $_.FullName })

    $required = @(
        'lib/net10.0/ULSAlgorithms.dll',
        'lib/net10.0/ULSAlgorithms.xml',
        'README.md',
        'LICENSE',
        'CITATION.cff'
    )

    foreach ($entry in $required) {
        if ($entries -notcontains $entry) {
            throw "NuGet package is missing required entry: $entry"
        }
    }

    if ($entries -contains 'lib/net10.0/ULSAlgorithms.pdb') {
        throw 'Main .nupkg unexpectedly contains the portable PDB; symbols must be in the .snupkg.'
    }

    $nuspecEntries = @($archive.Entries | Where-Object { $_.FullName -like '*.nuspec' })
    if ($nuspecEntries.Count -ne 1) {
        throw "Expected exactly one nuspec in package, found $($nuspecEntries.Count)."
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
    $licenseNode = $nuspec.SelectSingleNode(
        '/*[local-name()="package"]/*[local-name()="metadata"]/*[local-name()="license"]')
    $readmeNode = $nuspec.SelectSingleNode(
        '/*[local-name()="package"]/*[local-name()="metadata"]/*[local-name()="readme"]')
    $repositoryNode = $nuspec.SelectSingleNode(
        '/*[local-name()="package"]/*[local-name()="metadata"]/*[local-name()="repository"]')

    if ($null -eq $idNode -or $idNode.InnerText -ne 'ULSAlgorithms') {
        throw 'Unexpected or missing NuGet package id.'
    }

    if ($null -eq $licenseNode -or
        $licenseNode.InnerText -ne 'MIT' -or
        $licenseNode.GetAttribute('type') -ne 'expression') {
        throw 'NuGet package does not declare MIT as a license expression.'
    }

    if ($null -eq $readmeNode -or $readmeNode.InnerText -ne 'README.md') {
        throw 'NuGet package does not declare README.md as its package readme.'
    }

    if ($null -eq $repositoryNode -or
        $repositoryNode.GetAttribute('url') -ne 'https://github.com/Lemoine-OR/ULSAlgorithms' -or
        $repositoryNode.GetAttribute('type') -ne 'git') {
        throw 'NuGet package does not declare the expected git repository metadata.'
    }
}
finally {
    $archive.Dispose()
}

Write-Host "NuGet package validation passed: $path"
