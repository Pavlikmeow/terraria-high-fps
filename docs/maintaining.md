# Preparing a release

[English](#english) · [Русский](#русский)

## English

1. Update the mod version, compatibility constants and all README translations together. Validate a new Terraria version before changing compatibility.
2. Run the checks in [building.md](building.md). Record gameplay tests separately, including refresh rate and scenarios. Check all launcher languages and Windows scaling.
3. Build from the intended source revision. Inspect the ZIP for game files, saves, test fixtures or caches. Keep the source, documentation, licenses and checksum verifier in the package.
4. Copy `release/release-hashes.md` to `docs/release-hashes.md` only when publishing that build. Publish the ZIP and `release/SHA256SUMS.txt` together. Do not replace an existing release asset with different bytes under the same version.
5. Preserve licenses and attribution for dependencies and imported code. Public source without a license is not permission to reuse it. See [third-party notices](../THIRD-PARTY-NOTICES.md).
6. Check that the vulnerability reporting instructions in [SECURITY.md](../SECURITY.md) match the repository settings.

## Русский

1. Обновите версию мода, константы совместимости и все переводы README. До добавления поддержки новой Terraria проверьте её.
2. Выполните [проверки](building.ru.md). Ручные игровые тесты запишите отдельно, с частотой монитора и сценариями. Проверьте все языки лаунчера и масштаб Windows.
3. Соберите нужную ревизию. Проверьте ZIP: в нём не должно быть игры, сохранений, тестовых копий и кэшей. Сохраните исходники, документацию, лицензии и скрипт проверки хэшей.
4. Копируйте `release/release-hashes.md` в `docs/release-hashes.md` только при публикации этой сборки. Опубликуйте ZIP вместе с `release/SHA256SUMS.txt`. Не заменяйте файлы выпущенной версии другими без изменения номера.
5. Сохраняйте лицензии и авторство зависимостей и заимствованного кода. Публичный доступ без лицензии не даёт разрешения на копирование. См. [сторонние компоненты](../THIRD-PARTY-NOTICES.md).
6. Проверьте, что инструкции сообщения об уязвимостях в [SECURITY.md](../SECURITY.md#русский) соответствуют настройкам репозитория.
