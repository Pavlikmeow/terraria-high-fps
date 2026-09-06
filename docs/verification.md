# Verification for 1.1.0 / Проверка версии 1.1.0

Checked locally on Windows using the .NET Framework 4.x compiler, XNA 4 and a private copy of Steam Terraria 1.4.5.8. No game executable or test fixture is included in the release.

| Area | Result |
| --- | --- |
| Compilation | Runtime, launcher and harnesses compile with warnings treated as errors |
| Patching | Three hooks verified at their intended positions; source/output aliases and moved hooks rejected |
| Installation | First install, reuse, corrupt EXE/DLL repair and strict metadata parsing pass |
| Recovery | Locked output causes rollback; previous files restored; original EXE hash unchanged |
| Removal | Mod files removed from the test copy; original EXE and Content retained |
| JIT | Update, Draw and all three runtime hooks prepare successfully |
| Runtime state | Player, NPC, projectile and item interpolation/restoration pass; camera and MouseWorld preserved |
| Edge cases | Item rotation, teleports, repeated/interrupted drawing, replaced object slots and menu transitions pass |
| Launcher | Seven complete languages, culture fallback, live switching, action states and error text pass at two widths |
| Scaling | Simulated 150% and 200% geometry/font checks pass; screenshot review completed |
| CLI | Help, version, invalid arguments, incomplete/valid diagnostics, install and remove return expected exit codes |
| Dependencies | Pinned package/DLL hashes verified; fresh package extraction checked |
| Checksums | Valid files accepted; changed, missing, duplicate, malformed, empty and escaping entries rejected |

Run `scripts/test.ps1` to repeat the suite. `scripts/test-launcher.ps1` and `scripts/test-release.ps1` can run without the game. See [build instructions](building.md). Current CI results are available in [Actions](https://github.com/Pavlikmeow/terraria-high-fps/actions).

These checks do not measure real gameplay FPS, visual quality on every monitor, or multiplayer behavior. No new interactive gameplay session, independent security audit, antivirus certification, or Authenticode signing was performed for this release. Scaling tests simulate layout; they do not replace testing every Windows DPI/display configuration.

## Русский

Проверки выполнены локально на Windows с компилятором .NET Framework 4.x, XNA 4 и отдельной тестовой копией Steam Terraria 1.4.5.8. Сборка, установка, повторное использование, восстановление повреждений, откат и удаление прошли. Хеш оригинального EXE сохранился. Проверены три точки внедрения, JIT, интерполяция, восстановление координат, камера, прицеливание и пограничные случаи.

Семь языков проверены при двух ширинах окна, включая сообщения об ошибках. Выполнены моделирование масштаба 150%/200% и визуальная проверка снимков. Проверены CLI, распаковка зависимости и отклонение неправильных контрольных сумм. Для повторения используйте [инструкцию сборки](building.ru.md).

Новый ручной игровой сеанс в этом выпуске не проводился. Эти результаты не являются измерением FPS, проверкой всех мониторов или мультиплеера, независимым аудитом, антивирусной сертификацией или цифровой подписью. Текущие результаты CI доступны в [Actions](https://github.com/Pavlikmeow/terraria-high-fps/actions).
