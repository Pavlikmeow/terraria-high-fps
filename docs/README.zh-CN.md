# High FPS Support

**让 Terraria 的移动画面更流畅，发挥高刷新率显示器的优势。**  
由 **pavlikmeow** 制作的非官方开源启动器 · 版本 **1.1.0**

[English](../README.md) · [Русский](README.ru.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md) · [Português (Brasil)](README.pt-BR.md) · **简体中文**

Terraria 每秒更新游戏世界 60 次。此模组在更新之间绘制玩家、敌人、弹幕和掉落物的中间位置，让 120/144/165/240 Hz 显示器上的移动画面更流畅。游戏速度保持不变，无需 tModLoader。

## 使用条件

仅支持 **Windows Steam 原版 Terraria 1.4.5.8，x86 EXE**。需要安装 .NET Framework 4.x 和 XNA Framework 4；请先从 Steam 启动一次原版游戏，完成所需组件的安装。不支持其他游戏版本、平台、tModLoader 或其他 EXE 补丁。请使用自己合法取得的游戏副本。此模组使用 Terraria 的正常存档，尝试前请备份重要世界和角色。

## 开始游戏

1. 在此仓库的 [Releases](https://github.com/Pavlikmeow/terraria-high-fps/releases) 中下载 `HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip`，将**整个压缩包完整解压**到一个文件夹。不要在 ZIP 内运行，也不要只移动 EXE。
2. 按下方说明验证下载。关闭 Terraria，保持 Steam 运行。
3. 打开 **`HighFpsSupport.exe`**。在右上角 **Language / Язык** 中选择 **简体中文**。此设置会保存，仅影响启动器语言。
4. 确认检测到的游戏目录。如需手动选择，请指定同时包含 **`Terraria.exe` 和 `Content`** 的文件夹。在 Steam 中可通过 Terraria → 属性 → 已安装文件 → 浏览找到它。
5. 点击 **“安装并开始游戏”**。之后使用 **“开始游戏”** 即可。

模组会启用 **Frame Skip: Off**。请在 Windows 显示设置中选择显示器实际支持的刷新率。绘制更多帧需要 CPU/GPU 余量，此模组不保证达到某个 FPS。

**更新：** 关闭游戏，将新版模组解压到新文件夹，点击 **“仅安装 / 更新”**。Terraria 本身更新后，需要明确支持该游戏版本的模组。启动器会拒绝不兼容版本。

**移除：** 关闭游戏，点击 **“移除 High FPS”**。从 Steam 正常启动仍会打开原版游戏。如需手动卸载，只删除游戏目录中的 `Terraria.HighFPS.exe`、`HighFPS.Support.dll`、`HighFPS.Support.install.txt` 和 `HighFPS.Support.log`。`Terraria.exe` 和存档会保留。启动器偏好保存在 `%LOCALAPPDATA%\TerrariaHighFPS`；Terraria 可能会在自己的设置中保留 Frame Skip: Off。

## 验证下载

将 ZIP 的哈希与[同一版本的校验值](release-hashes.md)比较。在下载文件夹中打开 PowerShell：

```powershell
Get-FileHash -Algorithm SHA256 -LiteralPath .\HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip
```

压缩包包含 `SHA256SUMS.txt`。解压后先阅读 `verify-release.ps1`，再在解压目录中执行：

```powershell
powershell -NoProfile -File .\verify-release.ps1
```

如果系统禁止运行脚本，可以用 `Get-FileHash` 逐个计算文件并与 `SHA256SUMS.txt` 比较，无需更改安全策略。任何哈希不一致时，请不要运行程序。

**哈希一致只能证明文件与可信校验值相符，不能证明软件无害。** 程序没有数字签名，Windows 可能显示未知发布者或 SmartScreen 提示。本项目不声称经过独立安全审计，也不保证逐位可复现构建。请保持杀毒软件开启。

## 工作原理与本地更改

启动器会单独创建 `Terraria.HighFPS.exe` 和 `HighFPS.Support.dll`，**不会覆盖或重命名原版 EXE**。补丁添加三处调用：在逻辑 tick 前记录状态，绘制时对位置插值，绘制后恢复模拟坐标。游戏逻辑和网络不会加速。插值最多可能引入约一个 tick 的视觉延迟，并非所有动画都会被插值。

启动器没有遥测、登录、运行时下载或自动更新，也不安装服务或驱动。游戏路径、语言、安装信息和诊断日志都保存在本地。Steam 和 Terraria 自身仍正常使用网络。自行构建时，可能从 NuGet 下载固定版本的 Mono.Cecil，并核对哈希。

遇到问题时，先完全关闭游戏，重新完整解压 ZIP，检查游戏版本及目录写入权限。**“仅安装 / 更新”** 可修复安装；**“技术详情”** 中提供诊断。分享前请删除个人路径。[详细帮助（英语）](guide.md) · [安全说明（英语/俄语）](../SECURITY.md) · [技术原理（英语）](architecture.md) · [自行构建（英语）](building.md)。

## 许可与致谢

作者：**pavlikmeow**。项目代码和文档采用 [MIT 许可证](../LICENSE)。感谢 [TerrariaHighFPS](https://github.com/Yukurotei/TerrariaHighFPS) 提供的插值思路。Mono.Cecil 的许可证及相关声明见[第三方声明（英语/俄语）](../THIRD-PARTY-NOTICES.md)。

这是非官方爱好者项目，与 Re-Logic、Valve 或 Microsoft 无隶属关系。项目不包含游戏及其资源；请使用自己的 Terraria 副本。
