[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $RepoRoot
$Tools = Join-Path $RepoRoot "tools"
$BaseDoxyfile = Join-Path $PSScriptRoot "Doxyfile"
$CatalogPath = Join-Path $PSScriptRoot "algorithm-catalog.json"
$MainPagePath = Join-Path $PSScriptRoot "mainpage.md"

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found on PATH."
    }
}

function Escape-Html([string]$Value) {
    return [System.Net.WebUtility]::HtmlEncode($Value)
}

function Convert-ToDoxygenPath([string]$Path) {
    return '"' + $Path.Replace('\','/') + '"'
}

Require-Command "dotnet"
Require-Command "doxygen"
Require-Command "dot"

foreach ($requiredPath in @($BaseDoxyfile, $CatalogPath, $MainPagePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required documentation source is missing: $requiredPath"
    }
}

$versionInfo = & (Join-Path $Tools "Get-ULSAlgorithmsVersion.ps1")
$displayVersion = [string]$versionInfo.PackageVersion
$commitShort = [string]$versionInfo.GitCommitIdShort

if ([string]::IsNullOrWhiteSpace($commitShort)) {
    try {
        $commit = (& git -C $RepoRoot rev-parse HEAD 2>$null | Select-Object -First 1).Trim()
        $commitShort = if ($commit.Length -ge 10) { $commit.Substring(0, 10) } else { $commit }
    }
    catch {
        $commitShort = "unknown"
    }
}

$catalog = Get-Content -LiteralPath $CatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
$exact = @($catalog.exact)
$heuristics = @($catalog.heuristics)
$algorithms = @(
    $exact | ForEach-Object {
        [pscustomobject]@{
            Kind = "exact"
            Name = [string]$_.name
            Class = [string]$_.class
            Family = [string]$_.family
            Time = [string]$_.time
            Space = [string]$_.space
            Applicability = [string]$_.applicability
            SourcePath = [string]$_.sourcePath
            Publication = [string]$_.publication
            Doi = [string]$_.doi
            Implementation = [string]$_.implementation
        }
    }
    $heuristics | ForEach-Object {
        [pscustomobject]@{
            Kind = "heuristic"
            Name = [string]$_.name
            Class = [string]$_.class
            Family = [string]$_.family
            Time = [string]$_.time
            Space = [string]$_.space
            Applicability = [string]$_.applicability
            SourcePath = [string]$_.sourcePath
            Publication = [string]$_.publication
            Doi = [string]$_.doi
            Implementation = [string]$_.implementation
        }
    }
)

if ($algorithms.Count -eq 0) {
    throw "Algorithm catalog is empty."
}

foreach ($algorithm in $algorithms) {
    $source = Join-Path $RepoRoot $algorithm.SourcePath
    if (-not (Test-Path -LiteralPath $source)) {
        throw "Algorithm catalog references a missing source file: $($algorithm.SourcePath)"
    }
}

$Documentation = Join-Path $RepoRoot "Documentation"
$DoxygenRoot = Join-Path $Documentation "doxygen"
$SiteRoot = Join-Path $Documentation "site"
$GeneratedRoot = Join-Path $Documentation "generated"

Remove-Item -LiteralPath $DoxygenRoot -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $SiteRoot -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $GeneratedRoot -Recurse -Force -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Path $DoxygenRoot -Force | Out-Null
New-Item -ItemType Directory -Path $SiteRoot -Force | Out-Null
New-Item -ItemType Directory -Path $GeneratedRoot -Force | Out-Null

# ------------------------------------------------------------------
# Generate the canonical algorithm catalog Doxygen page from JSON.
# ------------------------------------------------------------------
$catalogPage = New-Object System.Text.StringBuilder
[void]$catalogPage.AppendLine("\page algorithm_catalog Algorithm Catalog")
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("# Algorithm Catalog")
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("This page is generated from ``docs/algorithm-catalog.json``. Do not maintain a duplicate hand-written matrix.")
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("Current inventory: **$($exact.Count) exact algorithms**, **$($heuristics.Count) heuristics**, **$($algorithms.Count) public strategies**.")
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("## Exact algorithms")
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("| Strategy | Family | Time | Memory | Applicability | Scientific source |")
[void]$catalogPage.AppendLine("|---|---|---:|---:|---|---|")
foreach ($a in $algorithms | Where-Object Kind -eq "exact") {
    $publication = $a.Publication
    if (-not [string]::IsNullOrWhiteSpace($a.Doi)) {
        $publication = "[$publication](https://doi.org/$($a.Doi))"
    }
    [void]$catalogPage.AppendLine("| @ref $($a.Class) | $($a.Family) | $($a.Time) | $($a.Space) | $($a.Applicability) | $publication |")
}
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("## Heuristics")
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("| Strategy | Family | Time | Memory | Applicability | Scientific source |")
[void]$catalogPage.AppendLine("|---|---|---:|---:|---|---|")
foreach ($a in $algorithms | Where-Object Kind -eq "heuristic") {
    $publication = $a.Publication
    if (-not [string]::IsNullOrWhiteSpace($a.Doi)) {
        $publication = "[$publication](https://doi.org/$($a.Doi))"
    }
    [void]$catalogPage.AppendLine("| @ref $($a.Class) | $($a.Family) | $($a.Time) | $($a.Space) | $($a.Applicability) | $publication |")
}
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("## Implementation provenance")
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("The ``implementation`` field in the JSON catalog distinguishes direct/classical implementations from modern reconstructions when that distinction matters.")
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("See @ref algorithm_selection and @ref complexity_applicability before choosing a restricted solver.")

$generatedCatalog = Join-Path $GeneratedRoot "algorithm-catalog.generated.md"
Set-Content -LiteralPath $generatedCatalog -Value $catalogPage.ToString() -Encoding UTF8

# ------------------------------------------------------------------
# Generate API documentation.
# ------------------------------------------------------------------
$inputs = @(
    (Join-Path $RepoRoot "src"),
    (Join-Path $PSScriptRoot "pages"),
    $MainPagePath,
    $generatedCatalog
)

$tempDoxyfile = Join-Path $Documentation "Doxyfile.generated"
$base = Get-Content -LiteralPath $BaseDoxyfile -Raw -Encoding UTF8
$inputValue = (($inputs | ForEach-Object { Convert-ToDoxygenPath $_ }) -join " ")
$outputValue = Convert-ToDoxygenPath $DoxygenRoot
$mainPageValue = Convert-ToDoxygenPath $MainPagePath

$generatedDoxyfile = @"
$base
PROJECT_NUMBER         = "$displayVersion"
OUTPUT_DIRECTORY       = $outputValue
INPUT                  = $inputValue
USE_MDFILE_AS_MAINPAGE = $mainPageValue
"@

Set-Content -LiteralPath $tempDoxyfile -Value $generatedDoxyfile -Encoding UTF8

Write-Host ""
Write-Host "Generating ULSAlgorithms API documentation..."
& doxygen $tempDoxyfile | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw "Doxygen documentation build failed with exit code $LASTEXITCODE."
}

