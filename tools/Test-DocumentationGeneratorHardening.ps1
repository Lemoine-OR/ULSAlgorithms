Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')
$buildPath = Join-Path $root 'docs\build-documentation.ps1'
$doxyfilePath = Join-Path $root 'docs\Doxyfile'
$problemPath = Join-Path $root 'docs\pages\02-problem-and-notation.md'
$familiesPath = Join-Path $root 'docs\pages\03-method-families.md'
$heuristicsPath = Join-Path $root 'docs\pages\05-heuristics.md'
$groffPath = Join-Path $root 'docs\pages\groff.md'
$separatorPath = Join-Path $root 'src\ULSAlgorithms\CuttingPlanes\Separation\GeneralLsCutSeparator.cs'

foreach ($requiredPath in @(
    $buildPath,
    $doxyfilePath,
    $problemPath,
    $familiesPath,
    $heuristicsPath,
    $groffPath,
    $separatorPath
)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required documentation hardening file is missing: $requiredPath"
    }
}

$content = [IO.File]::ReadAllText($buildPath)

$requiredPatterns = @(
    '[void]$page.AppendLine("**Class:** ``$($a.Namespace).$($a.Class)``")',
    '[void]$page.AppendLine("Use the Doxygen Classes index for constructors, members and source-level documentation.")',
    '[IO.File]::WriteAllText((Join-Path $GeneratedRoot "algorithm-catalog.generated.md"), $catalogPage.ToString(), [Text.UTF8Encoding]::new($false))',
    '[IO.File]::WriteAllText((Join-Path $GeneratedRoot ("algorithm-" + $a.Slug + ".md")), $page.ToString(), [Text.UTF8Encoding]::new($false))',
    '[IO.File]::WriteAllText($tempDoxyfile, $generatedDoxyfile, [Text.UTF8Encoding]::new($false))',
    '$doxygenCombinedLog = Join-Path $Documentation "doxygen-v028.log"',
    '-RedirectStandardOutput $doxygenStdoutLog',
    '-RedirectStandardError $doxygenStderrLog',
    'Doxygen input contains an unexpected UTF-8 BOM'
)

foreach ($pattern in $requiredPatterns) {
    if (-not $content.Contains($pattern)) {
        throw "Documentation generator is missing required hardened pattern: $pattern"
    }
}

$forbiddenPatterns = @(
    '[void]$page.AppendLine("**Class:** @ref $($a.Class)")',
    '[void]$page.AppendLine("**Class:** @ref $($a.Namespace).$($a.Class)")',
    '[void]$page.AppendLine("Open @ref $($a.Class) for constructors, members and source-level documentation.")',
    '[void]$page.AppendLine("Open @ref $($a.Namespace).$($a.Class) for constructors, members and source-level documentation.")',
    'Set-Content -LiteralPath $tempDoxyfile -Value $generatedDoxyfile -Encoding UTF8',
    'Set-Content -LiteralPath (Join-Path $GeneratedRoot "algorithm-catalog.generated.md") -Value $catalogPage.ToString() -Encoding UTF8',
    'Set-Content -LiteralPath (Join-Path $GeneratedRoot ("algorithm-" + $a.Slug + ".md")) -Value $page.ToString() -Encoding UTF8',
    '& doxygen $tempDoxyfile | Out-Host'
)

foreach ($pattern in $forbiddenPatterns) {
    if ($content.Contains($pattern)) {
        throw "Documentation generator still contains obsolete pattern: $pattern"
    }
}

$doxyfile = [IO.File]::ReadAllText($doxyfilePath)
if ($doxyfile -notmatch '(?m)^\s*WARN_AS_ERROR\s*=\s*FAIL_ON_WARNINGS\s*$') {
    throw 'Doxygen strict warning handling is not enabled.'
}

$problem = [IO.File]::ReadAllText($problemPath)
if ($problem.Contains('\(t=1,\ldots,T\)') -or
    $problem.Contains('\(y_t\in')) {
    throw 'Problem/notation page still contains legacy inline TeX syntax.'
}

$families = [IO.File]::ReadAllText($familiesPath)
if ($families -match '(?m)^##\s+Heuristics\s*$') {
    throw 'Method-families page still creates the duplicate heuristics label.'
}

$heuristics = [IO.File]::ReadAllText($heuristicsPath)
if ($heuristics -match '(?m)^#\s+Heuristics\s*$') {
    throw 'Heuristics page duplicates its Doxygen page label with the Markdown H1 heading.'
}
if ($heuristics -notmatch '(?m)^#\s+Heuristic Strategy Families\s*$') {
    throw 'Heuristics page is missing its collision-safe H1 heading.'
}

$groff = [IO.File]::ReadAllText($groffPath)
if ($groff -match '(?m)^\\page\s+groff\s+') {
    throw 'Groff documentation page still uses the duplicate groff page label.'
}

$separator = [IO.File]::ReadAllText($separatorPath)
if ($separator -notmatch 'sum\(j in L minus S\)') {
    throw 'General (l,S) separator documentation is not using the Doxygen-safe text.'
}

Write-Host 'Documentation generator hardening validation passed.' -ForegroundColor Green
