param([string]$ScreenshotDirectory)

. (Join-Path $PSScriptRoot 'common.ps1')
$root = Split-Path -Parent $PSScriptRoot
$csc = Get-CompilerPath
$cecil = Get-CecilPath $root
$run = Join-Path $root ('.build\ui-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $run | Out-Null
if (-not $ScreenshotDirectory) { $ScreenshotDirectory = Join-Path $run 'screenshots' }
$sources = @(Get-ChildItem -LiteralPath (Join-Path $root 'src\HighFPS.Launcher') -Filter '*.cs' | ForEach-Object FullName)
# UI tests need neither Terraria nor its proprietary resources. Game actions are never invoked.
& $csc /nologo /codepage:65001 /target:exe /platform:x86 /warnaserror+ /main:UiHarness "/out:$(Join-Path $run 'UiHarness.exe')" "/reference:$cecil" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll $sources (Join-Path $root 'tools\UiHarness.cs')
if ($LASTEXITCODE -ne 0) { throw 'UI harness compilation failed.' }
Copy-Item -LiteralPath $cecil -Destination (Join-Path $run 'Mono.Cecil.dll')
& (Join-Path $run 'UiHarness.exe') $ScreenshotDirectory
if ($LASTEXITCODE -ne 0) { throw 'UI or localization checks failed.' }
Write-Host "UI checks passed. Screenshots: $ScreenshotDirectory"
