# Maintaining a release / Подготовка выпуска

## English

1. Update the mod version, compatibility constants and all README variants together. Do not broaden Terraria compatibility without validating the new original game and the injected IL.
2. Review source provenance and the license of each new dependency or imported code fragment. Preserve required notices. Conceptual inspiration does not license someone else's implementation; do not infer permission from a public repository without a license.
3. Run the game-free and game-dependent checks described in [building](building.md). Record manual gameplay scenarios separately from harness results. Check every launcher language and relevant Windows scaling settings.
4. Build from the intended source revision with the documented pinned dependency. Inspect the ZIP file list: no `Terraria.exe`, `Terraria.HighFPS.exe`, game content, XNA binaries, saves, local paths, caches or test fixtures. Keep LICENSE, notices, localized docs and checksum verifier in the package.
5. Generate checksums only after final edits and screenshots. Publish the versioned ZIP and its `SHA256SUMS.txt` together, and retain the matching `docs/release-hashes.md` in the source revision. Do not silently replace a release asset with different bytes under the same version.
6. Configure GitHub's private vulnerability reporting if available and verify the Security instructions match the actual channel. Enable useful repository protections; never commit tokens or signing keys.
7. Confirm rights for the material being published. The MIT license covers project-owned work only; no general approval from Re-Logic, Valve or Microsoft is implied. Any rights question involving imported material should be resolved with its owner before distribution.

Do not advertise “100% safe”, “virus-free”, a performance multiplier, an independent audit, publisher signature or reproducible binaries without evidence for that exact claim. Use the source, dependency provenance, explicit file inventory and honest test results to make the release reviewable.

## Русский

Обновляйте версию мода и совместимость во всех языках одновременно. Расширять поддержку Terraria можно после проверки оригинальной новой версии и IL. Для любого заимствованного кода проверяйте лицензию и сохраняйте уведомления: публичный репозиторий без лицензии не даёт разрешения на копирование.

Выполните [проверки сборки](building.ru.md), отдельно запишите реальные игровые сценарии и проверьте языки/масштаб Windows. В ZIP не должно быть EXE игры, ресурсов, XNA, сохранений, локальных путей и тестовых копий. Сохраните LICENSE, уведомления, переводы и проверку хэшей.

Считайте контрольные суммы после последних правок и снимков интерфейса. Публикуйте ZIP с номером версии и `SHA256SUMS.txt` вместе; исходникам должна соответствовать таблица `docs/release-hashes.md`. Не заменяйте незаметно файлы уже выпущенной версии.

Настройте доступный закрытый канал уязвимостей GitHub и проверьте инструкции Security. Не коммитьте токены и ключи. Подтвердите права на публикуемые материалы: MIT относится только к собственной работе и не означает одобрения Re-Logic, Valve или Microsoft.

Не обещайте абсолютную безопасность, отсутствие вирусов, множитель FPS, независимый аудит, подпись издателя и воспроизводимость без соответствующих доказательств. Основа доверия — проверяемый код, происхождение зависимостей, список изменений на диске и честные результаты проверок.
