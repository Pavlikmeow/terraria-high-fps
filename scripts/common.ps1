# Shared, pinned build inputs. No game files are downloaded or redistributed.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-FileHash([string]$Path, [string]$Expected) {
    $actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash
    if ($actual -ne $Expected) { throw "SHA-256 mismatch: $Path. Expected $Expected; got $actual." }
}

function Get-CecilPath([string]$Root) {
    $deps = Join-Path $Root '.deps'
    $package = Join-Path $deps 'mono.cecil.0.11.6.nupkg'
    $expanded = Join-Path $deps 'mono.cecil.0.11.6'
    $dll = Join-Path $expanded 'lib\net40\Mono.Cecil.dll'
    New-Item -ItemType Directory -Force -Path $deps | Out-Null
    if (-not (Test-Path -LiteralPath $package)) {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        $download = $package + '.download'
        try {
            Invoke-WebRequest -UseBasicParsing -Uri 'https://api.nuget.org/v3-flatcontainer/mono.cecil/0.11.6/mono.cecil.0.11.6.nupkg' -OutFile $download
            Assert-FileHash $download 'D2A23832AAA948BA9A01ACC42B5726E34C5F995958F1B30D45C0E7C70B3A72D5'
            Move-Item -LiteralPath $download -Destination $package
        } finally {
            if (Test-Path -LiteralPath $download) { Remove-Item -LiteralPath $download }
        }
    }
    Assert-FileHash $package 'D2A23832AAA948BA9A01ACC42B5726E34C5F995958F1B30D45C0E7C70B3A72D5'
    if (-not (Test-Path -LiteralPath $dll)) {
        # ZipFile supports .nupkg on Windows PowerShell 5.1.
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $extract = Join-Path $deps ('cecil-extract-' + [Guid]::NewGuid().ToString('N'))
        [IO.Compression.ZipFile]::ExtractToDirectory($package, $extract)
        New-Item -ItemType Directory -Force -Path $expanded | Out-Null
        Copy-Item -Path (Join-Path $extract '*') -Destination $expanded -Recurse -Force
        $resolved = [IO.Path]::GetFullPath($extract)
        if (-not $resolved.StartsWith([IO.Path]::GetFullPath($deps) + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe extraction path.' }
        Remove-Item -LiteralPath $resolved -Recurse -Force
    }
    Assert-FileHash $dll 'C41BDB9FFD3C5F6E17D2382C1012D73703E035E3F1100245FDD4E08C8DC6EB5B'
    return $dll
}

function Get-CompilerPath {
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
    if (-not (Test-Path -LiteralPath $csc)) { throw '.NET Framework 4 compiler is required (Windows).' }
    return $csc
}

function Get-GamePath([string]$ExplicitPath) {
    if ($ExplicitPath) { return (Resolve-Path -LiteralPath $ExplicitPath).Path }
    $candidates = @('C:\Program Files (x86)\Steam\steamapps\common\Terraria\Terraria.exe', 'C:\Program Files\Steam\steamapps\common\Terraria\Terraria.exe')
    $steam = Get-ItemProperty -LiteralPath 'HKCU:\Software\Valve\Steam' -ErrorAction SilentlyContinue
    if ($steam -and $steam.PSObject.Properties['SteamPath']) {
        $candidates = @(Join-Path $steam.SteamPath 'steamapps\common\Terraria\Terraria.exe') + $candidates
        $libraries = Join-Path $steam.SteamPath 'steamapps\libraryfolders.vdf'
        if (Test-Path -LiteralPath $libraries) {
            foreach ($match in [regex]::Matches((Get-Content -Raw -LiteralPath $libraries), '"path"\s+"(?<path>[^"]+)"')) {
                $library = $match.Groups['path'].Value.Replace('\\', '\')
                $candidates += Join-Path $library 'steamapps\common\Terraria\Terraria.exe'
            }
        }
    }
    foreach ($candidate in $candidates | Select-Object -Unique) {
        if (Test-Path -LiteralPath $candidate) { return (Resolve-Path -LiteralPath $candidate).Path }
    }
    throw 'Terraria.exe was not found. Pass -TerrariaExe with its full path.'
}

function Get-XnaReferences {
    $gac = Join-Path $env:WINDIR 'Microsoft.NET\assembly\GAC_32'
    foreach ($name in @('Microsoft.Xna.Framework', 'Microsoft.Xna.Framework.Game', 'Microsoft.Xna.Framework.Graphics')) {
        $file = Join-Path $gac ($name + '\v4.0_4.0.0.0__842cf8be1de50553\' + $name + '.dll')
        if (-not (Test-Path -LiteralPath $file)) { throw "XNA Framework 4 is required: $file" }
        '/reference:' + $file
    }
}
