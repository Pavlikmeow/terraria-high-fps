# Build and verify from source

[English](building.md) · [Русский](building.ru.md) · [README](../README.md)

## Prerequisites

- Windows, PowerShell 5.1, and the .NET Framework 4.x C# compiler (`Framework\v4.0.30319\csc.exe` under the Windows directory).
- Your locally installed **Steam Terraria 1.4.5.8** and **Microsoft XNA Framework 4**. Run the original game once to install prerequisites.
- Internet for the first download of the pinned Mono.Cecil package, or an already populated, valid `.deps` cache.

Python, tModLoader, Visual Studio and a separate .NET SDK are not required. Read scripts before running them. Do not replace missing proprietary dependencies with DLL downloads from third-party sites.

## Build

In PowerShell at the repository root:

```powershell
powershell -NoProfile -File .\scripts\build.ps1
```

If discovery does not find the game:

```powershell
powershell -NoProfile -File .\scripts\build.ps1 -TerrariaExe 'D:\SteamLibrary\steamapps\common\Terraria\Terraria.exe'
```

The script downloads only the pinned [Mono.Cecil 0.11.6 package](https://www.nuget.org/packages/Mono.Cecil/0.11.6) from NuGet, verifies package/DLL SHA-256 against the constants documented in [notices](../THIRD-PARTY-NOTICES.md), compiles the runtime and launcher, and packages an allowlist of release files. A normal build does not install a mod into the game folder.

Outputs:

```text
dist/HighFpsSupport.exe
dist/HighFPS.Support.dll
dist/Mono.Cecil.dll
dist/SHA256SUMS.txt
release/HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip
release/SHA256SUMS.txt
```

Documentation, notices, a verifier and the project source/build/test scripts are included in `dist` and the ZIP. Each build writes its checksums to `release/release-hashes.md`. The published checksums in `docs/release-hashes.md` stay unchanged. The archive contains binary hashes without a circular self-hash.

If Windows blocks scripts, first review the script and its imported `scripts/common.ps1`. On your own computer, if your policy permits it, you can allow this one PowerShell process to execute the reviewed build:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

This does not change the machine-wide policy and does not bypass an organization's enforced policy. The same process-only option can be used for the reviewed test scripts below. Manual `Get-FileHash` verification works without running project scripts.

## Validate

```powershell
powershell -NoProfile -File .\scripts\verify-release.ps1
powershell -NoProfile -File .\scripts\verify-release.ps1 -Directory .\release
powershell -NoProfile -File .\scripts\test.ps1
```

`test.ps1` builds and runs patch, installation, JIT, interpolation and UI checks using a disposable copy of locally installed game files. It does not require writing the mod into your live game installation. `-SkipBuild` uses the current build; `-TerrariaExe` selects the game explicitly. Test artifacts may contain proprietary game data: never publish them.

For UI/localization checks without a game installation:

```powershell
powershell -NoProfile -File .\scripts\test-launcher.ps1
```

The GitHub workflow runs the game-free checks, PowerShell syntax validation and a guard against tracked proprietary binaries. It cannot run the full game-dependent suite without the game and XNA. Automated checks do not establish smoothness in real gameplay; also test movement, aiming, menus, teleports and multiplayer where relevant on a high-refresh display.

## What a local build proves

Building lets you choose which reviewed source and dependencies to run. It does not automatically prove that a downloaded release was built from that source. The current .NET Framework build is **not claimed to be bit-for-bit reproducible**; generated assembly identifiers and archive metadata can change hashes between builds. Compare behavior/source and verify the dependency pins rather than expecting a local EXE hash to match a release.
