Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')
$versionJsonPath = Join-Path $root 'version.json'

if (-not (Test-Path -LiteralPath $versionJsonPath)) {
    throw "Missing version.json at repository root."
}

$versionJson = Get-Content -LiteralPath $versionJsonPath -Raw | ConvertFrom-Json
$declaredVersion = [string]$versionJson.version
if ($declaredVersion -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') {
    throw "version.json must contain an explicit three-component SemVer version. Found '$declaredVersion'."
}

$target = & (Join-Path $PSScriptRoot 'Get-BuildTarget.ps1')
if ($null -eq $target) {
    $commit = ''
    try {
        $commit = (& git -C $root rev-parse HEAD 2>$null | Select-Object -First 1).Trim()
    }
    catch {
        $commit = ''
    }

    return [pscustomobject]@{
        AssemblyVersion              = ''
        AssemblyFileVersion          = ''
        AssemblyInformationalVersion = ''
        BuildVersion                 = $declaredVersion
        BuildVersionSimple           = $declaredVersion
        BuildVersion3Components      = ($declaredVersion -split '-', 2)[0]
        GitCommitId                  = $commit
        GitCommitIdShort             = if ($commit.Length -ge 10) { $commit.Substring(0, 10) } else { $commit }
        GitVersionHeight             = ''
        MajorMinorVersion            = (($declaredVersion -split '-', 2)[0].Split('.')[0..1] -join '.')
        PackageVersion               = $declaredVersion
        NuGetPackageVersion          = $declaredVersion
        PublicRelease                = [string]($env:PublicRelease -eq 'true')
        DeclaredVersion              = $declaredVersion
        UsesNerdbankGitVersioning    = $false
    }
}

$probeProject = $null
if ($target.Kind -eq 'Project') {
    $probeProject = $target.Path
}
else {
    $probeProject = Get-ChildItem -LiteralPath (Join-Path $root 'src') -Recurse -File -Filter '*.csproj' -ErrorAction SilentlyContinue |
        Sort-Object FullName |
        Select-Object -First 1 -ExpandProperty FullName
}

if ([string]::IsNullOrWhiteSpace($probeProject)) {
    throw 'A solution exists but no source project was found under src/.'
}

# A fresh CI runner has no restored NuGet assets yet. Nerdbank.GitVersioning is
# brought into every project through Directory.Build.props, and its
# GetBuildVersion target is only available after package restore.
Write-Host "Restoring version probe project: $probeProject"
& dotnet restore $probeProject --nologo
if ($LASTEXITCODE -ne 0) {
    throw "NuGet restore failed for the NBGV version probe project '$probeProject'."
}

$tempDir = Join-Path $root 'Documentation\version-probe'
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
$outputPath = Join-Path $tempDir 'ULSAlgorithms.version.txt'
Remove-Item -LiteralPath $outputPath -Force -ErrorAction SilentlyContinue

$escapedOutput = $outputPath.Replace('"', '\"')
& dotnet msbuild $probeProject `
    /nologo `
    /v:minimal `
    /t:WriteULSAlgorithmsVersion `
    "/p:ULSAlgorithmsVersionOutput=$escapedOutput"

if ($LASTEXITCODE -ne 0) {
    throw "Nerdbank.GitVersioning version probe failed for '$probeProject'."
}

if (-not (Test-Path -LiteralPath $outputPath)) {
    throw 'Version probe did not create its expected output file.'
}

$map = @{}
foreach ($line in Get-Content -LiteralPath $outputPath) {
    if ($line -match '^([^=]+)=(.*)$') {
        $map[$matches[1]] = $matches[2]
    }
}

$required = @(
    'BuildVersion',
    'BuildVersionSimple',
    'BuildVersion3Components',
    'GitCommitId',
    'GitCommitIdShort',
    'PackageVersion',
    'NuGetPackageVersion',
    'PublicRelease'
)

foreach ($key in $required) {
    if (-not $map.ContainsKey($key)) {
        throw "Version probe output is missing '$key'."
    }
}

return [pscustomobject]@{
    AssemblyVersion              = $map['AssemblyVersion']
    AssemblyFileVersion          = $map['AssemblyFileVersion']
    AssemblyInformationalVersion = $map['AssemblyInformationalVersion']
    BuildVersion                 = $map['BuildVersion']
    BuildVersionSimple           = $map['BuildVersionSimple']
    BuildVersion3Components      = $map['BuildVersion3Components']
    GitCommitId                  = $map['GitCommitId']
    GitCommitIdShort             = $map['GitCommitIdShort']
    GitVersionHeight             = $map['GitVersionHeight']
    MajorMinorVersion            = $map['MajorMinorVersion']
    PackageVersion               = $map['PackageVersion']
    NuGetPackageVersion          = $map['NuGetPackageVersion']
    PublicRelease                = $map['PublicRelease']
    DeclaredVersion              = $declaredVersion
    UsesNerdbankGitVersioning    = $true
}
