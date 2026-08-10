param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,

    [Parameter(Mandatory = $true)]
    [string]$PackageVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "Required command 'dotnet' was not found on PATH."
}

$package = (Resolve-Path -LiteralPath $PackagePath).Path
$packageDirectory = Split-Path -Parent $package

if ($PackageVersion -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') {
    throw "Invalid package version '$PackageVersion'."
}

$tempRoot =
    Join-Path `
        ([IO.Path]::GetTempPath()) `
        ("ULSAlgorithms-NuGetConsumer-" + [Guid]::NewGuid().ToString('N'))

New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

try {
    $projectPath = Join-Path $tempRoot 'Consumer.csproj'
    $programPath = Join-Path $tempRoot 'Program.cs'
    $utf8 = New-Object System.Text.UTF8Encoding($false)

    $projectText = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="ULSAlgorithms" Version="$PackageVersion" />
  </ItemGroup>
</Project>
"@

    $programText = @'
using ULSAlgorithms.Catalog;
using ULSAlgorithms.Models;
using ULSAlgorithms.Results;

const double ExpectedObjective = 680.0;
const double RelativeTolerance = 1.0e-8;

var problem =
    new UlsProblem(
        demands:
            [20.0, 30.0, 25.0, 40.0],
        setupCosts:
            [200.0, 200.0, 200.0, 200.0],
        unitProductionCosts:
            [0.0, 0.0, 0.0, 0.0],
        holdingCosts:
            [4.0, 4.0, 4.0, 0.0]);

var result =
    UlsSolverFactory
        .Create("adaptive-exact")
        .Solve(problem);

if (result.Status != UlsSolveStatus.Optimal)
{
    throw new InvalidOperationException(
        $"Package consumer returned status '{result.Status}' instead of Optimal.");
}

if (!result.ObjectiveValue.HasValue ||
    !double.IsFinite(result.ObjectiveValue.Value))
{
    throw new InvalidOperationException(
        "Package consumer returned no finite objective.");
}

var objective =
    result.ObjectiveValue.Value;

var scale =
    Math.Max(
        1.0,
        Math.Max(
            Math.Abs(ExpectedObjective),
            Math.Abs(objective)));

if (Math.Abs(objective - ExpectedObjective) >
    RelativeTolerance * scale)
{
    throw new InvalidOperationException(
        $"Package consumer objective mismatch. Expected {ExpectedObjective:R}, got {objective:R}.");
}

Console.WriteLine(
    $"NuGet consumer smoke passed. Objective = {objective:R}.");
'@

    [IO.File]::WriteAllText(
        $projectPath,
        $projectText,
        $utf8)

    [IO.File]::WriteAllText(
        $programPath,
        $programText,
        $utf8)

    Write-Host "Restoring isolated NuGet consumer from local package source: $packageDirectory"
    & dotnet restore `
        $projectPath `
        --source $packageDirectory `
        --ignore-failed-sources |
        Out-Host

    if ($LASTEXITCODE -ne 0) {
        throw "NuGet consumer restore failed with exit code $LASTEXITCODE."
    }

    Write-Host 'Building and running isolated NuGet consumer.'
    & dotnet run `
        --project $projectPath `
        --configuration Release `
        --no-restore |
        Out-Host

    if ($LASTEXITCODE -ne 0) {
        throw "NuGet consumer execution failed with exit code $LASTEXITCODE."
    }
}
finally {
    Remove-Item `
        -LiteralPath $tempRoot `
        -Recurse `
        -Force `
        -ErrorAction SilentlyContinue
}

Write-Host "NuGet consumer validation passed for ULSAlgorithms $PackageVersion."
