Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')

$excludedDirectoryPattern =
    '[\\/](?:\.git|\.vs|bin|obj|Documentation|BenchmarkDotNet\.Artifacts)[\\/]'

$projects = @(
    Get-ChildItem `
        -LiteralPath $root `
        -Recurse `
        -File `
        -Filter '*.csproj' `
        -ErrorAction Stop |
    Where-Object {
        $_.FullName -notmatch $excludedDirectoryPattern
    } |
    Sort-Object FullName -Unique
)

return $projects
