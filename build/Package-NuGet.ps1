param(
    [Parameter(Mandatory = $false)]
    [string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$tools = Join-Path $root 'tools'
$project = Join-Path $root 'src\ULSAlgorithms\ULSAlgorithms.csproj'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "Required command 'dotnet' was not found on PATH."
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $root 'Documentation\artifacts\nuget'
}
else {
    $OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$versionInfo = & (Join-Path $tools 'Get-ULSAlgorithmsVersion.ps1')
$v = [string]$versionInfo.PackageVersion
if ($v -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') {
    throw "Invalid package version '$v'."
}

$packageName = "ULSAlgorithms.$v.nupkg"
$packagePath = Join-Path $OutputDirectory $packageName
Remove-Item -LiteralPath $packagePath -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath "$packagePath.sha256" -Force -ErrorAction SilentlyContinue

& dotnet pack `
    $project `
    --configuration Release `
    --no-build `
    --output $OutputDirectory `
    /p:PackageVersion=$v | Out-Host

if ($LASTEXITCODE -ne 0) {
    throw "dotnet pack failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf)) {
    throw "Expected NuGet package was not produced: $packagePath"
}

& (Join-Path $tools 'Test-NuGetPackage.ps1') -PackagePath $packagePath

$hash = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $packageName" | Set-Content -LiteralPath "$packagePath.sha256" -Encoding ascii

return [pscustomobject]@{
    Package = $packagePath
    Sha256 = "$packagePath.sha256"
    Version = $v
}
