Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')
$project = Join-Path $root 'tools\ULSAlgorithms.PublicApiExporter\ULSAlgorithms.PublicApiExporter.csproj'
$baseline = Join-Path $root 'eng\public-api\ULSAlgorithms.PublicApi.txt'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "Required command 'dotnet' was not found on PATH."
}

& dotnet run `
    --configuration Release `
    --project $project `
    -- `
    --write $baseline | Out-Host

if ($LASTEXITCODE -ne 0) {
    throw "Public API baseline generation failed with exit code $LASTEXITCODE."
}

Write-Host 'Review eng/public-api/ULSAlgorithms.PublicApi.txt before committing it.'
