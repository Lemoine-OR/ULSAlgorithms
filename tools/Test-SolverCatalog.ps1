Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')
$project = Join-Path $root 'tools\ULSAlgorithms.CatalogExporter\ULSAlgorithms.CatalogExporter.csproj'
$catalog = Join-Path $root 'docs\algorithm-catalog.json'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "Required command 'dotnet' was not found on PATH."
}

foreach ($path in @($project, $catalog)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required solver-catalog file is missing: $path"
    }
}

& dotnet run `
    --configuration Release `
    --project $project `
    -- `
    --check $catalog | Out-Host

if ($LASTEXITCODE -ne 0) {
    throw "Solver catalog validation failed with exit code $LASTEXITCODE."
}

Write-Host 'Runtime solver catalog and documentation projection are synchronized.'
