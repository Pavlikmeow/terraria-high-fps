# Contributing / Участие в разработке

[English](#english) · [Русский](#русский)

## English

Small fixes, clearer translations and reproducible bug reports are welcome. Discuss a change to supported game versions, rendering behavior, dependencies or the security model in an issue before undertaking a large implementation.

1. Read the [architecture](docs/architecture.md) and [build instructions](docs/building.md).
2. Keep each pull request focused. Explain the user-visible problem, resulting behavior and validation you actually performed.
3. Preserve the original-game invariant, exact compatibility checks, install verification and restoration of simulation positions after drawing. Do not add telemetry, downloads or elevated privileges as an incidental change.
4. Comment the important invariants and non-obvious decisions in **English and Russian**. Comments should explain why, not paraphrase each line.
5. Keep launcher strings in its localization catalog. Maintain the same keys and placeholders in all seven languages. Use native language names, test longer labels, keyboard navigation and Windows display scaling. The language switch changes the launcher, not Terraria's language.
6. Run the relevant checks and a build. Rendering changes also need in-game validation; report the display refresh rate and tested scenarios. Do not describe a headless harness as a gameplay test.

Use your own code or a dependency with a compatible, documented license. Include attribution and complete required notices. Public code without a license is not automatically reusable. Never attach `Terraria.exe`, patched game binaries, XNA DLLs, worlds, characters, credentials or private logs to a PR. Your contribution is offered under the [project MIT license](LICENSE); third-party rights remain with their owners.

Bug reports should include the mod and game versions, Windows version, monitor refresh rate, install stage, expected/actual result, and a short reproduction. Redact usernames and local paths from diagnostics. For vulnerabilities, follow [SECURITY.md](SECURITY.md).

## Русский

Приветствуются небольшие исправления, улучшения переводов и воспроизводимые сообщения об ошибках. Крупные изменения совместимости, отрисовки, зависимостей и безопасности сначала обсудите в Issue.

1. Прочитайте [описание устройства](docs/architecture.ru.md) и [сборки](docs/building.ru.md).
2. Делайте PR с одной понятной задачей. Опишите проблему, поведение после исправления и реально выполненные проверки.
3. Сохраняйте оригинальный EXE, строгую совместимость, проверку установки и возврат игровых координат после кадра. Не добавляйте попутно телеметрию, загрузки и повышение прав.
4. Основные инварианты и неочевидные решения комментируйте на **английском и русском**. Объясняйте причину, а не повторяйте строку кода.
5. Тексты лаунчера храните в каталоге локализации. Сохраняйте одинаковые ключи и параметры во всех семи языках. Проверяйте длинные подписи, клавиатуру и масштаб Windows. Переключатель меняет язык лаунчера, а не Terraria.
6. Выполните подходящие проверки и сборку. Изменения отрисовки проверяйте в игре, указывая частоту монитора и сценарии. Автоматический тест без игры не заменяет игровую проверку.

Используйте собственный код или компоненты с подходящей проверенной лицензией. Сохраняйте авторство и полный текст необходимых уведомлений. Отсутствие лицензии у публичного кода не делает его свободным для копирования. Не прикладывайте игру, изменённые EXE игры, XNA DLL, миры, персонажей, пароли и личные журналы. Собственный вклад предоставляется по [MIT](LICENSE); права третьих лиц сохраняются.

В Issue укажите версии мода, игры и Windows, частоту монитора, этап установки, ожидаемый и фактический результат, шаги воспроизведения. Удалите имя пользователя и локальные пути из диагностики. Об уязвимостях сообщайте по [правилам безопасности](SECURITY.md#русский).
