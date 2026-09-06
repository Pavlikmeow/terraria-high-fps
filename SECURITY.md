# Security

[English](#english) · [Русский](#русский)

## English

This document covers High FPS Support 1.1.0 for Steam Terraria 1.4.5.8 on Windows x86.

### Files and permissions

The launcher runs with your Windows account's permissions and needs write access to the game folder. It leaves the original `Terraria.exe` unchanged.

| Location | Contents |
| --- | --- |
| Game folder: `Terraria.HighFPS.exe` | Separate patched executable |
| Game folder: `HighFPS.Support.dll` | Interpolation runtime |
| Game folder: `HighFPS.Support.install.txt` | Versions and hashes used to verify the installation |
| Game folder: `HighFPS.Support.log` | Runtime startup and error diagnostics |
| Game folder: `.HighFPS-staging-*` | Temporary installation and rollback files; retained if recovery fails |
| `%LOCALAPPDATA%\TerrariaHighFPS\game-path.txt` | Selected game folder |
| `%LOCALAPPDATA%\TerrariaHighFPS\language.txt` | Launcher language |

Game discovery reads Steam's registry paths and `libraryfolders.vdf`. Before installing or removing files, the launcher checks for running Terraria processes.

The launcher and interpolation runtime have no telemetry, network downloads or automatic updater. They do not install services, drivers or scheduled tasks. Building may download the pinned Mono.Cecil package from NuGet and verifies its package and DLL hashes. Steam and Terraria keep their own network behavior.

The mod uses normal Terraria saves. It sets Frame Skip to Off, which Terraria may retain in its own settings after removal.

### Verification and limits

The patcher checks the game version, architecture and expected code structure, then verifies the inserted calls. Installation stages new files before replacing the mod and attempts rollback on failure. If rollback fails, the error gives the location of retained recovery files.

The launcher verifies installed hashes against the original game, metadata and embedded runtime before launch. Running `Terraria.HighFPS.exe` directly skips those checks. These checks detect changes; they do not authenticate the original game or protect against an attacker replacing the launcher itself.

Release builds are unsigned and have not had an independent security audit. Compare downloads with the [published checksums](docs/release-hashes.md); matching hashes are only as trustworthy as their source. Keep antivirus enabled, and include the detection name and file hash when reporting an unexpected alert.

See [build instructions](docs/building.md) and [dependency notices](THIRD-PARTY-NOTICES.md) to inspect the build inputs.

### Reporting a vulnerability

Use **Security → Report a vulnerability** on GitHub if private reporting is available. Include the affected version, reproduction steps and impact. Do not attach game binaries or personal files; remove usernames and local paths from logs.

If private reporting is unavailable, open an issue asking **pavlikmeow** for a private contact without publishing exploit details. Ordinary bugs belong in Issues.

## Русский

Описание относится к High FPS Support 1.1.0 для Steam Terraria 1.4.5.8, Windows x86.

### Файлы и разрешения

Лаунчер работает с правами вашей учётной записи Windows. Ему нужна запись в папку игры. Оригинальный `Terraria.exe` остаётся прежним.

В папке игры появляются `Terraria.HighFPS.exe`, `HighFPS.Support.dll`, метаданные `HighFPS.Support.install.txt` и журнал `HighFPS.Support.log`. Папки `.HighFPS-staging-*` служат для установки и отката; при ошибке восстановления они сохраняются. Выбранная папка игры и язык хранятся в `game-path.txt` и `language.txt` внутри `%LOCALAPPDATA%\TerrariaHighFPS`.

Для поиска игры лаунчер читает пути Steam из реестра и `libraryfolders.vdf`. Перед установкой и удалением проверяет процессы Terraria.

В лаунчере и модуле нет телеметрии, сетевых загрузок и автообновления. Службы, драйверы и задания планировщика не создаются. При сборке может скачиваться закреплённый Mono.Cecil с NuGet с проверкой хэшей пакета и DLL. Steam и Terraria используют сеть как обычно.

Используются обычные сохранения Terraria. Мод включает Frame Skip: Off; после удаления игра может сохранить этот параметр.

### Проверки и ограничения

Патчер проверяет версию, архитектуру, структуру кода и результат вставки вызовов. Установка сначала готовит новые файлы, затем заменяет мод. При ошибке выполняется откат; если восстановление не удалось, сообщение указывает папку с оставшимися файлами.

Перед запуском лаунчер сверяет хэши установленного мода с оригинальной игрой, метаданными и встроенным модулем. Прямой запуск `Terraria.HighFPS.exe` пропускает эти проверки. Они выявляют изменения, но не подтверждают подлинность игры и не защищают от подмены самого лаунчера.

У сборок нет цифровой подписи; независимый аудит не проводился. Сравнивайте файлы с [опубликованными хэшами](docs/release-hashes.md), учитывая надёжность их источника. Не отключайте антивирус. При неожиданном срабатывании укажите название угрозы и хэш файла.

Зависимости описаны в [уведомлениях](THIRD-PARTY-NOTICES.md), процесс сборки — в [инструкции](docs/building.ru.md).

### Сообщить об уязвимости

Если доступно, используйте **Security → Report a vulnerability** на GitHub. Укажите версию, шаги воспроизведения и последствия. Не прикладывайте игру или личные файлы; удалите имена пользователей и пути из журналов.

Если закрытого канала нет, создайте Issue с просьбой к **pavlikmeow** предоставить приватный контакт, без подробностей эксплуатации. Обычные ошибки отправляйте в Issues.
