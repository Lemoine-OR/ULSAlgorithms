Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')
$path = Join-Path $root 'CHANGELOG.md'

if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
    throw 'CHANGELOG.md is missing.'
}

$text = [IO.File]::ReadAllText($path)

if ($text -notmatch '(?m)^## \[Unreleased\]\s*$') {
    throw 'CHANGELOG.md must contain an Unreleased section.'
}

if ($text -match '(?m)^## Earlier 0\.x releases\s*$') {
    throw 'CHANGELOG.md must not collapse historical 0.x releases into a generic summary.'
}

$expected =
    29..1 |
    ForEach-Object { "0.$_.0" }

$matches =
    [regex]::Matches(
        $text,
        '(?m)^## \[(0\.\d+\.0)\](?:\s+-\s+\d{4}-\d{2}-\d{2})?\s*$')

$actual =
    @(
        $matches |
        ForEach-Object { $_.Groups[1].Value }
    )

if ($actual.Count -ne $expected.Count) {
    throw "Expected $($expected.Count) explicit 0.x release entries, found $($actual.Count)."
}

for ($i = 0; $i -lt $expected.Count; $i++) {
    if ($actual[$i] -ne $expected[$i]) {
        throw "Unexpected changelog release order at index $i. Expected '$($expected[$i])', found '$($actual[$i])'."
    }
}

foreach ($version in $expected) {
    $count =
        @(
            $actual |
            Where-Object { $_ -eq $version }
        ).Count

    if ($count -ne 1) {
        throw "Expected exactly one changelog entry for '$version', found $count."
    }
}

if ($text -notmatch '(?m)^## \[0\.29\.0\]\s+-\s+2026-08-11\s*$') {
    throw 'The release-ready v0.29.0 changelog heading is missing or has the wrong date.'
}

Write-Host 'Changelog history validation passed: explicit entries 0.1.0 through 0.29.0 are complete and ordered.'
