Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')

$solutions = @(
    Get-ChildItem -LiteralPath $root -File -Filter '*.slnx' -ErrorAction SilentlyContinue
    Get-ChildItem -LiteralPath $root -File -Filter '*.sln'  -ErrorAction SilentlyContinue
)

if ($solutions.Count -gt 1) {
    throw "More than one solution was found at repository root: $($solutions.Name -join ', ')."
}

if ($solutions.Count -eq 1) {
    return [pscustomobject]@{
        Kind = 'Solution'
        Path = $solutions[0].FullName
    }
}

$projects = @(
    Get-ChildItem -LiteralPath (Join-Path $root 'src') -Recurse -File -Filter '*.csproj' -ErrorAction SilentlyContinue |
        Sort-Object FullName
)

if ($projects.Count -eq 1) {
    return [pscustomobject]@{
        Kind = 'Project'
        Path = $projects[0].FullName
    }
}

if ($projects.Count -gt 1) {
    throw 'Multiple projects exist under src/ but no root solution exists. Create ULSAlgorithms.slnx (preferred) or ULSAlgorithms.sln.'
}

return $null
