[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$version = "15.1.1"
$archiveName = "windows_10_cmake_Release_Graphviz-$version-win64.zip"

$downloadUrl = (
    "https://gitlab.com/api/v4/projects/4207231/packages/generic/" +
    "graphviz-releases/$version/$archiveName"
)

# Official SHA-256 published next to the 64-bit ZIP on graphviz.org.
$expectedSha256 =
    "e8256ef077e601d9f284378d96cd17faa7910832cf6bb85c43005e66ec2f255e"

$temporaryRoot = if ([string]::IsNullOrWhiteSpace($env:RUNNER_TEMP)) {
    Join-Path ([System.IO.Path]::GetTempPath()) "ulsalgorithms-tools"
}
else {
    Join-Path $env:RUNNER_TEMP "ulsalgorithms-tools"
}

$archivePath =
    Join-Path $temporaryRoot $archiveName

$installPath =
    Join-Path $temporaryRoot "graphviz-$version"

New-Item `
    -ItemType Directory `
    -Path $temporaryRoot `
    -Force |
    Out-Null

if (Test-Path -LiteralPath $archivePath) {
    Remove-Item `
        -LiteralPath $archivePath `
        -Force
}

Write-Host "Downloading official Graphviz $version x64 ZIP."

$maximumAttempts = 4
$downloadSucceeded = $false

for ($attempt = 1; $attempt -le $maximumAttempts; $attempt++) {
    try {
        Invoke-WebRequest `
            -Uri $downloadUrl `
            -OutFile $archivePath `
            -UseBasicParsing

        $downloadSucceeded = $true
        break
    }
    catch {
        if ($attempt -eq $maximumAttempts) {
            throw
        }

        $delaySeconds = 5 * $attempt

        Write-Warning (
            "Graphviz download attempt $attempt/$maximumAttempts failed. " +
            "Retrying in $delaySeconds second(s). Error: $($_.Exception.Message)"
        )

        Start-Sleep `
            -Seconds $delaySeconds
    }
}

if (-not $downloadSucceeded) {
    throw "Unable to download Graphviz $version."
}

$actualSha256 = (
    Get-FileHash `
        -LiteralPath $archivePath `
        -Algorithm SHA256
).Hash.ToLowerInvariant()

if ($actualSha256 -ne $expectedSha256) {
    throw (
        "Graphviz SHA-256 mismatch. " +
        "Expected $expectedSha256, got $actualSha256."
    )
}

Write-Host "Graphviz SHA-256 validation passed."

if (Test-Path -LiteralPath $installPath) {
    Remove-Item `
        -LiteralPath $installPath `
        -Recurse `
        -Force
}

New-Item `
    -ItemType Directory `
    -Path $installPath `
    -Force |
    Out-Null

Expand-Archive `
    -LiteralPath $archivePath `
    -DestinationPath $installPath `
    -Force

$dot = @(
    Get-ChildItem `
        -LiteralPath $installPath `
        -Filter "dot.exe" `
        -File `
        -Recurse
)[0]

if ($null -eq $dot) {
    throw "Graphviz archive does not contain dot.exe."
}

$graphvizBin = $dot.Directory.FullName

# Make Graphviz available to the remainder of this PowerShell process.
$env:PATH =
    "$graphvizBin$([System.IO.Path]::PathSeparator)$env:PATH"

# Make Graphviz available to subsequent GitHub Actions steps.
if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_PATH)) {
    $graphvizBin |
        Out-File `
            -FilePath $env:GITHUB_PATH `
            -Append `
            -Encoding utf8
}

Write-Host "Graphviz bin directory: $graphvizBin"

& $dot.FullName -V

if ($LASTEXITCODE -ne 0) {
    throw "Graphviz dot -V failed with exit code $LASTEXITCODE."
}

Write-Host "Verified official Graphviz $version installation."
