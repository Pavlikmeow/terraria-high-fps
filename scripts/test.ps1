param([string]$TerrariaExe, [switch]$SkipBuild)

. (Join-Path $PSScriptRoot 'common.ps1')
$root = Split-Path -Parent $PSScriptRoot
$game = Get-GamePath $TerrariaExe
if (-not $SkipBuild) { & (Join-Path $PSScriptRoot 'build.ps1') -TerrariaExe $game }
$csc = Get-CompilerPath
$cecil = Get-CecilPath $root
$xna = @(Get-XnaReferences)
$run = Join-Path $root ('.build\tests-' + [Guid]::NewGuid().ToString('N'))
$mock = Join-Path $run 'game'
New-Item -ItemType Directory -Force -Path $run, $mock, (Join-Path $mock 'Content') | Out-Null
# All mutation tests use a private copy. Never install into the actual Steam directory.
Copy-Item -LiteralPath $game -Destination (Join-Path $mock 'Terraria.exe')
Set-Content -LiteralPath (Join-Path $mock '.highfps-test-fixture') -Value 'Isolated test copy / Изолированная тестовая копия'
Copy-Item -LiteralPath $cecil -Destination (Join-Path $run 'Mono.Cecil.dll')
Copy-Item -LiteralPath (Join-Path $root 'dist\HighFPS.Support.dll') -Destination (Join-Path $mock 'HighFPS.Support.dll')
$launcherSources = @(Get-ChildItem -LiteralPath (Join-Path $root 'src\HighFPS.Launcher') -Filter '*.cs' | ForEach-Object FullName)
$logic = Join-Path $root 'dist\HighFPS.Support.dll'

function Invoke-Harness([string]$Name, [string[]]$Arguments) {
    & (Join-Path $run ($Name + '.exe')) @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Name failed with exit code $LASTEXITCODE." }
}
foreach ($name in @('PatchHarness', 'InstallHarness', 'UiHarness')) {
    & $csc /nologo /codepage:65001 /target:exe /platform:x86 /warnaserror+ "/out:$(Join-Path $run ($name + '.exe'))" "/main:$name" "/reference:$cecil" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "/resource:$logic,HighFPS.Support.dll" $launcherSources (Join-Path $root ('tools\' + $name + '.cs'))
    if ($LASTEXITCODE -ne 0) { throw "$name compilation failed." }
}
Invoke-Harness 'InstallHarness' @($mock)
Copy-Item -LiteralPath $logic -Destination (Join-Path $mock 'HighFPS.Support.dll')
Invoke-Harness 'PatchHarness' @((Join-Path $mock 'Terraria.exe'), (Join-Path $mock 'Terraria.HighFPS.exe'), (Join-Path $mock 'HighFPS.Support.dll'))

# Extract dependencies from the user's copy for isolated JIT/runtime tests only.
& $csc /nologo /codepage:65001 /target:exe /platform:x86 "/out:$(Join-Path $run 'ResourceExtractor.exe')" "/reference:$cecil" (Join-Path $root 'tools\ResourceExtractor.cs')
if ($LASTEXITCODE -ne 0) { throw 'Resource extractor compilation failed.' }
$resources = @{
    'ReLogic.ReLogic' = 'ReLogic'; 'DotNetZip.Ionic.Zip.CF' = 'Ionic.Zip.CF'; 'JSON.NET.Newtonsoft.Json' = 'Newtonsoft.Json';
    'CsvHelper.CsvHelper' = 'CsvHelper'; 'NVorbis.NVorbis' = 'NVorbis'; 'NVorbis.System.ValueTuple' = 'System.ValueTuple';
    'MP3Sharp.MP3Sharp' = 'MP3Sharp'; 'Steamworks.NET.Windows.Steamworks.NET' = 'Steamworks.NET';
    'RailSDK.Windows.RailSDK.Net' = 'RailSDK.Net'; 'SteelSeries.SteelSeriesEngineWrapper' = 'SteelSeriesEngineWrapper'
}
foreach ($key in $resources.Keys) { Invoke-Harness 'ResourceExtractor' @($game, ('Terraria.Libraries.' + $key + '.dll'), (Join-Path $mock ($resources[$key] + '.dll'))) }
foreach ($name in @('JitHarness', 'InterpolationHarness')) {
    & $csc /nologo /codepage:65001 /target:exe /platform:x86 /warnaserror+ "/out:$(Join-Path $mock ($name + '.exe'))" "/reference:$game" "/reference:$logic" $xna (Join-Path $root ('tools\' + $name + '.cs'))
    if ($LASTEXITCODE -ne 0) { throw "$name compilation failed." }
}
& (Join-Path $mock 'JitHarness.exe') (Join-Path $mock 'Terraria.HighFPS.exe')
if ($LASTEXITCODE -ne 0) { throw 'Patched method JIT verification failed.' }
& (Join-Path $mock 'InterpolationHarness.exe')
if ($LASTEXITCODE -ne 0) { throw 'Interpolation regression checks failed.' }
Invoke-Harness 'UiHarness' @((Join-Path $run 'screenshots'))
# Exercise the shipped Windows executable too, including its redirected CLI diagnostics.
foreach ($case in @(@('--version', 0), @('--help', 0), @('--unknown', 2), @('--diagnose', 1), @('--install', 0), @('--diagnose', 0), @('--remove', 0))) {
    $arguments = @($case[0])
    if ($case[0] -in @('--diagnose', '--install', '--remove')) { $arguments += ('"' + $mock + '"') }
    $process = Start-Process -FilePath (Join-Path $root 'dist\HighFpsSupport.exe') -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru -RedirectStandardOutput (Join-Path $run 'cli-output.txt') -RedirectStandardError (Join-Path $run 'cli-error.txt')
    try {
        if ($process.ExitCode -ne $case[1]) { throw "CLI $($case[0]) returned $($process.ExitCode), expected $($case[1])." }
    } finally { $process.Dispose() }
}
Write-Host 'Packaged CLI commands and exit codes verified.'
& (Join-Path $PSScriptRoot 'test-release.ps1')
& (Join-Path $PSScriptRoot 'verify-release.ps1')
Write-Host "All checks passed. Private test files: $run"
