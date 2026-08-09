Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$version = '1.17.0'
$url = 'https://github.com/doxygen/doxygen/releases/download/Release_1_17_0/doxygen-1.17.0.windows.x64.bin.zip'
$expected = '94594407c4cbca3049d76aacbb05d4a6f7d0f4e93c0de410b825d25ca5621c83'

$tempRoot = if ($env:RUNNER_TEMP) { $env:RUNNER_TEMP } else { [System.IO.Path]::GetTempPath() }
$zip = Join-Path $tempRoot "doxygen-$version.zip"
$dir = Join-Path $tempRoot "doxygen-$version"

Invoke-WebRequest -Uri $url -OutFile $zip
$actual = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()
if ($actual -ne $expected) {
    throw "Doxygen SHA-256 mismatch. Expected $expected, got $actual."
}

Remove-Item -LiteralPath $dir -Recurse -Force -ErrorAction SilentlyContinue
Expand-Archive -LiteralPath $zip -DestinationPath $dir -Force
$exe = Get-ChildItem -LiteralPath $dir -Recurse -Filter 'doxygen.exe' -File | Select-Object -First 1
if ($null -eq $exe) {
    throw 'doxygen.exe was not found in the verified official archive.'
}

if ($env:GITHUB_PATH) {
    Add-Content -LiteralPath $env:GITHUB_PATH -Value $exe.Directory.FullName
}
$env:PATH = "$($exe.Directory.FullName);$env:PATH"
& $exe.FullName --version
if ($LASTEXITCODE -ne 0) {
    throw 'Doxygen executable validation failed.'
}

return $exe.FullName
