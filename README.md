# High FPS Support

**Smoother movement in Terraria, at your display's refresh rate.**  
An unofficial, open-source launcher by **pavlikmeow** · Version **1.1.0**

**English** · [Русский](docs/README.ru.md) · [Deutsch](docs/README.de.md) · [Español](docs/README.es.md) · [Français](docs/README.fr.md) · [Português (Brasil)](docs/README.pt-BR.md) · [简体中文](docs/README.zh-CN.md)

Terraria normally advances its world 60 times a second. High FPS Support draws movement between those updates, making players, enemies, projectiles and dropped items appear smoother on 120/144/165/240 Hz displays. Game speed stays the same. No tModLoader is needed.

![High FPS Support launcher in English](docs/assets/launcher-en.png)

## Before you start

| Requirement | Supported |
| --- | --- |
| Game | **Steam Terraria 1.4.5.8**, original Windows x86 executable |
| System | Windows with .NET Framework 4.x and XNA Framework 4 installed |
| Display | Any; a refresh rate above 60 Hz makes the difference most visible |
| Other versions / platforms | Not supported by this release; tModLoader and other executable patches are not supported |

Use your own licensed copy of Terraria. Run the original game once through Steam to finish its prerequisites. Back up important worlds and characters before trying any mod; this uses your normal Terraria saves.

## Play in a minute

1. Open this repository's **Releases** section. Download `HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip`, then **extract the entire archive** to a folder you control. Do not run it inside the ZIP or move only the EXE.
2. [Check the download](#check-your-download), close Terraria and leave Steam running.
3. Open **`HighFpsSupport.exe`**. Choose your language using **Language / Язык** at the top right. The choice is remembered.
4. Check the detected game folder. If necessary, browse to the folder containing **`Terraria.exe` and `Content`**. In Steam: Terraria → Properties → Installed Files → Browse.
5. Click **Install & play**. On later visits, click **Play**.

The mod enables Terraria's **Frame Skip: Off** mode for the High FPS executable. Set your display's actual refresh rate in Windows. Higher rendering rates need spare CPU/GPU capacity; this mod does not guarantee a particular FPS or accelerate a slow simulation.

**Update:** close Terraria, extract the new launcher release to a fresh folder and choose **Install / update only**, then **Play**. After a Terraria update, use a mod release that explicitly supports that game version. An unsupported version is rejected.

**Go back:** launch Terraria normally from Steam. To uninstall, close the game and choose **Remove High FPS**. Your original executable and saves remain. [Manual removal and troubleshooting](docs/guide.md).

## What it changes

The launcher reads your installed game and creates a separate **`Terraria.HighFPS.exe`** with **`HighFPS.Support.dll`** beside it. It also writes installation metadata and a local diagnostic log. **`Terraria.exe` is not overwritten or renamed.** Your selected folder and launcher language are stored locally.

The launcher has **no telemetry, auto-updater, login or runtime downloads**. It installs no service, driver or scheduled task. Terraria and Steam retain their own normal network behavior. See the [file and permission inventory](SECURITY.md).

Under the hood, Mono.Cecil inserts three calls around Terraria's update and draw routines. The runtime captures state before a real tick, interpolates positions while drawing, and restores simulation positions after drawing. The 60 Hz simulation and network routines are not sped up. Interpolation can add up to one tick of visual delay; it does not create new gameplay updates. [Technical explanation and limits](docs/architecture.md).

## Check your download

The published build's EXE, DLL and archive hashes are listed in [release hashes](docs/release-hashes.md). Each ZIP includes **`SHA256SUMS.txt`** for its contents, plus the project source and build scripts for review.

Before extracting, open PowerShell in the download folder and compare this result with the archive hash for **the same release**:

```powershell
Get-FileHash -Algorithm SHA256 -LiteralPath .\HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip
```

After extracting, open PowerShell in that folder, inspect `verify-release.ps1`, then verify all listed files:

```powershell
powershell -NoProfile -File .\verify-release.ps1
```

If PowerShell blocks scripts, you can compare individual files with `Get-FileHash` and `SHA256SUMS.txt`; changing your security policy is unnecessary. Stop if any hash differs.

**A matching hash proves a match to the checksum you trust, not that software is harmless.** These builds are unsigned; Windows may report an unknown publisher or show SmartScreen. No independent security audit or bit-for-bit reproducible build is claimed. Review the [security model](SECURITY.md), inspect the code, or [build it yourself](docs/building.md). Do not disable antivirus to install it.

## Project and credits

[Contributing](CONTRIBUTING.md) · [Build from source](docs/building.md) · [Verification results](docs/verification.md) · [Security](SECURITY.md) · [Third-party notices](THIRD-PARTY-NOTICES.md)

Project-owned code and documentation are available under the [MIT License](LICENSE), copyright © 2026 pavlikmeow. Mono.Cecil has its own MIT notice. Thanks to [TerrariaHighFPS](https://github.com/Yukurotei/TerrariaHighFPS) for the publicly described interpolation approach; attribution is not a license to reuse its source.

This is an independent fan project, not affiliated with or endorsed by Re-Logic, Valve or Microsoft. Terraria belongs to [Re-Logic](https://store.steampowered.com/app/105600/Terraria/); Steam and Microsoft/XNA names belong to their respective owners. No game executable, game assets or XNA runtime are distributed. The MIT license grants no rights to those products; their terms and applicable law still apply. See [notices and license scope](THIRD-PARTY-NOTICES.md).
