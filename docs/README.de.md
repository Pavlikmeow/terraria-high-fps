# High FPS Support

**Flüssigere Bewegungen in Terraria, passend zur Bildwiederholrate deines Monitors.**  
Inoffizieller Open-Source-Launcher von **pavlikmeow** · Version **1.1.0**

[English](../README.md) · [Русский](README.ru.md) · **Deutsch** · [Español](README.es.md) · [Français](README.fr.md) · [Português (Brasil)](README.pt-BR.md) · [简体中文](README.zh-CN.md)

Terraria aktualisiert die Spielwelt 60-mal pro Sekunde. Dieser Mod zeichnet Zwischenpositionen für Spieler, Gegner, Projektile und fallengelassene Gegenstände. Auf Bildschirmen mit 120/144/165/240 Hz wirken Bewegungen dadurch flüssiger. Die Spielgeschwindigkeit bleibt gleich; tModLoader ist nicht nötig.

## Voraussetzungen

Unterstützt wird ausschließlich **Steam Terraria 1.4.5.8 für Windows, originales x86-EXE**. .NET Framework 4.x und XNA Framework 4 müssen installiert sein; starte das Originalspiel einmal über Steam. Andere Spielversionen, Plattformen, tModLoader und zusätzliche EXE-Patches werden nicht unterstützt. Du brauchst eine eigene lizenzierte Spielkopie. Sichere wichtige Welten und Charaktere: Es werden die normalen Terraria-Spielstände verwendet.

## In wenigen Schritten spielen

1. Lade in [Releases](https://github.com/Pavlikmeow/terraria-high-fps/releases) dieses Repositorys `HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip` herunter. Entpacke **das gesamte Archiv** in einen eigenen Ordner. Starte es nicht innerhalb des ZIPs und verschiebe nicht nur die EXE.
2. Prüfe den Download wie unten beschrieben. Schließe Terraria und lasse Steam laufen.
3. Öffne **`HighFpsSupport.exe`**. Wähle oben rechts unter **Language / Язык** die Sprache **Deutsch**. Die Auswahl wird gespeichert und ändert nur den Launcher.
4. Prüfe den erkannten Spielordner. Falls nötig, wähle den Ordner mit **`Terraria.exe` und `Content`**. Über Steam findest du ihn unter Terraria → Eigenschaften → Installierte Dateien → Durchsuchen.
5. Klicke auf **Installieren & spielen**. Später genügt **Spielen**.

Der Mod aktiviert **Frame Skip: Off**. Stelle in Windows die tatsächliche Bildwiederholrate deines Monitors ein. Mehr Frames benötigen CPU/GPU-Leistung; eine bestimmte FPS-Zahl wird nicht garantiert.

**Aktualisieren:** Schließe das Spiel, entpacke die neue Mod-Version in einen neuen Ordner und wähle **Installieren / aktualisieren**. Nach einem Terraria-Update brauchst du eine ausdrücklich passende Mod-Version. Unbekannte Versionen werden abgelehnt.

**Entfernen:** Schließe das Spiel und wähle **High FPS entfernen**. Der normale Start über Steam öffnet weiterhin das Original. Für die manuelle Entfernung lösche nur `Terraria.HighFPS.exe`, `HighFPS.Support.dll`, `HighFPS.Support.install.txt` und `HighFPS.Support.log` aus dem Spielordner. Spielstände und `Terraria.exe` bleiben erhalten. Launcher-Einstellungen liegen unter `%LOCALAPPDATA%\TerrariaHighFPS`; Terraria kann Frame Skip: Off in seinen eigenen Einstellungen behalten.

## Download prüfen

Vergleiche den ZIP-Hash mit den [Prüfsummen derselben Version](release-hashes.md). PowerShell im Download-Ordner:

```powershell
Get-FileHash -Algorithm SHA256 -LiteralPath .\HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip
```

Das Archiv enthält `SHA256SUMS.txt`. Lies nach dem Entpacken `verify-release.ps1` und führe im entpackten Ordner aus:

```powershell
powershell -NoProfile -File .\verify-release.ps1
```

Sind Skripte gesperrt, vergleiche einzelne Dateien mit `Get-FileHash` und `SHA256SUMS.txt`. Eine Änderung der Sicherheitsrichtlinie ist nicht nötig. Bei Abweichungen nicht starten.

**Ein passender Hash bestätigt die Übereinstimmung mit einer vertrauenswürdigen Prüfsumme, nicht die Unbedenklichkeit.** Die Anwendung ist nicht signiert; Windows kann einen unbekannten Herausgeber oder SmartScreen anzeigen. Ein unabhängiges Sicherheitsaudit und bitgenau reproduzierbare Builds werden nicht behauptet. Lass den Virenschutz aktiviert.

## Funktionsweise und Vertrauen

Der Launcher erstellt eine separate `Terraria.HighFPS.exe` und die Bibliothek `HighFPS.Support.dll`. **Das Original wird nicht überschrieben oder umbenannt.** Drei eingefügte Aufrufe erfassen den Zustand vor einem Spieltick, glätten Positionen während des Zeichnens und stellen danach die Simulationspositionen wieder her. Logik und Netzwerk werden nicht beschleunigt. Die Darstellung kann dabei um bis zu einen Tick verzögert sein; nicht alle Animationen werden interpoliert.

Der Launcher hat keine Telemetrie, Anmeldung, Laufzeit-Downloads oder automatische Updates und installiert keine Dienste oder Treiber. Pfad, Sprache, Installationsdaten und Diagnoseprotokolle bleiben lokal. Steam und Terraria nutzen das Netzwerk weiterhin normal. Beim Bauen wird gegebenenfalls das festgelegte Mono.Cecil-Paket mit Hash-Prüfung von NuGet geladen.

Bei Problemen: Spiel vollständig schließen; das gesamte ZIP erneut entpacken; die richtige Spielversion und Schreibrechte prüfen. Mit **Installieren / aktualisieren** lässt sich eine beschädigte Installation erneuern. Unter **Technische Details** findest du Diagnosen. Entferne persönliche Pfade vor dem Teilen. [Ausführliche Hilfe (EN)](guide.md) · [Sicherheit (EN/RU)](../SECURITY.md) · [Technik (EN)](architecture.md) · [Selbst bauen (EN)](building.md).

## Lizenz und Credits

Von **pavlikmeow**. Projekteigener Code und Dokumentation stehen unter [MIT](../LICENSE). Danke an [TerrariaHighFPS](https://github.com/Yukurotei/TerrariaHighFPS) für den Interpolationsansatz. Lizenzen und Hinweise zu Mono.Cecil stehen in den [Drittanbieterhinweisen (EN/RU)](../THIRD-PARTY-NOTICES.md).

Ein inoffizielles Fanprojekt ohne Verbindung zu Re-Logic, Valve oder Microsoft. Das Spiel und seine Ressourcen sind nicht enthalten; du brauchst eine eigene Kopie von Terraria.
