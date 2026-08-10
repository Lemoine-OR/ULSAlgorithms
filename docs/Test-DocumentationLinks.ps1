[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SiteRoot
)

$ErrorActionPreference = "Stop"

$resolvedRoot = (Resolve-Path -LiteralPath $SiteRoot).Path
$rootPrefix = $resolvedRoot.TrimEnd(
    [IO.Path]::DirectorySeparatorChar,
    [IO.Path]::AltDirectorySeparatorChar
) + [IO.Path]::DirectorySeparatorChar

Write-Host ""
Write-Host "Validating local HTML links under:"
Write-Host $resolvedRoot

$htmlFiles = Get-ChildItem -LiteralPath $resolvedRoot -Recurse -File -Filter "*.html"
$broken = @()

$attributeRegex = [regex]@'
(?is)\b(?:href|src)\s*=\s*(["'])(.*?)\1
'@

foreach ($htmlFile in $htmlFiles) {
    $content = Get-Content -LiteralPath $htmlFile.FullName -Raw

    foreach ($match in $attributeRegex.Matches($content)) {
        $rawLink = [System.Net.WebUtility]::HtmlDecode($match.Groups[2].Value).Trim()

        if ([string]::IsNullOrWhiteSpace($rawLink)) { continue }

        if (
            $rawLink.StartsWith("#") -or
            $rawLink.StartsWith("//") -or
            $rawLink -match '^(?i)(https?:|mailto:|tel:|javascript:|data:|ftp:)'
        ) {
            continue
        }

        $pathPart = ($rawLink -split '#', 2)[0]
        $pathPart = ($pathPart -split '\?', 2)[0]

        if ([string]::IsNullOrWhiteSpace($pathPart)) { continue }

        try { $pathPart = [Uri]::UnescapeDataString($pathPart) } catch {}

        $pathPart = $pathPart.Replace('/', [IO.Path]::DirectorySeparatorChar)

        if ($pathPart.StartsWith([IO.Path]::DirectorySeparatorChar)) {
            $candidate = Join-Path $resolvedRoot $pathPart.TrimStart([IO.Path]::DirectorySeparatorChar)
        }
        else {
            $candidate = Join-Path $htmlFile.Directory.FullName $pathPart
        }

        try {
            $candidate = [IO.Path]::GetFullPath($candidate)
        }
        catch {
            $broken += [pscustomobject]@{
                Source = $htmlFile.FullName.Substring($rootPrefix.Length)
                Link   = $rawLink
                Target = "<invalid path>"
            }
            continue
        }

        if (
            -not $candidate.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase) -and
            -not $candidate.Equals($resolvedRoot, [StringComparison]::OrdinalIgnoreCase)
        ) {
            $broken += [pscustomobject]@{
                Source = $htmlFile.FullName.Substring($rootPrefix.Length)
                Link   = $rawLink
                Target = "<outside documentation root>"
            }
            continue
        }

        if (Test-Path -LiteralPath $candidate -PathType Container) {
            $candidate = Join-Path $candidate "index.html"
        }

        if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            $targetDisplay = if ($candidate.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
                $candidate.Substring($rootPrefix.Length)
            } else { $candidate }

            $broken += [pscustomobject]@{
                Source = $htmlFile.FullName.Substring($rootPrefix.Length)
                Link   = $rawLink
                Target = $targetDisplay
            }
        }
    }
}

$report = Join-Path (Split-Path -Parent $resolvedRoot) "broken-links.txt"

if ($broken.Count -gt 0) {
    $uniqueBroken = $broken | Sort-Object Source, Link, Target -Unique
    $uniqueBroken |
        Format-Table Source, Link, Target -AutoSize |
        Out-String -Width 320 |
        Set-Content -LiteralPath $report -Encoding UTF8

    Write-Host ""
    Write-Host "Broken local documentation links detected: $($uniqueBroken.Count)" -ForegroundColor Red
    Write-Host "Full report: $report" -ForegroundColor Yellow

    $uniqueBroken |
        Select-Object -First 40 |
        Format-Table Source, Link, Target -AutoSize |
        Out-String -Width 260 |
        Write-Host

    throw "Documentation link validation failed with $($uniqueBroken.Count) broken local link(s)."
}

if (Test-Path -LiteralPath $report) {
    Remove-Item -LiteralPath $report -Force
}

Write-Host ""
Write-Host "Documentation link validation passed: no broken local href/src targets found." -ForegroundColor Green
