Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$tools = Join-Path $root 'tools'
$target = & (Join-Path $tools 'Get-BuildTarget.ps1')

if ($null -eq $target) {
    Write-Host 'Bootstrap mode: no .NET solution/project exists yet. Build skipped by design.'
    return [pscustomobject]@{
        Bootstrap = $true
        BuildTarget = $null
        TestProjects = 0
    }
}

$testResults = Join-Path $root 'Documentation\test-results'
Remove-Item -LiteralPath $testResults -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $testResults -Force | Out-Null

Write-Host "Restoring $($target.Path)"
& dotnet restore $target.Path | Out-Host
$restoreExitCode = $LASTEXITCODE
if ($restoreExitCode -ne 0) {
    throw "dotnet restore failed with exit code $restoreExitCode."
}

Write-Host "Building $($target.Path) in Release configuration"
& dotnet build $target.Path --configuration Release --no-restore | Out-Host
$buildExitCode = $LASTEXITCODE
if ($buildExitCode -ne 0) {
    throw "dotnet build failed with exit code $buildExitCode."
}

$testProjects = @(
    Get-ChildItem -LiteralPath (Join-Path $root 'tests') -Recurse -File -Filter '*.csproj' -ErrorAction SilentlyContinue |
        Sort-Object FullName
)

foreach ($project in $testProjects) {
    Write-Host "Testing $($project.FullName)"
    & dotnet test $project.FullName `
        --configuration Release `
        --no-restore `
        --logger "trx;LogFileName=$($project.BaseName).trx" `
        --results-directory $testResults | Out-Host

    $testExitCode = $LASTEXITCODE
    if ($testExitCode -ne 0) {
        throw "Tests failed for '$($project.FullName)' (exit code $testExitCode)."
    }
}

return [pscustomobject]@{
    Bootstrap = $false
    BuildTarget = $target.Path
    TestProjects = $testProjects.Count
}
