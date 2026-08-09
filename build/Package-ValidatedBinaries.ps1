Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$tools = Join-Path $root 'tools'
$target = & (Join-Path $tools 'Get-BuildTarget.ps1')

if ($null -eq $target) {
    Write-Host 'Bootstrap mode: binary packaging skipped because no source project exists yet.'
    return $null
}

$versionInfo = & (Join-Path $tools 'Get-ULSAlgorithmsVersion.ps1')
$version = $versionInfo.PackageVersion
if ([string]::IsNullOrWhiteSpace($version)) {
    throw 'PackageVersion is empty.'
}

$artifactRoot = Join-Path $root 'Documentation\artifacts'
$validatedRoot = Join-Path $artifactRoot 'validated'

Remove-Item -LiteralPath $validatedRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $validatedRoot -Force | Out-Null

$sourceProjects = @(
    Get-ChildItem -LiteralPath (Join-Path $root 'src') -Recurse -File -Filter '*.csproj' -ErrorAction SilentlyContinue |
        Sort-Object FullName
)

if ($sourceProjects.Count -eq 0) {
    throw 'No source .csproj files were found under src/.'
}

$manifestEntries = [System.Collections.Generic.List[object]]::new()

foreach ($project in $sourceProjects) {
    $projectName = $project.BaseName
    $releaseRoot = Join-Path $project.Directory.FullName 'bin\Release'

    if (-not (Test-Path -LiteralPath $releaseRoot)) {
        throw "Release output not found for project '$projectName'. Run the Release build first."
    }

    $files = @(
        Get-ChildItem -LiteralPath $releaseRoot -Recurse -File |
            Where-Object {
                $_.FullName -notmatch '[\\/](ref|refint|publish)[\\/]' -and
                $_.Extension -in @('.dll', '.xml', '.json')
            }
    )

    $primaryDll = $files |
        Where-Object { $_.Name -eq "$projectName.dll" } |
        Select-Object -First 1

    if ($null -eq $primaryDll) {
        throw "Primary assembly '$projectName.dll' was not found for project '$projectName'."
    }

    $targetProjectRoot = Join-Path $validatedRoot $projectName

    foreach ($file in $files) {
        $relative = [System.IO.Path]::GetRelativePath($releaseRoot, $file.FullName)
        $destination = Join-Path $targetProjectRoot $relative

        New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $destination -Force

        $manifestEntries.Add([pscustomobject]@{
            project = $projectName
            path = [System.IO.Path]::GetRelativePath($validatedRoot, $destination).Replace('\', '/')
            size = (Get-Item -LiteralPath $destination).Length
            sha256 = (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash.ToLowerInvariant()
        })
    }
}

& (Join-Path $tools 'Verify-AssemblyMetadata.ps1') -ArtifactsRoot $validatedRoot

$metadata = [ordered]@{
    product = 'ULSAlgorithms'
    packageVersion = $versionInfo.PackageVersion
    buildVersion = $versionInfo.BuildVersion
    assemblyVersion = $versionInfo.AssemblyVersion
    assemblyFileVersion = $versionInfo.AssemblyFileVersion
    assemblyInformationalVersion = $versionInfo.AssemblyInformationalVersion
    gitCommitId = $versionInfo.GitCommitId
    gitCommitIdShort = $versionInfo.GitCommitIdShort
    publicRelease = $versionInfo.PublicRelease
    generatedUtc = [DateTime]::UtcNow.ToString('o')
}

$metadataPath = Join-Path $artifactRoot 'build-metadata.json'
$metadata |
    ConvertTo-Json -Depth 8 |
    Set-Content -LiteralPath $metadataPath -Encoding utf8

# Do not use @($manifestEntries) here. On PowerShell 7 this can hit the
# dynamic binder for a generic List[object] and fail with
# "Argument types do not match". Materialize an object[] explicitly.
$manifestFiles = [object[]]$manifestEntries.ToArray()

$manifestPath = Join-Path $artifactRoot 'binaries-manifest.json'
[ordered]@{
    product = 'ULSAlgorithms'
    version = $version
    files = $manifestFiles
} |
    ConvertTo-Json -Depth 10 |
    Set-Content -LiteralPath $manifestPath -Encoding utf8

$zipPath = Join-Path $artifactRoot "ULSAlgorithms-$version-binaries.zip"
Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
Compress-Archive `
    -Path (Join-Path $validatedRoot '*') `
    -DestinationPath $zipPath `
    -CompressionLevel Optimal

$zipHash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
$shaPath = "$zipPath.sha256"
"$zipHash  $([System.IO.Path]::GetFileName($zipPath))" |
    Set-Content -LiteralPath $shaPath -Encoding ascii

return [pscustomobject]@{
    Version = $version
    ValidatedDirectory = $validatedRoot
    BinaryZip = $zipPath
    BinarySha256 = $shaPath
    BuildMetadata = $metadataPath
    BinariesManifest = $manifestPath
}
