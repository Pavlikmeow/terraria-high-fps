# Third-party notices / Сторонние компоненты

The [project MIT license](LICENSE) applies to project-owned code and documentation only. It does not relicense third-party software, Terraria, game assets, Steam, XNA, or a locally generated patched game executable.

Лицензия MIT проекта относится только к собственному коду и документации. Она не меняет лицензии сторонних компонентов, Terraria, игровых ресурсов, Steam, XNA и локально созданного изменённого EXE игры.

## Mono.Cecil 0.11.6

Used by the launcher to inspect managed assemblies and generate the separate patched executable. The package's `lib/net40/Mono.Cecil.dll` is distributed with this launcher. / Используется для анализа сборок и создания отдельного изменённого EXE. Вместе с лаунчером распространяется `lib/net40/Mono.Cecil.dll` из пакета.

- Author/project: [Jb Evain and contributors](https://github.com/jbevain/cecil)
- Package: [Mono.Cecil 0.11.6 on NuGet](https://www.nuget.org/packages/Mono.Cecil/0.11.6)
- License source for this version: [0.11.6/LICENSE.txt](https://github.com/jbevain/cecil/blob/0.11.6/LICENSE.txt)
- NuGet package SHA-256: `D2A23832AAA948BA9A01ACC42B5726E34C5F995958F1B30D45C0E7C70B3A72D5`
- Included DLL SHA-256: `C41BDB9FFD3C5F6E17D2382C1012D73703E035E3F1100245FDD4E08C8DC6EB5B`

The complete upstream license follows verbatim:

```text
Copyright (c) 2008 - 2015 Jb Evain
Copyright (c) 2008 - 2011 Novell, Inc.

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:
The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```

## Inspiration / Идея

Thanks to [Yukurotei/TerrariaHighFPS](https://github.com/Yukurotei/TerrariaHighFPS) for its publicly described high-FPS interpolation approach, credited in this project's earlier README. Its published source tree did not show a license file when reviewed on 2026-09-05. This project's MIT license does not license that source; public availability and attribution do not grant redistribution permission.

This distribution does not include that project's downloaded executables, `HighFPSLogic.dll`, `GpuBatchLogic.dll`, or GPU batching implementation. Contributors must check rights before importing any third-party code; see [maintainer guidance](docs/maintaining.md).

Благодарность [Yukurotei/TerrariaHighFPS](https://github.com/Yukurotei/TerrariaHighFPS) за публично описанный подход к интерполяции, упомянутый в прежнем README проекта. При проверке 05.09.2026 в опубликованном дереве исходников файл лицензии не был обнаружен. MIT этого проекта не лицензирует те исходники: публичный доступ и благодарность не заменяют разрешение на распространение.

В комплект не входят скачанные EXE того проекта, `HighFPSLogic.dll`, `GpuBatchLogic.dll` и реализация GPU batching. Перед заимствованием стороннего кода необходимо проверить права; см. [рекомендации для релизов](docs/maintaining.md).

## Terraria, Steam, Windows and XNA / Игра и платформы

- **Terraria** is developed and published by **Re-Logic**. [Official Steam listing](https://store.steampowered.com/app/105600/Terraria/).
- **Steam** belongs to **Valve**. Users remain subject to the [Steam Subscriber Agreement](https://store.steampowered.com/subscriber_agreement/) and applicable game terms.
- **Windows, .NET and Microsoft XNA Framework** are Microsoft products. They are external prerequisites; their binaries are not included in the release archive.

High FPS Support is an unofficial fan project, not affiliated with, sponsored by or endorsed by these companies. Product names identify compatibility. No game binaries, art, music or proprietary runtime files belong in this repository or its releases. The patched game executable is generated locally from the user's installed copy and must not be uploaded as a project release.

The project license grants no additional rights to third-party products. Their terms and applicable law still apply; local patching alone does not establish legal permission. No universal legal approval or guarantee of compliance with every platform term is claimed.

Это неофициальный проект без связи с указанными компаниями, их спонсорства или одобрения. Названия продуктов обозначают совместимость. Не добавляйте в репозиторий и релизы игру, графику, музыку и закрытые библиотеки. Изменённый EXE создаётся локально из установленной копии пользователя и не должен публиковаться в релизах проекта.

Лицензия проекта не предоставляет дополнительных прав на чужие продукты. Их условия и применимое законодательство сохраняют силу; локальное создание патча само по себе не является разрешением. Универсальная законность и соответствие всем условиям платформ не гарантируются.
