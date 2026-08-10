Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = & (Join-Path $PSScriptRoot 'Get-RepositoryRoot.ps1')
$files = @(
    Get-ChildItem -LiteralPath (Join-Path $root 'build') -Recurse -File -Filter '*.ps1' -ErrorAction SilentlyContinue
    Get-ChildItem -LiteralPath (Join-Path $root 'tools') -Recurse -File -Filter '*.ps1' -ErrorAction SilentlyContinue
    Get-ChildItem -LiteralPath (Join-Path $root 'docs')  -Recurse -File -Filter '*.ps1' -ErrorAction SilentlyContinue
) | Sort-Object FullName -Unique

if ($files.Count -eq 0) {
    throw 'No PowerShell automation scripts were found.'
}

$failures = New-Object System.Collections.Generic.List[string]

foreach ($file in $files) {
    $tokens = $null
    $errors = $null

    [void][System.Management.Automation.Language.Parser]::ParseFile(
        $file.FullName,
        [ref]$tokens,
        [ref]$errors)

    foreach ($error in @($errors)) {
        $failures.Add(
            "$($file.FullName):$($error.Extent.StartLineNumber):$($error.Extent.StartColumnNumber): $($error.Message)")
    }

    $source = [System.IO.File]::ReadAllText($file.FullName)

    if ($source -match '\[(?:System\.)?IO\.Path\]::GetRelativePath') {
        $failures.Add(
            "$($file.FullName): uses System.IO.Path.GetRelativePath, which is not compatible with Windows PowerShell 5.1.")
    }
}
if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Error $_ }
    throw "PowerShell syntax validation failed with $($failures.Count) error(s)."
}

Write-Host "PowerShell syntax validation passed for $($files.Count) script(s)."
