# Release checksums / Контрольные суммы

Version **1.1.0**, Terraria **1.4.5.8**, Windows x86.

SHA-256 values for this local build. Publish the ZIP and `release/SHA256SUMS.txt` together in GitHub Releases. These values detect file changes; they do not establish publisher identity or prove safety. Rebuilds can have different hashes.

| File | SHA-256 |
| --- | --- |
| HighFpsSupport.exe | `84D5399605DCA0F6EE2657D1A7CD12DDF439172C3FE3E8A6F102A8404DE178E0` |
| HighFPS.Support.dll | `D1E99E375B5DBEC85B262EDEFD549798FB18295838B2D13FD7B85C81036C6F8F` |
| Mono.Cecil.dll | `C41BDB9FFD3C5F6E17D2382C1012D73703E035E3F1100245FDD4E08C8DC6EB5B` |
| HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip | `A63A4005EE6374D9F8A4ED7D12447C17F898E7C0E3FC9973BC660C94015D9C7A` |

Verify extracted files with `powershell -File .\verify-release.ps1`. Verify the ZIP with `Get-FileHash -Algorithm SHA256 .\HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip` and compare against the table above.

RU: Сверяйте хеш с отдельной доверенной копией этой страницы. Хеши не заменяют подпись издателя, изучение исходников или проверку антивирусом.
