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
    if ($null -eq $Value) { return "" }
    return [System.Net.WebUtility]::HtmlEncode($Value)
}

function Convert-ToDoxygenPath([string]$Path) {
    return '"' + $Path.Replace('\','/') + '"'
}

function Get-AlgorithmSlug([string]$ClassName) {
    return $ClassName.ToLowerInvariant()
}

function Get-AlgorithmNamespace([string]$SourcePath) {
    $relative = $SourcePath.Replace('\','/')
    $prefix = "src/ULSAlgorithms/"
    if (-not $relative.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
        return "ULSAlgorithms"
    }
    $directory = [IO.Path]::GetDirectoryName($relative.Substring($prefix.Length))
    if ([string]::IsNullOrWhiteSpace($directory)) { return "ULSAlgorithms" }
    return "ULSAlgorithms." + $directory.Replace('\','.').Replace('/','.')
}

function Get-DocumentationGroup([object]$Algorithm) {
    if ($Algorithm.Kind -eq "heuristic") { return "heuristic" }
    if ($Algorithm.Family.StartsWith("Solver-backed", [StringComparison]::OrdinalIgnoreCase)) { return "optimization" }
    if ($Algorithm.Family.StartsWith("Cutting planes", [StringComparison]::OrdinalIgnoreCase)) { return "cutting" }
    return "exact"
}

function Get-GroupLabel([string]$Group) {
    switch ($Group) {
        "exact" { return "Exact algorithm" }
        "optimization" { return "Mathematical optimization" }
        "cutting" { return "Cutting-plane method" }
        "heuristic" { return "Heuristic" }
        default { return "Algorithm" }
    }
}

function Get-Description([object]$Algorithm) {
    if ($Algorithm.Class -eq "AdaptiveExactUlsSolver") {
        return "Adaptive exact selection is the recommended orchestration strategy when client code needs an exact ULS solution without hard-coding a particular exact algorithm. It inspects the no-speculative-motive cost condition and dispatches to the fastest supported specialized or general exact strategy."
    }
    switch ($Algorithm.Group) {
        "heuristic" {
            return "$($Algorithm.Name) is a fast ULS heuristic in the $($Algorithm.Family) family. It constructs a feasible replenishment plan without claiming an optimality proof. Use it only when the documented applicability conditions match the instance."
        }
        "optimization" {
            return "$($Algorithm.Name) is an exact solver-backed ULS strategy. It builds a mathematical formulation and delegates the optimization step to the selected external engine while keeping the common IUlsSolver result contract."
        }
        "cutting" {
            return "$($Algorithm.Name) is an exact polyhedral ULS strategy. It strengthens the root optimization model with classical (l,S) inequalities before the final exact solve and records the generated and added cuts."
        }
        default {
            return "$($Algorithm.Name) is a direct exact ULS method in the $($Algorithm.Family) family. It solves the problem without requiring an external mathematical-programming engine and returns an optimal solution when its applicability conditions are satisfied."
        }
    }
}

function Get-HowItWorks([object]$Algorithm) {
    if ($Algorithm.Class -eq "AdaptiveExactUlsSolver") {
        return "The selector checks p[t] + h[t] >= p[t+1] over adjacent periods. If the condition holds, it executes the linear-time Wagner-Whitin specialization. Otherwise it executes the configured general O(T log T) solver, Wagelmans by default or Federgruen-Tzur when explicitly requested. Selection does not change the common IUlsSolver contract."
    }
    switch ($Algorithm.Group) {
        "heuristic" { return "The method scans the planning horizon and constructs replenishment cycles according to its published decision rule. The library then reconstructs production, inventory, setups and cost components through the common heuristic solution builder." }
        "optimization" { return "The method builds its portable linear or mixed-integer formulation, automatically selects an available engine in the CPLEX -> Gurobi -> Xpress -> CBC priority order, solves the model, normalizes numerical values and reconstructs a UlsSolution that is checked independently." }
        "cutting" { return "The method solves a root relaxation, separates violated (l,S) inequalities, records every candidate and disposition, adds the selected unique violated cuts, repeats root strengthening, and finally solves the strengthened binary model exactly with the same optimization engine." }
        default { return "The method works directly on the ULS arrays using the algorithmic mechanism identified by its family and implementation note. No external optimizer is needed. The returned plan is reconstructed through the common ULS result model." }
    }
}


function Get-FormulationModelHtml([object]$Algorithm) {
    switch ($Algorithm.Class) {
        "AggregateInventoryFormulationSolver" {
            return @'
<section class="mathematical-model">
  <span class="kicker">Mathematical formulation</span>
  <h2>Aggregate inventory-balance model</h2>
  <p class="model-intro">The equations below use periods 1,...,T for readability. The C# API uses zero-based indices 0,...,T-1.</p>
  <div class="model-notation"><span><code>x_t</code> production</span><span><code>y_t</code> setup</span><span><code>I_t</code> end-of-period inventory</span></div>
  <div class="math-block">\[
    \min \sum_{t=1}^{T}\left(f_t y_t + p_t x_t + h_t I_t\right)
  \]</div>
  <p class="equation-label">subject to</p>
  <div class="math-block">\[
    I_{t-1}+x_t-I_t=d_t, \qquad t=1,\ldots,T
  \]</div>
  <div class="math-block">\[
    x_t \le D_{t,T}y_t, \qquad
    D_{t,T}=\sum_{k=t}^{T}d_k
  \]</div>
  <div class="math-block">\[
    I_0=0,\qquad I_T=0,\qquad x_t\ge0,\ I_t\ge0,\ y_t\in\{0,1\}.
  \]</div>
  <p class="model-note">ULSAlgorithms uses the tight suffix-demand bound <code>D[t..T]</code> rather than an arbitrary big-M.</p>
</section>
'@
        }
        "FacilityLocationFormulationSolver" {
            return @'
<section class="mathematical-model">
  <span class="kicker">Mathematical formulation</span>
  <h2>Disaggregated facility-location model</h2>
  <p class="model-intro">The equations below use periods 1,...,T for readability. <code>q_{tk}</code> is the quantity of demand in period k supplied by production in period t.</p>
  <div class="model-notation"><span><code>q_{tk}</code> assignment quantity</span><span><code>y_t</code> setup</span></div>
  <div class="math-block">\[
    c_{tk}=p_t+\sum_{r=t}^{k-1}h_r
  \]</div>
  <div class="math-block">\[
    \min \sum_{t=1}^{T}f_t y_t
    +\sum_{k=1}^{T}\sum_{t=1}^{k}c_{tk}q_{tk}
  \]</div>
  <p class="equation-label">subject to</p>
  <div class="math-block">\[
    \sum_{t=1}^{k}q_{tk}=d_k, \qquad k=1,\ldots,T
  \]</div>
  <div class="math-block">\[
    q_{tk}\le d_k y_t, \qquad 1\le t\le k\le T
  \]</div>
  <div class="math-block">\[
    q_{tk}\ge0,\qquad y_t\in\{0,1\}.
  \]</div>
  <p class="model-note">Zero-demand assignment variables are omitted by the implementation.</p>
</section>
'@
        }
        "ShortestPathFormulationSolver" {
            return @'
<section class="mathematical-model">
  <span class="kicker">Mathematical formulation</span>
  <h2>Regeneration shortest-path model</h2>
  <p class="model-intro">A replenishment arc from t to j+1 represents one setup in period t serving all demand from t through j.</p>
  <div class="model-notation"><span><code>z_{t,j+1}</code> arc-flow variable</span><span><code>c_{tj}</code> regeneration-arc cost</span></div>
  <div class="math-block">\[
    c_{tj}=f_t+\sum_{k=t}^{j}d_k\left(p_t+\sum_{r=t}^{k-1}h_r\right)
  \]</div>
  <div class="math-block">\[
    \min \sum_{(t,j+1)\in A}c_{tj}z_{t,j+1}
  \]</div>
  <p class="equation-label">subject to node-flow conservation</p>
  <div class="math-block">\[
    \sum_{a\in\delta^+(v)}z_a-\sum_{a\in\delta^-(v)}z_a=b_v,
    \qquad
    b_v=\begin{cases}
      1 & v=0,\\
      -1 & v=T,\\
      0 & \text{otherwise.}
    \end{cases}
  \]</div>
  <div class="math-block">\[
    0\le z_a\le1.
  \]</div>
  <p class="equation-label">Applicability condition</p>
  <div class="math-block">\[
    p_t+h_t\ge p_{t+1}, \qquad t=1,\ldots,T-1.
  \]</div>
  <p class="model-note">The network matrix is integral; zero-demand periods may be crossed by explicit zero-cost skip arcs.</p>
</section>
'@
        }
        "InventoryEliminatedFormulationSolver" {
            return @'
<section class="mathematical-model">
  <span class="kicker">Mathematical formulation</span>
  <h2>Inventory-eliminated aggregate model</h2>
  <p class="model-intro">Inventory variables are removed algebraically. The implementation folds holding costs into the production coefficients and keeps the corresponding objective constant.</p>
  <div class="model-notation"><span><code>x_t</code> production</span><span><code>y_t</code> setup</span></div>
  <div class="math-block">\[
    \bar p_t=p_t+\sum_{r=t}^{T-1}h_r,
    \qquad
    C=-\sum_{t=1}^{T-1}h_t\sum_{i=1}^{t}d_i
  \]</div>
  <div class="math-block">\[
    \min \sum_{t=1}^{T}f_t y_t+\sum_{t=1}^{T}\bar p_t x_t+C
  \]</div>
  <p class="equation-label">subject to</p>
  <div class="math-block">\[
    \sum_{i=1}^{t}x_i\ge\sum_{i=1}^{t}d_i,
    \qquad t=1,\ldots,T-1
  \]</div>
  <div class="math-block">\[
    \sum_{i=1}^{T}x_i=\sum_{i=1}^{T}d_i
  \]</div>
  <div class="math-block">\[
    x_t\le D_{t,T}y_t,\qquad
    D_{t,T}=\sum_{k=t}^{T}d_k,
    \qquad x_t\ge0,\ y_t\in\{0,1\}.
  \]</div>
  <p class="model-note">The cumulative-demand inequalities are exactly the nonnegative-inventory conditions after eliminating <code>I_t</code>.</p>
</section>
'@
        }
        default { return "" }
    }
}

function Get-FormulationModelDoxygen([object]$Algorithm) {
    switch ($Algorithm.Class) {
        "AggregateInventoryFormulationSolver" {
            return @'
## Mathematical model

Equations use periods 1,...,T for readability; the C# API is zero-based.

\f[
\min \sum_{t=1}^{T}\left(f_t y_t+p_t x_t+h_t I_t\right)
\f]

subject to

\f[
I_{t-1}+x_t-I_t=d_t, \qquad t=1,\ldots,T,
\f]

\f[
x_t\le D_{t,T}y_t, \qquad D_{t,T}=\sum_{k=t}^{T}d_k,
\f]

\f[
I_0=0,\qquad I_T=0,\qquad x_t\ge0,\ I_t\ge0,\ y_t\in\{0,1\}.
\f]
'@
        }
        "FacilityLocationFormulationSolver" {
            return @'
## Mathematical model

Let \f$q_{tk}\f$ be the amount of demand in period \f$k\f$ supplied by production in period \f$t\f$, and define

\f[
c_{tk}=p_t+\sum_{r=t}^{k-1}h_r.
\f]

Then

\f[
\min \sum_{t=1}^{T}f_t y_t+\sum_{k=1}^{T}\sum_{t=1}^{k}c_{tk}q_{tk}
\f]

subject to

\f[
\sum_{t=1}^{k}q_{tk}=d_k, \qquad k=1,\ldots,T,
\f]

\f[
q_{tk}\le d_k y_t, \qquad 1\le t\le k\le T,
\f]

\f[
q_{tk}\ge0,\qquad y_t\in\{0,1\}.
\f]
'@
        }
        "ShortestPathFormulationSolver" {
            return @'
## Mathematical model

A replenishment arc \f$(t,j+1)\f$ has cost

\f[
c_{tj}=f_t+\sum_{k=t}^{j}d_k\left(p_t+\sum_{r=t}^{k-1}h_r\right).
\f]

The unit-flow model is

\f[
\min \sum_{(t,j+1)\in A}c_{tj}z_{t,j+1}
\f]

with node-flow conservation

\f[
\sum_{a\in\delta^+(v)}z_a-\sum_{a\in\delta^-(v)}z_a=b_v,
\qquad
b_v=\begin{cases}
1 & v=0,\\
-1 & v=T,\\
0 & \text{otherwise,}
\end{cases}
\f]

and \f$0\le z_a\le1\f$. The formulation requires

\f[
p_t+h_t\ge p_{t+1}, \qquad t=1,\ldots,T-1.
\f]
'@
        }
        "InventoryEliminatedFormulationSolver" {
            return @'
## Mathematical model

After eliminating inventory, define

\f[
\bar p_t=p_t+\sum_{r=t}^{T-1}h_r,
\qquad
C=-\sum_{t=1}^{T-1}h_t\sum_{i=1}^{t}d_i.
\f]

The model is

\f[
\min \sum_{t=1}^{T}f_t y_t+\sum_{t=1}^{T}\bar p_t x_t+C
\f]

subject to

\f[
\sum_{i=1}^{t}x_i\ge\sum_{i=1}^{t}d_i,
\qquad t=1,\ldots,T-1,
\f]

\f[
\sum_{i=1}^{T}x_i=\sum_{i=1}^{T}d_i,
\f]

\f[
x_t\le D_{t,T}y_t,\qquad D_{t,T}=\sum_{k=t}^{T}d_k,
\qquad x_t\ge0,\ y_t\in\{0,1\}.
\f]
'@
        }
        default { return "" }
    }
}

function Get-MathJaxHead([object]$Algorithm) {
    if ($Algorithm.Group -ne "optimization") { return "" }

    return @'
<script>
window.MathJax = {
  tex: {
    inlineMath: [['\\(', '\\)']],
    displayMath: [['\\[', '\\]']]
  },
  svg: { fontCache: 'global' }
};
</script>
<script defer src="https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-svg.js"></script>
'@
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
    catch { $commitShort = "unknown" }
}

$catalog = Get-Content -LiteralPath $CatalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
$exact = @($catalog.exact)
$heuristics = @($catalog.heuristics)
$algorithms = @(
    $exact | ForEach-Object {
        [pscustomobject]@{Kind="exact";Name=[string]$_.name;Class=[string]$_.class;Family=[string]$_.family;Time=[string]$_.time;Space=[string]$_.space;Applicability=[string]$_.applicability;SourcePath=[string]$_.sourcePath;Publication=[string]$_.publication;Doi=[string]$_.doi;Implementation=[string]$_.implementation}
    }
    $heuristics | ForEach-Object {
        [pscustomobject]@{Kind="heuristic";Name=[string]$_.name;Class=[string]$_.class;Family=[string]$_.family;Time=[string]$_.time;Space=[string]$_.space;Applicability=[string]$_.applicability;SourcePath=[string]$_.sourcePath;Publication=[string]$_.publication;Doi=[string]$_.doi;Implementation=[string]$_.implementation}
    }
)
if ($algorithms.Count -eq 0) { throw "Algorithm catalog is empty." }

foreach ($algorithm in $algorithms) {
    $source = Join-Path $RepoRoot $algorithm.SourcePath
    if (-not (Test-Path -LiteralPath $source)) { throw "Algorithm catalog references a missing source file: $($algorithm.SourcePath)" }
    Add-Member -InputObject $algorithm -NotePropertyName Slug -NotePropertyValue (Get-AlgorithmSlug $algorithm.Class)
    Add-Member -InputObject $algorithm -NotePropertyName Group -NotePropertyValue (Get-DocumentationGroup $algorithm)
    Add-Member -InputObject $algorithm -NotePropertyName GroupLabel -NotePropertyValue (Get-GroupLabel $algorithm.Group)
    Add-Member -InputObject $algorithm -NotePropertyName Namespace -NotePropertyValue (Get-AlgorithmNamespace $algorithm.SourcePath)
    Add-Member -InputObject $algorithm -NotePropertyName Description -NotePropertyValue (Get-Description $algorithm)
    Add-Member -InputObject $algorithm -NotePropertyName HowItWorks -NotePropertyValue (Get-HowItWorks $algorithm)
}

$directExact = @($algorithms | Where-Object Group -eq "exact")
$optimization = @($algorithms | Where-Object Group -eq "optimization")
$cutting = @($algorithms | Where-Object Group -eq "cutting")
$heuristicGroup = @($algorithms | Where-Object Group -eq "heuristic")

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

# Canonical catalog page.
$catalogPage = New-Object System.Text.StringBuilder
[void]$catalogPage.AppendLine("\page algorithm_catalog Algorithm Catalog")
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("# Algorithm Catalog")
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("Generated from ``docs/algorithm-catalog.json``. The documentation portal is the recommended browsing interface.")
[void]$catalogPage.AppendLine("")
[void]$catalogPage.AppendLine("Current inventory: **$($exact.Count) exact strategies**, **$($heuristics.Count) heuristics**, **$($algorithms.Count) public strategies**.")
[void]$catalogPage.AppendLine("")
foreach ($groupName in @("exact","optimization","cutting","heuristic")) {
    $groupAlgorithms = @($algorithms | Where-Object Group -eq $groupName)
    if ($groupAlgorithms.Count -eq 0) { continue }
    [void]$catalogPage.AppendLine("## $(Get-GroupLabel $groupName)")
    [void]$catalogPage.AppendLine("")
    [void]$catalogPage.AppendLine("| Strategy | Family | Time | Memory | Applicability |")
    [void]$catalogPage.AppendLine("|---|---|---:|---:|---|")
    foreach ($a in $groupAlgorithms) {
        [void]$catalogPage.AppendLine("| @ref algorithm_$($a.Slug) | $($a.Family) | $($a.Time) | $($a.Space) | $($a.Applicability) |")
    }
    [void]$catalogPage.AppendLine("")
}
Set-Content -LiteralPath (Join-Path $GeneratedRoot "algorithm-catalog.generated.md") -Value $catalogPage.ToString() -Encoding UTF8

# One stable Doxygen API landing page per algorithm.
foreach ($a in $algorithms) {
    $page = New-Object System.Text.StringBuilder
    [void]$page.AppendLine("\page algorithm_$($a.Slug) $($a.Name)")
    [void]$page.AppendLine("")
    [void]$page.AppendLine("# $($a.Name)")
    [void]$page.AppendLine("")
    [void]$page.AppendLine("**Class:** @ref $($a.Class)")
    [void]$page.AppendLine("")
    [void]$page.AppendLine("**Family:** $($a.Family)  ")
    [void]$page.AppendLine("**Time:** $($a.Time)  ")
    [void]$page.AppendLine("**Memory:** $($a.Space)  ")
    [void]$page.AppendLine("**Applicability:** $($a.Applicability)")
    [void]$page.AppendLine("")
    [void]$page.AppendLine("## Description")
    [void]$page.AppendLine("")
    [void]$page.AppendLine($a.Description)
    [void]$page.AppendLine("")
    $modelDoxygen = Get-FormulationModelDoxygen $a
    if (-not [string]::IsNullOrWhiteSpace($modelDoxygen)) {
        [void]$page.AppendLine($modelDoxygen.Trim())
        [void]$page.AppendLine("")
        [void]$page.AppendLine("For the formulation taxonomy and historical context, see @ref mathematical_formulations.")
        [void]$page.AppendLine("")
    }
    [void]$page.AppendLine("## Minimal API")
    [void]$page.AppendLine("")
    [void]$page.AppendLine("~~~~{.cs}")
    [void]$page.AppendLine("IUlsSolver solver = new $($a.Class)();")
    [void]$page.AppendLine("UlsSolveResult result = solver.Solve(problem);")
    [void]$page.AppendLine("~~~~")
    [void]$page.AppendLine("")
    [void]$page.AppendLine("## Scientific source")
    [void]$page.AppendLine("")
    if ([string]::IsNullOrWhiteSpace($a.Doi)) {
        [void]$page.AppendLine($a.Publication)
    } else {
        [void]$page.AppendLine("[$($a.Publication)](https://doi.org/$($a.Doi))")
    }
    [void]$page.AppendLine("")
    [void]$page.AppendLine("## Full class reference")
    [void]$page.AppendLine("")
    [void]$page.AppendLine("Open @ref $($a.Class) for constructors, members and source-level documentation.")
    Set-Content -LiteralPath (Join-Path $GeneratedRoot ("algorithm-" + $a.Slug + ".md")) -Value $page.ToString() -Encoding UTF8
}

# Doxygen API.
#
# docs/Doxyfile intentionally excludes */Documentation/* so generated build
# artifacts are never recursively parsed.  Generated Markdown sources live
# under Documentation\generated; therefore they must be passed to Doxygen as
# explicit files (the same approach used by the pre-refactor catalog page), not
# as an INPUT directory.  An explicit INPUT file is processed while the
# directory-level exclusion remains effective for all other build artifacts.
$generatedMarkdownInputs = @(
    Get-ChildItem -LiteralPath $GeneratedRoot -File -Filter "*.md" |
        Sort-Object Name |
        Select-Object -ExpandProperty FullName
)

$expectedGeneratedPageCount = $algorithms.Count + 1 # catalog + one page per strategy
if ($generatedMarkdownInputs.Count -ne $expectedGeneratedPageCount) {
    throw "Generated documentation source count mismatch. Expected $expectedGeneratedPageCount Markdown files, found $($generatedMarkdownInputs.Count)."
}

$inputs = @(
    (Join-Path $RepoRoot "src"),
    (Join-Path $PSScriptRoot "pages"),
    $MainPagePath
) + $generatedMarkdownInputs

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
if ($LASTEXITCODE -ne 0) { throw "Doxygen documentation build failed with exit code $LASTEXITCODE." }
$ApiHtml = Join-Path $DoxygenRoot "html"
if (-not (Test-Path -LiteralPath (Join-Path $ApiHtml "index.html"))) { throw "Doxygen did not generate html/index.html." }
$ApiSite = Join-Path $SiteRoot "api"
New-Item -ItemType Directory -Path $ApiSite -Force | Out-Null
Copy-Item -Path (Join-Path $ApiHtml "*") -Destination $ApiSite -Recurse -Force

# Custom card portal.
function New-AlgorithmCard([object]$a) {
    $search = "$($a.Name) $($a.Class) $($a.Family) $($a.Applicability) $($a.Publication)".ToLowerInvariant()
    $summary = $a.Implementation
    return "<a class=`"algorithm-card`" data-group=`"$($a.Group)`" data-search=`"$(Escape-Html $search)`" href=`"algorithms/$($a.Slug).html`"><div class=`"card-top`"><span class=`"method-chip $($a.Group)`">$(Escape-Html $a.GroupLabel)</span><span class=`"family-label`">$(Escape-Html $a.Family)</span></div><h4>$(Escape-Html $a.Name)</h4><code>$(Escape-Html $a.Class)</code><p>$(Escape-Html $summary)</p><div class=`"card-footer`"><span>$(Escape-Html $a.Time)</span><strong>Open &rarr;</strong></div></a>"
}

$cardHtml = @{}
foreach ($groupName in @("exact","optimization","cutting","heuristic")) {
    $builder = New-Object System.Text.StringBuilder
    foreach ($a in $algorithms | Where-Object Group -eq $groupName) { [void]$builder.AppendLine((New-AlgorithmCard $a)) }
    $cardHtml[$groupName] = $builder.ToString().TrimEnd()
}

$PortalTemplate = Get-Content -LiteralPath (Join-Path $PSScriptRoot "portal\index.html") -Raw -Encoding UTF8
$Portal = $PortalTemplate.Replace("{{VERSION}}", (Escape-Html $displayVersion)).Replace("{{COMMIT}}", (Escape-Html $commitShort))
$Portal = $Portal.Replace("{{TOTAL_COUNT}}", [string]$algorithms.Count)
$Portal = $Portal.Replace("{{DIRECT_EXACT_COUNT}}", [string]$directExact.Count)
$Portal = $Portal.Replace("{{OPTIMIZATION_COUNT}}", [string]$optimization.Count)
$Portal = $Portal.Replace("{{CUTTING_COUNT}}", [string]$cutting.Count)
$Portal = $Portal.Replace("{{HEURISTIC_COUNT}}", [string]$heuristicGroup.Count)
$Portal = $Portal.Replace("{{EXACT_CARDS}}", $cardHtml["exact"])
$Portal = $Portal.Replace("{{OPTIMIZATION_CARDS}}", $cardHtml["optimization"])
$Portal = $Portal.Replace("{{CUTTING_CARDS}}", $cardHtml["cutting"])
$Portal = $Portal.Replace("{{HEURISTIC_CARDS}}", $cardHtml["heuristic"])
Set-Content -LiteralPath (Join-Path $SiteRoot "index.html") -Value $Portal -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "portal\styles.css") -Destination (Join-Path $SiteRoot "styles.css") -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "portal\app.js") -Destination (Join-Path $SiteRoot "app.js") -Force
New-Item -ItemType Directory -Path (Join-Path $SiteRoot "assets") -Force | Out-Null
Copy-Item -Path (Join-Path $PSScriptRoot "assets\*") -Destination (Join-Path $SiteRoot "assets") -Recurse -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "assets\algorithms-icon.ico") -Destination (Join-Path $SiteRoot "favicon.ico") -Force

