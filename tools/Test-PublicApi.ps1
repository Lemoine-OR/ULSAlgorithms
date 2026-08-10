Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')
$project = Join-Path $root 'tools\ULSAlgorithms.PublicApiExporter\ULSAlgorithms.PublicApiExporter.csproj'
$baseline = Join-Path $root 'eng\public-api\ULSAlgorithms.PublicApi.txt'

foreach ($path in @($project, $baseline)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required public API compatibility file is missing: $path"
    }
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "Required command 'dotnet' was not found on PATH."
}

& dotnet run `
    --configuration Release `
    --project $project `
    -- `
    --check $baseline | Out-Host

if ($LASTEXITCODE -ne 0) {
    throw "Public API compatibility validation failed with exit code $LASTEXITCODE."
}

Write-Host 'Public API compatibility validation passed.'
