# High FPS Support

**Movimento mais fluido no Terraria, na frequência da sua tela.**  
Launcher não oficial de código aberto por **pavlikmeow** · Versão **1.1.0**

[English](../README.md) · [Русский](README.ru.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md) · **Português (Brasil)** · [简体中文](README.zh-CN.md)

O Terraria atualiza o mundo 60 vezes por segundo. Este mod desenha posições intermediárias de jogadores, inimigos, projéteis e itens no chão, deixando o movimento mais fluido em telas de 120/144/165/240 Hz. A velocidade do jogo continua igual. Não precisa de tModLoader.

## Requisitos

Compatível somente com **Steam Terraria 1.4.5.8 para Windows, EXE original x86**. É necessário ter .NET Framework 4.x e XNA Framework 4; abra o jogo original uma vez pelo Steam para concluir a instalação dos componentes. Outras versões, plataformas, tModLoader e outros patches do EXE não são compatíveis. Use sua própria cópia licenciada. Faça backup dos mundos e personagens importantes: o mod usa os saves normais do Terraria.

## Comece a jogar

1. Na seção **Releases** deste repositório, baixe `HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip`. **Extraia todo o arquivo** para uma pasta. Não execute dentro do ZIP nem mova só o EXE.
2. Verifique o download conforme as instruções abaixo. Feche o Terraria e deixe o Steam aberto.
3. Abra **`HighFpsSupport.exe`**. No canto superior direito, em **Language / Язык**, escolha **Português (Brasil)**. A escolha fica salva e só altera o launcher.
4. Confira a pasta encontrada. Se necessário, selecione a que contém **`Terraria.exe` e `Content`**. No Steam: Terraria → Propriedades → Arquivos instalados → Explorar.
5. Clique em **Instalar e jogar**. Depois, use **Jogar**.

O mod ativa **Frame Skip: Off**. Selecione a frequência real do monitor nas configurações do Windows. Mais quadros exigem capacidade de CPU/GPU; não há garantia de um FPS específico.

**Atualizar:** feche o jogo, extraia a nova versão do mod em outra pasta e clique em **Instalar / atualizar**. Após uma atualização do Terraria, use uma versão do mod explicitamente compatível com ela. Versões incompatíveis são recusadas.

**Remover:** feche o jogo e escolha **Remover High FPS**. O Steam continua abrindo o jogo original. Para remover manualmente, apague apenas `Terraria.HighFPS.exe`, `HighFPS.Support.dll`, `HighFPS.Support.install.txt` e `HighFPS.Support.log` da pasta do jogo. `Terraria.exe` e os saves permanecem. As preferências ficam em `%LOCALAPPDATA%\TerrariaHighFPS`; o Terraria pode manter Frame Skip: Off nas próprias configurações.

## Verificar o download

Compare o hash do ZIP com as [somas de verificação da mesma versão](release-hashes.md). Abra o PowerShell na pasta do download:

```powershell
Get-FileHash -Algorithm SHA256 -LiteralPath .\HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip
```

O arquivo inclui `SHA256SUMS.txt`. Depois de extrair, leia `verify-release.ps1` e execute na pasta extraída:

```powershell
powershell -NoProfile -File .\verify-release.ps1
```

Se scripts estiverem bloqueados, compare os arquivos individualmente usando `Get-FileHash` e `SHA256SUMS.txt`; não é necessário mudar a política de segurança. Não execute o programa se algum hash for diferente.

**Um hash igual confirma a correspondência com uma soma confiável, não que o programa seja inofensivo.** O aplicativo não é assinado: o Windows pode mostrar um editor desconhecido ou o SmartScreen. Não há alegação de auditoria de segurança independente nem de compilações reproduzíveis bit a bit. Mantenha o antivírus ativado.

## Como funciona e o que muda

O launcher cria `Terraria.HighFPS.exe` e `HighFPS.Support.dll` separadamente. **O original não é sobrescrito nem renomeado.** Três chamadas adicionadas capturam o estado antes de um tick, interpolam posições durante o desenho e restauram as coordenadas da simulação depois. A lógica e a rede não são aceleradas. A interpolação pode acrescentar até um tick de atraso visual; nem todas as animações são interpoladas.

O launcher não tem telemetria, login, downloads durante o uso ou atualização automática, e não instala serviços nem drivers. Caminho, idioma, dados da instalação e logs ficam locais. Steam e Terraria continuam usando a rede normalmente. Ao compilar, a versão fixada do Mono.Cecil pode ser baixada do NuGet com verificação de hashes.

Se houver erro: feche o jogo completamente, extraia todo o ZIP novamente e confira a versão e a permissão de escrita. **Instalar / atualizar** permite reparar a instalação. Consulte **Detalhes técnicos** e remova caminhos pessoais antes de compartilhar. [Ajuda completa (EN)](guide.md) · [Segurança (EN/RU)](../SECURITY.md) · [Arquitetura (EN)](architecture.md) · [Compilar (EN)](building.md).

## Licença e créditos

Código e documentação próprios: [MIT](../LICENSE), © 2026 pavlikmeow. Mono.Cecil tem seu próprio aviso MIT. Créditos a [TerrariaHighFPS](https://github.com/Yukurotei/TerrariaHighFPS) pela abordagem descrita publicamente; a menção não autoriza copiar código sem licença. [Avisos completos (EN/RU)](../THIRD-PARTY-NOTICES.md).

Projeto independente de fãs, sem vínculo ou aprovação de Re-Logic, Valve ou Microsoft. Terraria pertence à Re-Logic; outros nomes pertencem aos respectivos titulares. O jogo, seus recursos e XNA não são distribuídos. Não publique o EXE do jogo gerado localmente. A licença do projeto não concede direitos sobre esses produtos; os termos deles e a legislação aplicável continuam valendo.
