Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$tools = Join-Path $root 'tools'
$baseDoxyfile = Join-Path $PSScriptRoot 'Doxyfile'

if (-not (Test-Path -LiteralPath $baseDoxyfile)) {
    throw 'docs/Doxyfile is missing.'
}

$doxygen = Get-Command doxygen -ErrorAction SilentlyContinue
if ($null -eq $doxygen) {
    throw 'Doxygen is not available on PATH. Run tools/Install-Doxygen.ps1 first.'
}

$version = & (Join-Path $tools 'Get-ULSAlgorithmsVersion.ps1')
$projectVersion = $version.PackageVersion

$docRoot = Join-Path $root 'Documentation'
$doxygenRoot = Join-Path $docRoot 'doxygen'
$siteRoot = Join-Path $docRoot 'site'

Remove-Item -LiteralPath $doxygenRoot -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $siteRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $doxygenRoot -Force | Out-Null
New-Item -ItemType Directory -Path $siteRoot -Force | Out-Null

$inputs = New-Object System.Collections.Generic.List[string]
$readme = Join-Path $root 'README.md'
if (Test-Path -LiteralPath $readme) {
    $inputs.Add($readme)
}

$src = Join-Path $root 'src'
if (Test-Path -LiteralPath $src) {
    $inputs.Add($src)
}

$pages = Join-Path $PSScriptRoot 'pages'
if (Test-Path -LiteralPath $pages) {
    $inputs.Add($pages)
}

if ($inputs.Count -eq 0) {
    throw 'No documentation input was found.'
}

function Convert-ToDoxygenPath([string]$Path) {
    return ('"' + $Path.Replace('\','/') + '"')
}

$tempDoxyfile = Join-Path $docRoot 'Doxyfile.generated'
$base = Get-Content -LiteralPath $baseDoxyfile -Raw
$inputValue = (($inputs | ForEach-Object { Convert-ToDoxygenPath $_ }) -join ' ')
$outputValue = Convert-ToDoxygenPath $doxygenRoot
$readmeValue = if (Test-Path -LiteralPath $readme) {
    Convert-ToDoxygenPath $readme
}
else {
    ''
}

$generated = @"
$base
PROJECT_NUMBER         = $projectVersion
OUTPUT_DIRECTORY       = $outputValue
INPUT                  = $inputValue
USE_MDFILE_AS_MAINPAGE = $readmeValue
"@

$generated | Set-Content -LiteralPath $tempDoxyfile -Encoding utf8

# Doxygen writes normal progress text to stdout. Route it to the host so this
# script remains safe to invoke from a parent script that captures its return value.
& $doxygen.Source $tempDoxyfile | Out-Host
$doxygenExitCode = $LASTEXITCODE
if ($doxygenExitCode -ne 0) {
    throw "Doxygen documentation build failed with exit code $doxygenExitCode."
}

$html = Join-Path $doxygenRoot 'html'
if (-not (Test-Path -LiteralPath (Join-Path $html 'index.html'))) {
    throw 'Doxygen did not generate html/index.html.'
}

Copy-Item -Path (Join-Path $html '*') -Destination $siteRoot -Recurse -Force
New-Item -ItemType File -Path (Join-Path $siteRoot '.nojekyll') -Force | Out-Null
Write-Host "Documentation site generated: $siteRoot"
