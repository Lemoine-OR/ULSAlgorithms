Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$tools = Join-Path $root 'tools'
$target = & (Join-Path $tools 'Get-BuildTarget.ps1')
if ($null -eq $target) {
    throw 'A release cannot be created before the repository contains a buildable C# solution/project.'
}

$versionInfo = & (Join-Path $tools 'Get-ULSAlgorithmsVersion.ps1')
$releaseVersion = [string]$versionInfo.PackageVersion
if ($releaseVersion -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') {
    throw "Invalid release PackageVersion '$releaseVersion'."
}

$package = & (Join-Path $PSScriptRoot 'Package-ValidatedBinaries.ps1')
if ($null -eq $package) {
    throw 'Binary packaging failed to produce release inputs.'
}

$nuget = & (Join-Path $PSScriptRoot 'Package-NuGet.ps1')
if ($null -eq $nuget) {
    throw 'NuGet packaging failed to produce release inputs.'
}

& (Join-Path $root 'docs\build-documentation.ps1')
$site = Join-Path $root 'Documentation\site'
if (-not (Test-Path -LiteralPath (Join-Path $site 'index.html'))) {
    throw 'Documentation site does not contain index.html.'
}

$releaseDir = Join-Path $root 'Documentation\release'
Remove-Item -LiteralPath $releaseDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null

foreach ($path in @(
    $package.BinaryZip,
    $package.BinarySha256,
    $package.BuildMetadata,
    $package.BinariesManifest,
    $nuget.Package,
    $nuget.Sha256,
    $nuget.SymbolPackage,
    $nuget.SymbolSha256
)) {
    Copy-Item -LiteralPath $path -Destination (Join-Path $releaseDir ([System.IO.Path]::GetFileName($path))) -Force
}

$docsZipName = "ULSAlgorithms-$releaseVersion-documentation.zip"
$docsZip = Join-Path $releaseDir $docsZipName
Compress-Archive -Path (Join-Path $site '*') -DestinationPath $docsZip -CompressionLevel Optimal
$docsHash = (Get-FileHash -LiteralPath $docsZip -Algorithm SHA256).Hash.ToLowerInvariant()
"$docsHash  $docsZipName" | Set-Content -LiteralPath "$docsZip.sha256" -Encoding ascii

$assetFiles = @(
    Get-ChildItem -LiteralPath $releaseDir -File |
        Sort-Object Name |
        Where-Object { $_.Name -notin @('release-manifest.json','release-manifest.json.sha256') }
)

$releaseManifest = [ordered]@{
    product = 'ULSAlgorithms'
    releaseVersion = $releaseVersion
    buildVersion = $versionInfo.BuildVersion
    tag = "v$releaseVersion"
    commit = $versionInfo.GitCommitId
    prerelease = $releaseVersion.Contains('-')
    generatedUtc = [DateTime]::UtcNow.ToString('o')
    assets = @(
        $assetFiles | ForEach-Object {
            [ordered]@{
                name = $_.Name
                size = $_.Length
                sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            }
        }
    )
}

$manifestPath = Join-Path $releaseDir 'release-manifest.json'
$releaseManifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $manifestPath -Encoding utf8
$manifestHash = (Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash.ToLowerInvariant()
"$manifestHash  release-manifest.json" | Set-Content -LiteralPath (Join-Path $releaseDir 'release-manifest.json.sha256') -Encoding ascii

& (Join-Path $tools 'Test-ReleaseArtifacts.ps1') -ReleaseDirectory $releaseDir

return [pscustomobject]@{
    ReleaseVersion = $releaseVersion
    BuildVersion = $versionInfo.BuildVersion
    Tag = "v$releaseVersion"
    CommitId = $versionInfo.GitCommitId
    Prerelease = $releaseVersion.Contains('-')
    ReleaseDirectory = $releaseDir
}
