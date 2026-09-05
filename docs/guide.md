# Using High FPS Support

[English](guide.md) · [Русский](guide.ru.md) · [Back to README](../README.md)

## Find the correct folder

In Steam, open Terraria's **Properties → Installed Files → Browse**. The folder must contain the original `Terraria.exe` and a `Content` directory. Select this folder in the launcher, not the ZIP folder, a saves directory or a tModLoader installation. If more than one Steam library is installed, check the displayed path.

The launcher and all its extracted files can stay in a separate folder. It copies only its installed components into the game folder. The language menu remembers your choice; it does not change the game's own language.

## Install, play and update

- **Install & play:** check compatibility, install or refresh the mod files, then launch.
- **Play:** validate the existing installation and launch the separate game executable.
- **Install / update only:** install or repair without opening the game.
- **Technical details:** inspect diagnostic information if an operation fails.

Close both Terraria and `Terraria.HighFPS` before changing the installation. Leave Steam running when playing. Steam's ordinary Play button starts the unmodified game; use this launcher for High FPS. Directly opening `Terraria.HighFPS.exe` is possible, but skips the launcher's pre-launch integrity checks.

After updating this mod, extract its new ZIP into a fresh folder and install from there. Do not mix DLLs from different releases. After an update to Terraria itself, check the supported game version before installing again. Do not bypass an incompatibility error or downgrade using files from an unknown source.

## Remove everything added by the mod

Use **Remove High FPS** with the game closed. To remove manually, close the game and delete only these files from its installation directory:

```text
Terraria.HighFPS.exe
HighFPS.Support.dll
HighFPS.Support.install.txt
HighFPS.Support.log
```

Then delete the extracted launcher folder if you no longer need it. Optional: remove `%LOCALAPPDATA%\TerrariaHighFPS\game-path.txt` and `language.txt` to clear launcher preferences. Do not delete `Terraria.exe`, `Content`, Steam files or save folders. If an installation error preserved a recovery folder, inspect its error message before deleting that folder.

The mod does not delete worlds or characters. Terraria may retain **Frame Skip: Off** in its own settings; choose your preferred Frame Skip setting in the original game after removal.

## Troubleshooting

| Symptom | Next step |
| --- | --- |
| Unsupported Terraria version | Use a mod release explicitly compatible with that exact version; this release supports only 1.4.5.8. |
| Missing Mono.Cecil or another launcher file | Extract the entire ZIP again to a fresh folder and verify the checksums. |
| Installation changed / integrity check failed | Close the game, verify the launcher download, then use **Install / update only**. If the original game is suspect, verify Terraria's files in Steam first. |
| File in use | Close every Terraria instance, including one still exiting. Retry. |
| Access denied | Check that your account can write to the selected game folder. Use Steam's storage settings to move the game to a library you can write to. The launcher does not automatically elevate. |
| Missing XNA / .NET | Run the original game from Steam and complete its prerequisites. Do not download individual DLLs from random sites. |
| Steam-related startup error | Start Steam, confirm the original game launches, then retry with this launcher. |
| Still looks like 60 FPS | Check Windows' display refresh rate and available CPU/GPU headroom. Verify you launched `Terraria.HighFPS.exe`; compare the same scene. |
| Visual artifact or incorrect aim | Reproduce it in the original game, note refresh rate/zoom/resolution, and file an issue with steps. Use vanilla while investigating. |
| SmartScreen or antivirus warning | Check the publisher/source and hashes. Keep protection enabled. Unsigned does not mean malicious, but it does not establish trust either. |

The local `HighFPS.Support.log` records startup and runtime errors. No log may be created if the game fails before the runtime loads. Launcher **Technical details** can explain install failures. Remove usernames, local paths and other private data before posting diagnostics. Never upload the game executable with a report.
