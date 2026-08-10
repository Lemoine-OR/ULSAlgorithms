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
        DotNetProjects = 0
        TestProjects = 0
    }
}

$allProjects = @(
    & (Join-Path $tools 'Get-DotNetProjects.ps1')
)

if ($allProjects.Count -eq 0) {
    throw 'A build target exists, but no .NET project was discovered.'
}

$testResults =
    Join-Path `
        (Join-Path $root 'Documentation') `
        'test-results'

Remove-Item `
    -LiteralPath $testResults `
    -Recurse `
    -Force `
    -ErrorAction SilentlyContinue

New-Item `
    -ItemType Directory `
    -Path $testResults `
    -Force |
    Out-Null

Write-Host "Restoring primary build target: $($target.Path)"
& dotnet restore $target.Path | Out-Host
$restoreExitCode = $LASTEXITCODE
if ($restoreExitCode -ne 0) {
    throw "dotnet restore failed with exit code $restoreExitCode."
}

Write-Host "Building primary target in Release configuration: $($target.Path)"
& dotnet build `
    $target.Path `
    --configuration Release `
    --no-restore |
    Out-Host

$buildExitCode = $LASTEXITCODE
if ($buildExitCode -ne 0) {
    throw "dotnet build failed with exit code $buildExitCode."
}

Write-Host "Discovered $($allProjects.Count) .NET project(s). Building every project explicitly."

foreach ($project in $allProjects) {
    Write-Host "Restoring project: $($project.FullName)"
    & dotnet restore $project.FullName | Out-Host

    $projectRestoreExitCode = $LASTEXITCODE
    if ($projectRestoreExitCode -ne 0) {
        throw "Restore failed for '$($project.FullName)' (exit code $projectRestoreExitCode)."
    }

    Write-Host "Building project: $($project.FullName)"
    & dotnet build `
        $project.FullName `
        --configuration Release `
        --no-restore |
        Out-Host

    $projectBuildExitCode = $LASTEXITCODE
    if ($projectBuildExitCode -ne 0) {
        throw "Build failed for '$($project.FullName)' (exit code $projectBuildExitCode)."
    }
}

$testsRoot =
    (Resolve-Path (Join-Path $root 'tests')).Path

$testsPrefix =
    $testsRoot.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar) +
    [System.IO.Path]::DirectorySeparatorChar

$pathComparison =
    if ([System.Environment]::OSVersion.Platform -eq
        [System.PlatformID]::Win32NT) {
        [System.StringComparison]::OrdinalIgnoreCase
    }
    else {
        [System.StringComparison]::Ordinal
    }

$testProjects = @(
    $allProjects |
    Where-Object {
        $_.FullName.StartsWith(
            $testsPrefix,
            $pathComparison)
    }
)

foreach ($project in $testProjects) {
    Write-Host "Testing $($project.FullName)"
    & dotnet test `
        $project.FullName `
        --configuration Release `
        --no-build `
        --no-restore `
        --logger "trx;LogFileName=$($project.BaseName).trx" `
        --results-directory $testResults |
        Out-Host

    $testExitCode = $LASTEXITCODE
    if ($testExitCode -ne 0) {
        throw "Tests failed for '$($project.FullName)' (exit code $testExitCode)."
    }
}

return [pscustomobject]@{
    Bootstrap = $false
    BuildTarget = $target.Path
    DotNetProjects = $allProjects.Count
    TestProjects = $testProjects.Count
}
