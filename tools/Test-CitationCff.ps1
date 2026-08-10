Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')
$path = Join-Path $root 'CITATION.cff'

if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
    throw 'CITATION.cff is missing.'
}

$bytes = [System.IO.File]::ReadAllBytes($path)
if ($bytes.Length -eq 0) {
    throw 'CITATION.cff is empty.'
}

if ($bytes.Length -ge 3 -and
    $bytes[0] -eq 0xEF -and
    $bytes[1] -eq 0xBB -and
    $bytes[2] -eq 0xBF) {
    throw 'CITATION.cff must be UTF-8 without BOM.'
}

$text = [System.IO.File]::ReadAllText($path)

if ($text.Contains("`t")) {
    throw 'CITATION.cff must not contain tab indentation.'
}

$requiredPatterns = @(
    '(?m)^cff-version:\s*1\.2\.0\s*$',
    '(?m)^message:\s*".+"\s*$',
    '(?m)^title:\s*ULSAlgorithms\s*$',
    '(?m)^type:\s*software\s*$',
    '(?m)^authors:\s*$',
    '(?m)^\s+-\s+family-names:\s*Lemoine\s*$',
    '(?m)^\s+given-names:\s*David\s*$',
    '(?m)^repository-code:\s*"https://github\.com/Lemoine-OR/ULSAlgorithms"\s*$',
    '(?m)^license:\s*MIT\s*$'
)

foreach ($pattern in $requiredPatterns) {
    if ($text -notmatch $pattern) {
        throw "CITATION.cff failed required-field validation: $pattern"
    }
}

Write-Host 'CITATION.cff validation passed.'
