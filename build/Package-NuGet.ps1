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
$symbolPackageName = "ULSAlgorithms.$v.snupkg"
$symbolPackagePath = Join-Path $OutputDirectory $symbolPackageName

foreach ($path in @(
    $packagePath,
    "$packagePath.sha256",
    $symbolPackagePath,
    "$symbolPackagePath.sha256"
)) {
    Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
}

& dotnet pack `
    $project `
    --configuration Release `
    --no-build `
    --output $OutputDirectory `
    /p:PackageVersion=$v |
    Out-Host

if ($LASTEXITCODE -ne 0) {
    throw "dotnet pack failed with exit code $LASTEXITCODE."
}

foreach ($path in @($packagePath, $symbolPackagePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Expected NuGet package was not produced: $path"
    }
}

& (Join-Path $tools 'Test-NuGetPackage.ps1') `
    -PackagePath $packagePath

& (Join-Path $tools 'Test-NuGetSymbolPackage.ps1') `
    -PackagePath $symbolPackagePath `
    -PackageVersion $v

& (Join-Path $tools 'Test-NuGetConsumer.ps1') `
    -PackagePath $packagePath `
    -PackageVersion $v

$packageHash =
    (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).
        Hash.
        ToLowerInvariant()

"$packageHash  $packageName" |
    Set-Content `
        -LiteralPath "$packagePath.sha256" `
        -Encoding ascii

$symbolHash =
    (Get-FileHash -LiteralPath $symbolPackagePath -Algorithm SHA256).
        Hash.
        ToLowerInvariant()

"$symbolHash  $symbolPackageName" |
    Set-Content `
        -LiteralPath "$symbolPackagePath.sha256" `
        -Encoding ascii

return [pscustomobject]@{
    Package = $packagePath
    Sha256 = "$packagePath.sha256"
    SymbolPackage = $symbolPackagePath
    SymbolSha256 = "$symbolPackagePath.sha256"
    Version = $v
}
