# High FPS Support

**Des mouvements plus fluides dans Terraria, à la fréquence de votre écran.**  
Lanceur non officiel et open source de **pavlikmeow** · Version **1.1.0**

[English](../README.md) · [Русский](README.ru.md) · [Deutsch](README.de.md) · [Español](README.es.md) · **Français** · [Português (Brasil)](README.pt-BR.md) · [简体中文](README.zh-CN.md)

Terraria met son monde à jour 60 fois par seconde. Ce mod dessine des positions intermédiaires pour les joueurs, ennemis, projectiles et objets au sol. Le mouvement paraît ainsi plus fluide sur les écrans 120/144/165/240 Hz. La vitesse du jeu reste identique. Aucun tModLoader n'est nécessaire.

## Prérequis

Seul **Steam Terraria 1.4.5.8 pour Windows, EXE x86 d'origine**, est pris en charge. .NET Framework 4.x et XNA Framework 4 doivent être installés ; lancez une fois le jeu original depuis Steam pour terminer l'installation des prérequis. Les autres versions, plateformes, tModLoader et autres correctifs de l'EXE ne sont pas pris en charge. Utilisez votre propre copie sous licence. Sauvegardez vos mondes et personnages importants : ce mod utilise les sauvegardes habituelles de Terraria.

## Commencer à jouer

1. Dans **Releases** de ce dépôt, téléchargez `HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip`. **Extrayez toute l'archive** dans un dossier. Ne lancez pas le programme depuis le ZIP et ne déplacez pas seulement l'EXE.
2. Vérifiez le téléchargement comme indiqué ci-dessous. Fermez Terraria et laissez Steam ouvert.
3. Ouvrez **`HighFpsSupport.exe`**. En haut à droite, sous **Language / Язык**, choisissez **Français**. Le choix est mémorisé et ne change que le lanceur.
4. Vérifiez le dossier détecté. Au besoin, sélectionnez celui qui contient **`Terraria.exe` et `Content`**. Dans Steam : Terraria → Propriétés → Fichiers installés → Parcourir.
5. Cliquez sur **Installer et jouer**. Ensuite, utilisez **Jouer**.

Le mod active **Frame Skip: Off**. Choisissez la fréquence réelle de votre écran dans Windows. Les images supplémentaires demandent des ressources CPU/GPU ; aucun nombre précis de FPS n'est garanti.

**Mise à jour :** fermez le jeu, extrayez la nouvelle version du mod dans un autre dossier, puis cliquez sur **Installer / mettre à jour**. Après une mise à jour de Terraria, il faut un mod explicitement compatible avec cette version. Les versions incompatibles sont refusées.

**Suppression :** fermez le jeu et choisissez **Supprimer High FPS**. Steam continue de lancer le jeu original. Pour une suppression manuelle, effacez uniquement `Terraria.HighFPS.exe`, `HighFPS.Support.dll`, `HighFPS.Support.install.txt` et `HighFPS.Support.log` du dossier du jeu. `Terraria.exe` et vos sauvegardes restent présents. Les préférences du lanceur sont dans `%LOCALAPPDATA%\TerrariaHighFPS` ; Terraria peut conserver Frame Skip: Off dans ses propres réglages.

## Vérifier le téléchargement

Comparez le hash du ZIP avec les [sommes de contrôle de la même version](release-hashes.md). Dans PowerShell, depuis le dossier de téléchargement :

```powershell
Get-FileHash -Algorithm SHA256 -LiteralPath .\HighFPS-Support-1.1.0-Terraria-1.4.5.8-win-x86.zip
```

L'archive contient `SHA256SUMS.txt`. Après extraction, lisez `verify-release.ps1`, puis exécutez depuis le dossier extrait :

```powershell
powershell -NoProfile -File .\verify-release.ps1
```

Si les scripts sont bloqués, comparez les fichiers individuellement avec `Get-FileHash` et `SHA256SUMS.txt` ; modifier la politique de sécurité n'est pas nécessaire. Ne lancez rien si un hash diffère.

**Un hash identique confirme la correspondance avec une somme de confiance, pas l'innocuité du programme.** L'application n'est pas signée : Windows peut afficher un éditeur inconnu ou SmartScreen. Aucun audit de sécurité indépendant ni compilation reproductible bit à bit n'est revendiqué. Gardez votre antivirus activé.

## Fonctionnement et confiance

Le lanceur crée séparément `Terraria.HighFPS.exe` et `HighFPS.Support.dll`. **L'original n'est ni écrasé ni renommé.** Trois appels ajoutés capturent l'état avant un tick, interpolent les positions pendant le dessin et restaurent ensuite les coordonnées de simulation. La logique et le réseau ne sont pas accélérés. L'interpolation peut ajouter jusqu'à un tick de retard visuel ; toutes les animations ne sont pas concernées.

Le lanceur ne comporte ni télémétrie, connexion à un compte, téléchargement pendant l'utilisation ou mise à jour automatique. Il n'installe aucun service ni pilote. Le chemin, la langue, les données d'installation et les journaux restent locaux. Steam et Terraria gardent leur fonctionnement réseau habituel. La compilation peut télécharger la version fixée de Mono.Cecil depuis NuGet en vérifiant ses hashes.

En cas d'erreur : fermez complètement le jeu, extrayez de nouveau tout le ZIP et vérifiez la version ainsi que les droits d'écriture. **Installer / mettre à jour** permet de réparer l'installation. Consultez **Détails techniques** et retirez les chemins personnels avant tout partage. [Aide détaillée (EN)](guide.md) · [Sécurité (EN/RU)](../SECURITY.md) · [Architecture (EN)](architecture.md) · [Compiler (EN)](building.md).

## Licence et crédits

Code et documentation propres au projet : [MIT](../LICENSE), © 2026 pavlikmeow. Mono.Cecil possède son propre avis MIT. Merci à [TerrariaHighFPS](https://github.com/Yukurotei/TerrariaHighFPS) pour l'approche décrite publiquement ; cette mention n'autorise pas la copie de sources sans licence. [Avis complets (EN/RU)](../THIRD-PARTY-NOTICES.md).

Projet de fans indépendant, sans affiliation ni approbation de Re-Logic, Valve ou Microsoft. Terraria appartient à Re-Logic ; les autres noms appartiennent à leurs titulaires. Le jeu, ses ressources et XNA ne sont pas distribués. Ne publiez pas l'EXE du jeu généré localement. La licence du projet ne donne aucun droit sur ces produits ; leurs conditions et le droit applicable restent en vigueur.