$ApiHtml = Join-Path $DoxygenRoot "html"
if (-not (Test-Path -LiteralPath (Join-Path $ApiHtml "index.html"))) {
    throw "Doxygen did not generate html/index.html."
}

$ApiSite = Join-Path $SiteRoot "api"
New-Item -ItemType Directory -Path $ApiSite -Force | Out-Null
Copy-Item -Path (Join-Path $ApiHtml "*") -Destination $ApiSite -Recurse -Force

# ------------------------------------------------------------------
# Render the custom research-oriented portal.
# ------------------------------------------------------------------
$rows = New-Object System.Text.StringBuilder
foreach ($a in $algorithms) {
    $kindLabel = if ($a.Kind -eq "exact") { "Exact" } else { "Heuristic" }
    $search = "$($a.Name) $($a.Class) $($a.Family) $($a.Applicability) $($a.Publication)".ToLowerInvariant()
    $sourceUrl = "https://github.com/Lemoine-OR/ULSAlgorithms/blob/main/$($a.SourcePath)"
    [void]$rows.AppendLine("            <tr data-kind=`"$($a.Kind)`" data-search=`"$(Escape-Html $search)`">")
    [void]$rows.AppendLine("              <td class=`"alg-name`"><span class=`"chip $($a.Kind)`">$kindLabel</span><strong>$(Escape-Html $a.Name)</strong><code>$(Escape-Html $a.Class)</code></td>")
    [void]$rows.AppendLine("              <td><span class=`"family-chip`">$(Escape-Html $a.Family)</span></td>")
    [void]$rows.AppendLine("              <td><strong>$(Escape-Html $a.Time)</strong></td>")
    [void]$rows.AppendLine("              <td>$(Escape-Html $a.Space)</td>")
    [void]$rows.AppendLine("              <td class=`"applicability`">$(Escape-Html $a.Applicability)</td>")
    [void]$rows.AppendLine("              <td><a class=`"source-link`" href=`"$sourceUrl`">Source ↗</a></td>")
    [void]$rows.AppendLine("            </tr>")
}

$PortalTemplate = Get-Content -LiteralPath (Join-Path $PSScriptRoot "portal\index.html") -Raw -Encoding UTF8
$Portal = $PortalTemplate.Replace("{{VERSION}}", (Escape-Html $displayVersion))
$Portal = $Portal.Replace("{{COMMIT}}", (Escape-Html $commitShort))
$Portal = $Portal.Replace("{{EXACT_COUNT}}", [string]$exact.Count)
$Portal = $Portal.Replace("{{HEURISTIC_COUNT}}", [string]$heuristics.Count)
$Portal = $Portal.Replace("{{TOTAL_COUNT}}", [string]$algorithms.Count)
$Portal = $Portal.Replace("{{ALGORITHM_ROWS}}", $rows.ToString().TrimEnd())

Set-Content -LiteralPath (Join-Path $SiteRoot "index.html") -Value $Portal -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "portal\styles.css") -Destination (Join-Path $SiteRoot "styles.css") -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "portal\app.js") -Destination (Join-Path $SiteRoot "app.js") -Force

New-Item -ItemType Directory -Path (Join-Path $SiteRoot "assets") -Force | Out-Null
Copy-Item -Path (Join-Path $PSScriptRoot "assets\*") -Destination (Join-Path $SiteRoot "assets") -Recurse -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "assets\algorithms-icon.ico") -Destination (Join-Path $SiteRoot "favicon.ico") -Force
New-Item -ItemType File -Path (Join-Path $SiteRoot ".nojekyll") -Force | Out-Null

$requiredOutputs = @(
    "index.html",
    "styles.css",
    "app.js",
    "favicon.ico",
    "api\index.html",
    "api\algorithm_catalog.html",
    "api\getting_started.html",
    "api\problem_and_notation.html",
    "api\exact_algorithms.html",
    "api\heuristics.html",
    "api\algorithm_selection.html",
    "api\complexity_applicability.html",
    "api\validation_benchmarks.html",
    "api\api_reference_guide.html",
    "api\scientific_references.html",
    "api\releases_reproducibility.html"
)

$missing = @()
foreach ($relative in $requiredOutputs) {
    if (-not (Test-Path -LiteralPath (Join-Path $SiteRoot $relative))) {
        $missing += $relative
    }
}

if ($missing.Count -gt 0) {
    throw "Documentation site is incomplete. Missing: $($missing -join ', ')"
}

& (Join-Path $PSScriptRoot "Test-DocumentationLinks.ps1") -SiteRoot $SiteRoot

Write-Host ""
Write-Host "ULSAlgorithms documentation successfully built and link-validated." -ForegroundColor Green
Write-Host "Portal: $SiteRoot"
Write-Host "Exact algorithms: $($exact.Count)"
Write-Host "Heuristics: $($heuristics.Count)"
Write-Host "Public strategies: $($algorithms.Count)"