# One user-friendly portal page per algorithm.
$AlgorithmSite = Join-Path $SiteRoot "algorithms"
New-Item -ItemType Directory -Path $AlgorithmSite -Force | Out-Null
$AlgorithmTemplate = Get-Content -LiteralPath (Join-Path $PSScriptRoot "portal\algorithm.html") -Raw -Encoding UTF8
foreach ($a in $algorithms) {
    $namespaceLine = if ($a.Namespace -eq "ULSAlgorithms") { "" } else { "using $($a.Namespace);`r`n" }
    $usage = @"
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Models;
$namespaceLine
var problem = new UlsProblem(
    demands:             [20.0, 30.0, 25.0, 40.0],
    setupCosts:          [200.0, 200.0, 200.0, 200.0],
    unitProductionCosts: [0.0, 0.0, 0.0, 0.0],
    holdingCosts:        [4.0, 4.0, 4.0, 0.0]);

IUlsSolver solver = new $($a.Class)();
var result = solver.Solve(problem);

Console.WriteLine(result.Status);
Console.WriteLine(result.ObjectiveValue);
"@
    $publicationHtml = if ([string]::IsNullOrWhiteSpace($a.Doi)) {
        Escape-Html $a.Publication
    } else {
        "<a href=`"https://doi.org/$($a.Doi)`">$(Escape-Html $a.Publication)</a> &middot; DOI $(Escape-Html $a.Doi)"
    }
    $sourceUrl = "https://github.com/Lemoine-OR/ULSAlgorithms/blob/main/$($a.SourcePath)"
    $apiHref = "../api/algorithm_$($a.Slug).html"
    $page = $AlgorithmTemplate.Replace("{{NAME}}", (Escape-Html $a.Name)).Replace("{{CLASS}}", (Escape-Html $a.Class))
    $page = $page.Replace("{{GROUP}}", (Escape-Html $a.Group)).Replace("{{GROUP_LABEL}}", (Escape-Html $a.GroupLabel))
    $page = $page.Replace("{{FAMILY}}", (Escape-Html $a.Family)).Replace("{{TIME}}", (Escape-Html $a.Time)).Replace("{{SPACE}}", (Escape-Html $a.Space))
    $page = $page.Replace("{{APPLICABILITY}}", (Escape-Html $a.Applicability)).Replace("{{DESCRIPTION}}", (Escape-Html $a.Description))
    $page = $page.Replace("{{HOW_IT_WORKS}}", (Escape-Html $a.HowItWorks)).Replace("{{IMPLEMENTATION}}", (Escape-Html $a.Implementation))
    $page = $page.Replace("{{MATHEMATICAL_MODEL}}", (Get-FormulationModelHtml $a)).Replace("{{MATHJAX_HEAD}}", (Get-MathJaxHead $a))
    $page = $page.Replace("{{USAGE_CODE}}", (Escape-Html $usage.Trim())).Replace("{{PUBLICATION_HTML}}", $publicationHtml)
    $page = $page.Replace("{{SOURCE_URL}}", $sourceUrl).Replace("{{API_HREF}}", $apiHref)
    $page = $page.Replace("{{VERSION}}", (Escape-Html $displayVersion)).Replace("{{COMMIT}}", (Escape-Html $commitShort))
    Set-Content -LiteralPath (Join-Path $AlgorithmSite ($a.Slug + ".html")) -Value $page -Encoding UTF8
}
New-Item -ItemType File -Path (Join-Path $SiteRoot ".nojekyll") -Force | Out-Null

