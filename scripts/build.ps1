param([string]$TerrariaExe)

. (Join-Path $PSScriptRoot 'common.ps1')
$root = Split-Path -Parent $PSScriptRoot
$gameExe = Get-GamePath $TerrariaExe
$gameVersion = (Get-Item -LiteralPath $gameExe).VersionInfo.ProductVersion
if ($gameVersion -ne '1.4.5.8') { throw "Supported Terraria: 1.4.5.8; found $gameVersion." }
$cecilDll = Get-CecilPath $root
$csc = Get-CompilerPath
$xna = @(Get-XnaReferences)
$stage = Join-Path $root ('.build\package-' + [Guid]::NewGuid().ToString('N'))
$dist = Join-Path $root 'dist'
$release = Join-Path $root 'release'
New-Item -ItemType Directory -Force -Path $stage, $dist, $release | Out-Null

# EN: Build in a fresh directory; package only explicitly selected project files.
# RU: Собираем в новой папке; в архив попадают только явно выбранные файлы проекта.
$logicOutput = Join-Path $stage 'HighFPS.Support.dll'
& $csc /nologo /codepage:65001 /target:library /optimize+ /platform:x86 /warnaserror+ "/out:$logicOutput" "/reference:$gameExe" $xna (Join-Path $root 'src\HighFPS.Support\FpsManager.cs')
if ($LASTEXITCODE -ne 0) { throw 'Runtime compilation failed.' }
$sources = @(Get-ChildItem -LiteralPath (Join-Path $root 'src\HighFPS.Launcher') -Filter '*.cs' | Sort-Object Name | ForEach-Object FullName)
$launcherOutput = Join-Path $stage 'HighFpsSupport.exe'
& $csc /nologo /codepage:65001 /target:winexe /optimize+ /platform:x86 /warnaserror+ "/out:$launcherOutput" "/reference:$cecilDll" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "/resource:$logicOutput,HighFPS.Support.dll" $sources
if ($LASTEXITCODE -ne 0) { throw 'Launcher compilation failed.' }
Copy-Item -LiteralPath $cecilDll -Destination (Join-Path $stage 'Mono.Cecil.dll')
foreach ($name in @('README.md', 'LICENSE', 'THIRD-PARTY-NOTICES.md', 'SECURITY.md', 'CONTRIBUTING.md', '.gitignore', '.gitattributes', '.editorconfig')) {
    Copy-Item -LiteralPath (Join-Path $root $name) -Destination (Join-Path $stage $name)
}
Copy-Item -LiteralPath (Join-Path $root 'docs') -Destination (Join-Path $stage 'docs') -Recurse
Copy-Item -LiteralPath (Join-Path $root '.github') -Destination (Join-Path $stage '.github') -Recurse
# EN: Ship the exact sources beside binaries so the release can be inspected and rebuilt.
# RU: Исходники включены рядом с бинарниками, чтобы релиз можно было изучить и пересобрать.
foreach ($folder in @('src', 'scripts', 'tools')) {
    foreach ($file in Get-ChildItem -LiteralPath (Join-Path $root $folder) -File -Recurse) {
        if ($file.Extension -notin @('.cs', '.ps1')) { continue }
        $target = Join-Path $stage $file.FullName.Substring($root.Length + 1)
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $target) | Out-Null
        Copy-Item -LiteralPath $file.FullName -Destination $target
    }
}
# EN: The packaged page contains binary hashes; the repository page also contains the ZIP hash.
# RU: Страница в архиве содержит хеши бинарников, а страница в репозитории ещё и хеш ZIP.
$packagedHashes = @('# Release checksums / Контрольные суммы', '', 'High FPS Support 1.1.0. Compare the ZIP hash with the separately published release checksum before extracting. / Сверьте хеш ZIP с отдельно опубликованной суммой релиза до распаковки.', '', 'Run `powershell -File .\verify-release.ps1` from the extracted folder to check the files. Checksums are not signatures or malware scans. / Хеши не являются цифровой подписью или антивирусной проверкой.', '', '| File | SHA-256 |', '| --- | --- |')
foreach ($name in @('HighFpsSupport.exe', 'HighFPS.Support.dll', 'Mono.Cecil.dll')) {
    $packagedHashes += '| ' + $name + ' | `' + (Get-FileHash -LiteralPath (Join-Path $stage $name) -Algorithm SHA256).Hash + '` |'
}
$packagedHashes | Set-Content -LiteralPath (Join-Path $stage 'docs\release-hashes.md') -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'verify-release.ps1') -Destination (Join-Path $stage 'verify-release.ps1')
$files = @(Get-ChildItem -LiteralPath $stage -File -Recurse | Sort-Object FullName)
$hashLines = foreach ($file in $files) {
    $name = $file.FullName.Substring($stage.Length + 1).Replace('\', '/')
    (Get-FileHash -Algorithm SHA256 -LiteralPath $file.FullName).Hash + ' *' + $name
}
$hashLines | Set-Content -Encoding ASCII -LiteralPath (Join-Path $stage 'SHA256SUMS.txt')
& (Join-Path $PSScriptRoot 'verify-release.ps1') -Directory $stage
$archiveName = 'HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip'
$zip = Join-Path $release $archiveName
# EN: Explicit portable ZIP entry names avoid PowerShell 5.1's backslash paths and hidden-file omissions.
# RU: Явные имена ZIP с прямым слешем исключают ошибки путей и пропуски скрытых файлов в PowerShell 5.1.
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zipStream = [IO.File]::Open($zip + '.building', [IO.FileMode]::Create)
try {
    $archive = New-Object IO.Compression.ZipArchive($zipStream, [IO.Compression.ZipArchiveMode]::Create, $false)
    try {
        foreach ($file in Get-ChildItem -LiteralPath $stage -File -Recurse -Force | Sort-Object FullName) {
            $entryName = $file.FullName.Substring($stage.Length + 1).Replace('\', '/')
            [void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $file.FullName, $entryName, [IO.Compression.CompressionLevel]::Optimal)
        }
    } finally { $archive.Dispose() }
} finally { $zipStream.Dispose() }
Move-Item -LiteralPath ($zip + '.building') -Destination $zip -Force
$archiveHash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash
"$archiveHash *$archiveName" | Set-Content -Encoding ASCII -LiteralPath (Join-Path $release 'SHA256SUMS.txt')
Copy-Item -Path (Join-Path $stage '*') -Destination $dist -Recurse -Force
$hashDoc = @('# Release checksums / Контрольные суммы', '', 'Version **1.1.0**, Terraria **1.4.5.8**, Windows x86.', '', 'SHA-256 values for this local build. Publish the ZIP and `release/SHA256SUMS.txt` together in GitHub Releases. These values detect file changes; they do not establish publisher identity or prove safety. Rebuilds can have different hashes.', '', '| File | SHA-256 |', '| --- | --- |')
foreach ($name in @('HighFpsSupport.exe', 'HighFPS.Support.dll', 'Mono.Cecil.dll')) {
    $hashDoc += '| ' + $name + ' | `' + (Get-FileHash -LiteralPath (Join-Path $stage $name) -Algorithm SHA256).Hash + '` |'
}
$hashDoc += '| ' + $archiveName + ' | `' + $archiveHash + '` |'
$hashDoc += @('', ('Verify extracted files with `powershell -File .\verify-release.ps1`. Verify the ZIP with `Get-FileHash -Algorithm SHA256 .\' + $archiveName + '` and compare against the table above.'), '', 'RU: Сверяйте хеш с отдельной доверенной копией этой страницы. Хеши не заменяют подпись издателя, изучение исходников или проверку антивирусом.')
$hashDoc | Set-Content -LiteralPath (Join-Path $root 'docs\release-hashes.md') -Encoding UTF8
Write-Host "Built launcher: $dist\HighFpsSupport.exe"
Write-Host "Release: $zip"
