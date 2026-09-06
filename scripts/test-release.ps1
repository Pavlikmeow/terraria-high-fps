# Verify both acceptance and rejection without a game installation or network access.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$fixture = Join-Path $root ('.build\checksums-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $fixture | Out-Null
$file = Join-Path $fixture 'sample.txt'
$manifest = Join-Path $fixture 'SHA256SUMS.txt'
Set-Content -LiteralPath $file -Value 'checksum fixture' -Encoding ASCII
$hash = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash
"$hash *sample.txt" | Set-Content -LiteralPath $manifest -Encoding ASCII
& (Join-Path $PSScriptRoot 'verify-release.ps1') -Directory $fixture
$invalid = @(
    ('0' * 64 + ' *sample.txt'),
    ($hash + ' *missing.txt'),
    ($hash + ' *../sample.txt'),
    ($hash + ' *sample.txt' + "`r`n" + $hash + ' *sample.txt'),
    'invalid checksum line',
    ''
)
foreach ($contents in $invalid) {
    $contents | Set-Content -LiteralPath $manifest -Encoding ASCII
    $rejected = $false
    try { & (Join-Path $PSScriptRoot 'verify-release.ps1') -Directory $fixture }
    catch { $rejected = $true }
    if (-not $rejected) { throw 'The verifier accepted invalid or incomplete checksum input.' }
}
Write-Host 'Checksum checks passed: valid files accepted; modified, missing, escaping, duplicate, malformed and empty entries rejected.'