$requiredOutputs = @("index.html","styles.css","app.js","favicon.ico","api\index.html","api\algorithm_catalog.html","api\getting_started.html","api\method_families.html","api\simple_api.html","api\complexity_applicability.html","api\validation_benchmarks.html")
$missing = @()
foreach ($relative in $requiredOutputs) { if (-not (Test-Path -LiteralPath (Join-Path $SiteRoot $relative))) { $missing += $relative } }
foreach ($a in $algorithms) {
    foreach ($relative in @("algorithms\$($a.Slug).html","api\algorithm_$($a.Slug).html")) {
        if (-not (Test-Path -LiteralPath (Join-Path $SiteRoot $relative))) { $missing += $relative }
    }
}
if ($missing.Count -gt 0) { throw "Documentation site is incomplete. Missing: $($missing -join ', ')" }

& (Join-Path $PSScriptRoot "Test-DocumentationLinks.ps1") -SiteRoot $SiteRoot

# Windows PowerShell 5.1 can misinterpret non-ASCII literals in a UTF-8
# script without a BOM. Keep this build script ASCII-only and fail the
# documentation build if common mojibake markers appear in generated text.
$badEncodingChars = @(
    [char]0x00C2, # capital A with circumflex: common UTF-8/cp1252 marker
    [char]0x00C3, # capital A with tilde: common UTF-8/cp1252 marker
    [char]0x00E2, # lower-case a with circumflex: common punctuation marker
    [char]0xFFFD  # Unicode replacement character
)
$encodingProblems = @()
foreach ($file in Get-ChildItem -LiteralPath $SiteRoot -Recurse -File | Where-Object { $_.Extension -in @('.html','.css','.js') }) {
    $content = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
    foreach ($badChar in $badEncodingChars) {
        if ($content.Contains([string]$badChar)) {
            $encodingProblems += $file.FullName
            break
        }
    }
}
if ($encodingProblems.Count -gt 0) {
    $relativeProblems = @($encodingProblems | Sort-Object -Unique | ForEach-Object { $_.Substring($SiteRoot.Length).TrimStart([char]92,[char]47) })
    throw "Documentation encoding validation failed. Possible mojibake in: $($relativeProblems -join ', ')"
}
Write-Host "Documentation encoding validation passed: no common mojibake markers found." -ForegroundColor Green

Write-Host ""
Write-Host "ULSAlgorithms documentation successfully built and link-validated." -ForegroundColor Green
Write-Host "Portal: $SiteRoot"
Write-Host "Direct exact: $($directExact.Count)"
Write-Host "Optimization formulations: $($optimization.Count)"
Write-Host "Cutting-plane methods: $($cutting.Count)"
Write-Host "Heuristics: $($heuristicGroup.Count)"
Write-Host "Public strategies: $($algorithms.Count)"
