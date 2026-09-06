param([string]$Directory)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $Directory) {
    $Directory = if (Test-Path -LiteralPath (Join-Path $PSScriptRoot 'SHA256SUMS.txt')) { $PSScriptRoot } else { Join-Path (Split-Path -Parent $PSScriptRoot) 'dist' }
}
$base = (Resolve-Path -LiteralPath $Directory).Path.TrimEnd('\', '/')
$manifest = Join-Path $base 'SHA256SUMS.txt'
$seen = @{}

# Reject ambiguous entries and paths outside the selected release folder.
foreach ($line in Get-Content -LiteralPath $manifest) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    if ($line -notmatch '^([A-Fa-f0-9]{64}) \*(.+)$') { throw "Invalid checksum line: $line" }
    $expected = $Matches[1]
    $name = $Matches[2]
    if ([IO.Path]::IsPathRooted($name) -or $name.Contains(':') -or $name -match '(^|[\\/])\.\.?([\\/]|$)') { throw "Unsafe checksum path: $name" }
    $path = [IO.Path]::GetFullPath((Join-Path $base $name))
    if (-not $path.StartsWith($base + '\', [StringComparison]::OrdinalIgnoreCase)) { throw "Path escapes release: $name" }
    if ($seen.ContainsKey($path)) { throw "Duplicate checksum entry: $name" }
    $seen[$path] = $true
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing file: $name" }
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $expected) { throw "SHA-256 mismatch: $name" }
    Write-Host "OK  $name"
}
if ($seen.Count -eq 0) { throw 'The checksum manifest is empty.' }
Write-Host "Verified $($seen.Count) files. Compare this manifest with the publisher's separately obtained release hashes."
Write-Host 'Checksums detect changed files; they are not a signature or a malware scan.'
