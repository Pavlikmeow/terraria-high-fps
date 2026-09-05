# Security / Безопасность

[English](#english) · [Русский](#русский) · [README](README.md)

## English

### Supported scope

This security description covers **High FPS Support 1.1.0 for Steam Terraria 1.4.5.8, Windows x86**. Older mod releases and different game builds are outside the current supported scope. This is a community project: no external security audit, response-time commitment or code-signing certificate is claimed.

### What runs and what is stored

| Location / action | Purpose |
| --- | --- |
| Extracted launcher folder | Launcher, runtime DLL, Mono.Cecil, source, build/test scripts, documentation and checksums |
| Game folder: `Terraria.HighFPS.exe` | Separate executable generated from your local game |
| Game folder: `HighFPS.Support.dll` | Render interpolation runtime |
| Game folder: `HighFPS.Support.install.txt` | Mod/game versions and SHA-256 hashes of the source, output and runtime |
| Game folder: `HighFPS.Support.log` | Startup and error diagnostics; may contain local paths and exception details |
| Temporary files/directories in game folder | Staging and rollback during install; recovery data may remain if recovery fails |
| `%LOCALAPPDATA%\TerrariaHighFPS\game-path.txt` | Selected game directory |
| `%LOCALAPPDATA%\TerrariaHighFPS\language.txt` | Launcher language |
| Steam registry keys and `libraryfolders.vdf` | Read-only installation discovery |
| Process list | Check that Terraria is closed before installing/removing |

The launcher does not change the original `Terraria.exe`, install services/drivers, add scheduled tasks, write Steam registry settings, request credentials, or upload logs. It runs with your current Windows permissions and needs write access to the game folder. It is not a sandbox: a launcher or game you choose to run can act with those permissions.

The launcher and interpolation runtime contain no network client, telemetry or automatic updater. **Building** may download the pinned Mono.Cecil package from `api.nuget.org`; it checks the package and DLL hashes before use. Steam and Terraria themselves use the network normally. Opening documentation links in your browser contacts the linked sites.

The mod uses normal Terraria saves. Its runtime forces Frame Skip off, which the game may preserve in its normal configuration. Uninstalling the mod does not reset Terraria's own settings.

### Integrity controls and their limits

- The patcher checks the game assembly name, exact version, architecture, required members and expected hook locations, then reads back the generated output and verifies its hooks.
- Installation stages and validates files before replacing the installed mod. A failed commit attempts rollback. This is not an all-or-nothing transaction across a power loss; retained recovery files and an error message require attention.
- Installation reuse and launcher startup check recorded hashes against the current original game, generated EXE and runtime DLL. The installed runtime must match the launcher's embedded copy. Launching `Terraria.HighFPS.exe` directly skips the launcher's checks.
- These checks detect unexpected changes and incompatibility. They do **not** authenticate Terraria as a genuine Steam download, certify arbitrary same-version game files, or resist an attacker who can replace the launcher and its metadata together. Use Steam's file verification when the original is suspect.
- Release checksums identify exact artifacts when compared with an independently trusted release. A ZIP and checksum from the same compromised source can agree. Unsigned hashes are not publisher signatures; source availability and an antivirus scan are not guarantees either.

See [download verification](README.md#check-your-download), [dependency hashes](THIRD-PARTY-NOTICES.md) and [build instructions](docs/building.md). Keep antivirus enabled. Report an unexpected detection with the file hash and detection name; do not assume it is a false positive.

### Reporting a vulnerability

Use this repository's **Security → Report a vulnerability** if that private channel is available. Include the affected release, steps to reproduce, impact and a minimal example without game binaries, personal files or secrets. Remove your username and local paths from shared logs.

If private reporting is not enabled, open a public issue asking **pavlikmeow** for a private contact, without exploit details. A private address or active reporting channel is not promised before the repository is published. Ordinary bugs belong in Issues. Please allow time for assessment before publishing exploitable details.

## Русский

### Область поддержки

Описание относится к **High FPS Support 1.1.0 для Steam Terraria 1.4.5.8, Windows x86**. Старые выпуски мода и другие версии игры не входят в текущую область поддержки. Независимый аудит, гарантированный срок ответа и цифровая подпись не заявлены.

### Файлы и разрешения

В распакованном архиве находятся лаунчер, его DLL, Mono.Cecil, исходники, скрипты сборки/проверки и документация. В папке игры создаются `Terraria.HighFPS.exe`, `HighFPS.Support.dll`, сведения об установке `HighFPS.Support.install.txt` и локальный журнал `HighFPS.Support.log`. Временные файлы служат для установки и отката; при неудачном восстановлении данные для восстановления могут остаться. В `%LOCALAPPDATA%\TerrariaHighFPS` хранятся `game-path.txt` и `language.txt`.

Для поиска игры лаунчер читает пути Steam из реестра и `libraryfolders.vdf`, а перед установкой/удалением проверяет процессы Terraria. Оригинальный `Terraria.exe` не изменяется. Службы, драйверы, задания планировщика и записи Steam в реестре не создаются. Пароли не запрашиваются, журналы никуда не отправляются. Нужны права записи в папку игры; программа работает с текущими правами пользователя и не является песочницей.

В лаунчере и модуле интерполяции нет сетевого клиента, телеметрии и автообновления. При **сборке** может скачиваться закреплённая версия Mono.Cecil с `api.nuget.org` с проверкой хэшей пакета и DLL. Сама Terraria и Steam продолжают работать с сетью. Переход по ссылке в документации открывает соответствующий сайт.

Используются обычные сохранения Terraria. Мод включает Frame Skip: Off; игра может сохранить этот параметр в собственных настройках. Удаление мода настройки игры не сбрасывает.

### Что подтверждают проверки

Патчер проверяет имя, точную версию, архитектуру, необходимые элементы и точки вставки в игре, затем перечитывает результат. До замены файлов установка готовит и проверяет новые файлы. При ошибке замены выполняется попытка отката; абсолютной защиты от обрыва питания нет.

Повторная установка и запуск через лаунчер сверяют хэши оригинала, созданного EXE и DLL с метаданными; DLL также должна совпадать со встроенной в лаунчер. Прямой запуск `Terraria.HighFPS.exe` пропускает эти проверки. Хэши и проверка структуры выявляют изменения, но не подтверждают подлинность игры или издателя и не защищают от одновременной подмены лаунчера и метаданных. При сомнениях в оригинале используйте проверку файлов Steam.

Совпадение хэша релиза означает совпадение с доверенной контрольной суммой. Злоумышленник может заменить и файл, и сумму в одном источнике. Хэш без подписи не является подписью издателя; открытый код и проверка антивирусом тоже не гарантируют безопасность. [Проверка загрузки](docs/README.ru.md#как-проверить-скачанные-файлы) · [Хэши зависимости](THIRD-PARTY-NOTICES.md) · [Самостоятельная сборка](docs/building.ru.md).

Не отключайте антивирус. При неожиданном срабатывании сообщите название угрозы и хэш файла: заранее считать его ложным нельзя.

### Сообщить об уязвимости

Если доступно, используйте **Security → Report a vulnerability** этого репозитория. Укажите выпуск, воспроизведение и возможные последствия. Не прикладывайте игру, личные файлы и секреты; уберите имя пользователя и пути из журналов.

Если закрытый канал ещё не включён, создайте Issue с просьбой к **pavlikmeow** предоставить приватный контакт, без описания эксплуатации. До публикации репозитория наличие такого канала не обещается. Обычные ошибки отправляйте в Issues. Дайте автору время на проверку перед публикацией опасных подробностей.
