# Contributing

[English](#english) · [Русский](#русский)

## English

Bug fixes, translations and clear bug reports are welcome. For changes to rendering or supported game versions, open an issue first so we can discuss the approach.

Start with the [build instructions](docs/building.md) and [architecture](docs/architecture.md). Keep each pull request focused and describe the problem, the change and the checks you ran.

- Keep the original game executable intact, validate compatibility and restore simulation positions after drawing.
- Use English comments for non-obvious decisions. User-facing text belongs in `Localization.cs`; keep keys and placeholders consistent across all seven languages.
- Run the relevant checks from the build guide. Rendering changes also need in-game testing; include the refresh rate and scenarios you checked.
- Keep generated binaries, game files and personal data out of commits. Preserve dependency licenses and attribution. Contributions to project code use the [MIT license](LICENSE).

For a bug report, include the mod, Terraria and Windows versions, monitor refresh rate, steps to reproduce, and what you expected to happen. Remove personal paths from diagnostics. For vulnerabilities, use [SECURITY.md](SECURITY.md).

## Русский

Приветствуются исправления, переводы и понятные сообщения об ошибках. Изменения отрисовки или поддерживаемых версий игры сначала обсудите в Issue.

Начните с [инструкции сборки](docs/building.ru.md) и [описания устройства](docs/architecture.ru.md). Посвящайте каждый PR одной задаче: опишите проблему, исправление и выполненные проверки.

- Сохраняйте оригинальный EXE, проверку совместимости и восстановление игровых координат после отрисовки.
- Пишите комментарии к неочевидным решениям на английском. Тексты интерфейса храните в `Localization.cs`, сохраняя одинаковые ключи и параметры во всех семи языках.
- Выполняйте подходящие проверки из инструкции сборки. Изменения отрисовки проверяйте и в игре: укажите частоту монитора и сценарии.
- Не добавляйте сборки, файлы игры и личные данные в коммиты. Сохраняйте лицензии зависимостей и авторство. Вклад в код проекта распространяется по [MIT](LICENSE).

В сообщении об ошибке укажите версии мода, Terraria и Windows, частоту монитора, шаги воспроизведения и ожидаемый результат. Удалите личные пути из диагностики. Об уязвимостях сообщайте по [SECURITY.md](SECURITY.md#русский).
