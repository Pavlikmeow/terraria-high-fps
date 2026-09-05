using System;
using System.Collections.Generic;
using System.Globalization;

namespace TerrariaHighFPS.Launcher
{
    internal sealed class LanguageOption
    {
        public readonly string Code;
        public readonly string NativeName;
        public LanguageOption(string code, string nativeName) { Code = code; NativeName = nativeName; }
        public override string ToString() { return NativeName; }
    }

    internal sealed class Localization
    {
        public static readonly LanguageOption[] Languages = {
            new LanguageOption("en", "English"), new LanguageOption("ru", "Русский"),
            new LanguageOption("de", "Deutsch"), new LanguageOption("es", "Español"),
            new LanguageOption("fr", "Français"), new LanguageOption("pt-BR", "Português (Brasil)"),
            new LanguageOption("zh-Hans", "简体中文")
        };

        // EN: Every key has all seven translations. The UI harness checks this contract before release.
        // RU: У каждого ключа есть семь переводов. UI-проверка контролирует это перед выпуском.
        private static readonly Dictionary<string, string[]> Strings = CreateStrings();
        private readonly int _index;
        public readonly string Code;

        public Localization(string code)
        {
            Code = IsSupported(code) ? code : "en";
            for (int i = 0; i < Languages.Length; i++) if (Languages[i].Code == Code) _index = i;
        }

        public string this[string key] { get { return Strings[key][_index]; } }
        public string Format(string key, params object[] values)
        {
            return string.Format(CultureInfo.CurrentCulture, this[key], values);
        }

        public static bool IsSupported(string code)
        {
            foreach (LanguageOption language in Languages) if (language.Code == code) return true;
            return false;
        }

        public static string MatchCulture(string culture)
        {
            if (string.IsNullOrEmpty(culture)) return "en";
            foreach (LanguageOption language in Languages)
                if (culture.Equals(language.Code, StringComparison.OrdinalIgnoreCase)) return language.Code;
            string prefix = culture.Split('-')[0].ToLowerInvariant();
            if (prefix == "pt") return "pt-BR";
            // EN: Do not silently substitute Simplified Chinese for a Traditional Chinese locale.
            // RU: Не подменяем традиционный китайский упрощённым без выбора пользователя.
            if (culture.Equals("zh-CN", StringComparison.OrdinalIgnoreCase) || culture.Equals("zh-SG", StringComparison.OrdinalIgnoreCase)) return "zh-Hans";
            foreach (LanguageOption language in Languages) if (language.Code == prefix) return language.Code;
            return "en";
        }

        internal static void ValidateTranslations()
        {
            foreach (KeyValuePair<string, string[]> entry in Strings)
            {
                if (entry.Value.Length != Languages.Length) throw new InvalidOperationException("Translation count: " + entry.Key);
                foreach (string translation in entry.Value)
                {
                    if (string.IsNullOrWhiteSpace(translation)) throw new InvalidOperationException("Empty translation: " + entry.Key);
                    string.Format(CultureInfo.InvariantCulture, translation, "a", "b", "c");
                }
            }
        }

        private static Dictionary<string, string[]> CreateStrings()
        {
            var d = new Dictionary<string, string[]>(StringComparer.Ordinal);
            Add(d, "hero", "A smoother Terraria.", "Terraria. Ещё плавнее.", "Terraria. Noch flüssiger.", "Terraria, más fluido.", "Terraria, plus fluide.", "Terraria, mais fluido.", "让 Terraria 更流畅。");
            Add(d, "subtitle", "High refresh rate rendering. The familiar 60 Hz game simulation. No tModLoader required.", "Плавная отрисовка. Привычная логика игры на 60 Гц. Без tModLoader.", "Hohe Bildwiederholrate. Bewährte 60-Hz-Spielsimulation. Ohne tModLoader.", "Imagen más fluida. Simulación del juego a 60 Hz. Sin tModLoader.", "Affichage haute fréquence. Simulation du jeu à 60 Hz. Sans tModLoader.", "Alta taxa de atualização. Simulação do jogo a 60 Hz. Sem tModLoader.", "高刷新率画面渲染，保留 60 Hz 游戏逻辑。无需 tModLoader。");
            Add(d, "compatibility", "Windows · Terraria 1.4.5.8 · Steam", "Windows · Terraria 1.4.5.8 · Steam", "Windows · Terraria 1.4.5.8 · Steam", "Windows · Terraria 1.4.5.8 · Steam", "Windows · Terraria 1.4.5.8 · Steam", "Windows · Terraria 1.4.5.8 · Steam", "Windows · Terraria 1.4.5.8 · Steam");
            Add(d, "detecting", "Looking for Terraria…", "Ищем Terraria…", "Terraria wird gesucht…", "Buscando Terraria…", "Recherche de Terraria…", "Procurando Terraria…", "正在查找 Terraria…");
            Add(d, "detectHint", "Checking your saved folder and Steam libraries.", "Проверяем сохранённую папку и библиотеки Steam.", "Gespeicherter Ordner und Steam-Bibliotheken werden geprüft.", "Comprobando la carpeta guardada y las bibliotecas de Steam.", "Vérification du dossier enregistré et des bibliothèques Steam.", "Verificando a pasta salva e as bibliotecas do Steam.", "正在检查已保存的文件夹和 Steam 游戏库。");
            Add(d, "missing", "Choose your game folder", "Выберите папку игры", "Spielordner auswählen", "Elige la carpeta del juego", "Choisissez le dossier du jeu", "Escolha a pasta do jogo", "选择游戏文件夹");
            Add(d, "missingHint", "Select the folder containing Terraria.exe and Content to get started.", "Для начала выберите папку с Terraria.exe и Content.", "Wähle zuerst den Ordner mit Terraria.exe und Content.", "Para empezar, selecciona la carpeta con Terraria.exe y Content.", "Pour commencer, sélectionnez le dossier contenant Terraria.exe et Content.", "Para começar, selecione a pasta com Terraria.exe e Content.", "请选择包含 Terraria.exe 和 Content 的文件夹。");
            Add(d, "ready", "Ready for a smoother game", "Всё готово к установке", "Bereit für flüssigeres Spielen", "Todo listo para instalar", "Prêt pour l’installation", "Tudo pronto para instalar", "可以开始安装了");
            Add(d, "readyHint", "Install a separate High FPS copy, then start playing.", "Создайте отдельную High FPS версию и начните играть.", "Installiere eine separate High-FPS-Kopie und starte das Spiel.", "Instala una copia High FPS independiente y empieza a jugar.", "Installez une copie High FPS séparée, puis lancez le jeu.", "Instale uma cópia High FPS separada e comece a jogar.", "安装独立的 High FPS 副本，然后开始游戏。");
            Add(d, "installed", "High FPS is installed", "High FPS установлен", "High FPS ist installiert", "High FPS está instalado", "High FPS est installé", "High FPS está instalado", "High FPS 已安装");
            Add(d, "installedHint", "Your installation is checked before every launch.", "Перед каждым запуском установка проверяется.", "Die Installation wird vor jedem Start geprüft.", "La instalación se verifica antes de cada inicio.", "L’installation est vérifiée avant chaque lancement.", "A instalação é verificada antes de cada início.", "每次启动前都会检查安装状态。");
            Add(d, "folder", "GAME LOCATION", "ПАПКА ИГРЫ", "SPIELORDNER", "CARPETA DEL JUEGO", "DOSSIER DU JEU", "PASTA DO JOGO", "游戏位置");
            Add(d, "noFolder", "No folder selected", "Папка не выбрана", "Kein Ordner ausgewählt", "Ninguna carpeta seleccionada", "Aucun dossier sélectionné", "Nenhuma pasta selecionada", "未选择文件夹");
            Add(d, "browse", "Choose folder…", "Выбрать папку…", "Ordner wählen…", "Elegir carpeta…", "Choisir un dossier…", "Escolher pasta…", "选择文件夹…");
            Add(d, "installPlay", "Install & play", "Установить и играть", "Installieren & spielen", "Instalar y jugar", "Installer et jouer", "Instalar e jogar", "安装并开始游戏");
            Add(d, "play", "Play", "Играть", "Spielen", "Jugar", "Jouer", "Jogar", "开始游戏");
            Add(d, "installOnly", "Install / update only", "Установить / обновить", "Installieren / aktualisieren", "Instalar / actualizar", "Installer / mettre à jour", "Instalar / atualizar", "仅安装 / 更新");
            Add(d, "openFolder", "Open game folder", "Открыть папку игры", "Spielordner öffnen", "Abrir carpeta del juego", "Ouvrir le dossier du jeu", "Abrir pasta do jogo", "打开游戏文件夹");
            Add(d, "remove", "Remove High FPS", "Удалить High FPS", "High FPS entfernen", "Eliminar High FPS", "Supprimer High FPS", "Remover High FPS", "移除 High FPS");
            Add(d, "originalTitle", "Your original game stays intact.", "Оригинальная игра остаётся нетронутой.", "Dein Originalspiel bleibt unverändert.", "El juego original queda intacto.", "Votre jeu original reste intact.", "Seu jogo original permanece intacto.", "保留原版游戏，安心体验。");
            Add(d, "originalBody", "Creates Terraria.HighFPS.exe beside the original. No changes to saves or Terraria.exe.", "Рядом создаётся Terraria.HighFPS.exe. Terraria.exe и сохранения не изменяются.", "Erstellt Terraria.HighFPS.exe neben dem Original. Terraria.exe und Spielstände bleiben unverändert.", "Crea Terraria.HighFPS.exe junto al original. No modifica Terraria.exe ni las partidas guardadas.", "Crée Terraria.HighFPS.exe à côté de l’original. Terraria.exe et les sauvegardes restent intacts.", "Cria Terraria.HighFPS.exe ao lado do original. Não altera Terraria.exe nem os arquivos de save.", "在原版旁创建 Terraria.HighFPS.exe。安装过程不修改 Terraria.exe 或存档。");
            Add(d, "privacy", "Runs locally · No accounts · No telemetry", "Работает локально · Без аккаунтов и телеметрии", "Lokal · Kein Konto · Keine Telemetrie", "Ejecución local · Sin cuentas · Sin telemetría", "Fonctionne localement · Sans compte ni télémétrie", "Execução local · Sem contas · Sem telemetria", "本地运行 · 无需账户 · 无遥测");
            Add(d, "help", "How it works", "Как пользоваться", "So funktioniert’s", "Cómo usarlo", "Mode d’emploi", "Como usar", "使用方法");
            Add(d, "details", "Technical details", "Технические детали", "Technische Details", "Detalles técnicos", "Détails techniques", "Detalhes técnicos", "技术详情");
            Add(d, "hideDetails", "Hide details", "Скрыть детали", "Details ausblenden", "Ocultar detalles", "Masquer les détails", "Ocultar detalhes", "隐藏详情");
            Add(d, "copy", "Copy details", "Копировать детали", "Details kopieren", "Copiar detalles", "Copier les détails", "Copiar detalhes", "复制详情");
            Add(d, "detailsIntro", "Local activity only. Diagnostic messages can include your game folder; review before sharing.", "Только локальные события. Диагностика может содержать путь к игре; проверьте её перед публикацией.", "Nur lokale Aktivitäten. Diagnosen können den Spielpfad enthalten; vor dem Teilen prüfen.", "Solo actividad local. El diagnóstico puede incluir la ruta del juego; revísalo antes de compartirlo.", "Activité locale uniquement. Les diagnostics peuvent inclure le chemin du jeu ; vérifiez avant de partager.", "Apenas atividade local. O diagnóstico pode conter o caminho do jogo; revise antes de compartilhar.", "仅记录本地活动。诊断信息可能包含游戏路径，请在分享前检查。");
            Add(d, "chooseDescription", "Choose the folder containing Terraria.exe and the Content folder.", "Выберите папку, в которой находятся Terraria.exe и папка Content.", "Wähle den Ordner mit Terraria.exe und dem Unterordner Content.", "Elige la carpeta que contiene Terraria.exe y la carpeta Content.", "Choisissez le dossier contenant Terraria.exe et le sous-dossier Content.", "Escolha a pasta que contém Terraria.exe e a pasta Content.", "请选择包含 Terraria.exe 文件和 Content 文件夹的目录。");
            Add(d, "invalidDirectory", "This is not a Terraria game folder. Choose the folder containing the original Terraria.exe and Content.", "Это не папка Terraria. Выберите папку с оригинальным Terraria.exe и Content.", "Dies ist kein Terraria-Spielordner. Wähle den Ordner mit der originalen Terraria.exe und Content.", "No es una carpeta de Terraria. Elige la carpeta con el Terraria.exe original y Content.", "Ce n’est pas un dossier Terraria. Choisissez celui avec Terraria.exe d’origine et Content.", "Esta não é uma pasta do Terraria. Escolha a pasta com o Terraria.exe original e Content.", "这不是 Terraria 游戏文件夹。请选择包含原版 Terraria.exe 和 Content 的文件夹。");
            Add(d, "removeTitle", "Remove High FPS?", "Удалить High FPS?", "High FPS entfernen?", "¿Eliminar High FPS?", "Supprimer High FPS ?", "Remover High FPS?", "移除 High FPS？");
            Add(d, "removeConfirm", "Remove the High FPS executable, support library and local installation files? The original game and saves stay in place.", "Удалить High FPS версию, библиотеку мода и локальные файлы установки? Оригинальная игра и сохранения останутся на месте.", "High-FPS-Programm, Hilfsbibliothek und lokale Installationsdateien entfernen? Originalspiel und Spielstände bleiben erhalten.", "¿Eliminar el ejecutable High FPS, su biblioteca y los archivos de instalación locales? El juego original y las partidas se conservan.", "Supprimer l’exécutable High FPS, sa bibliothèque et les fichiers locaux d’installation ? Le jeu original et les sauvegardes sont conservés.", "Remover o executável High FPS, a biblioteca e os arquivos locais de instalação? O jogo original e os saves serão mantidos.", "移除 High FPS 程序、支持库和本地安装文件？原版游戏和存档会保留。");
            Add(d, "installing", "Preparing High FPS…", "Подготавливаем High FPS…", "High FPS wird vorbereitet…", "Preparando High FPS…", "Préparation de High FPS…", "Preparando High FPS…", "正在准备 High FPS…");
            Add(d, "launching", "Checking and starting your game…", "Проверяем и запускаем игру…", "Spiel wird geprüft und gestartet…", "Verificando e iniciando el juego…", "Vérification et lancement du jeu…", "Verificando e iniciando o jogo…", "正在检查并启动游戏…");
            Add(d, "removing", "Removing High FPS…", "Удаляем High FPS…", "High FPS wird entfernt…", "Eliminando High FPS…", "Suppression de High FPS…", "Removendo High FPS…", "正在移除 High FPS…");
            Add(d, "busyHint", "Please wait. This window will be available when the operation finishes.", "Подождите. Окно станет доступно после завершения операции.", "Bitte warten. Nach Abschluss ist das Fenster wieder verfügbar.", "Espera. La ventana estará disponible al finalizar la operación.", "Veuillez patienter. La fenêtre sera disponible à la fin de l’opération.", "Aguarde. A janela estará disponível ao concluir a operação.", "请稍候，操作完成后即可继续使用此窗口。");
            Add(d, "closeBusy", "Finishing the current operation. Close this window once it completes.", "Завершаем текущую операцию. Закройте окно после её окончания.", "Der Vorgang wird abgeschlossen. Schließe das Fenster danach.", "Terminando la operación. Cierra la ventana cuando finalice.", "Opération en cours. Fermez la fenêtre une fois celle-ci terminée.", "Concluindo a operação. Feche a janela após o término.", "正在完成当前操作，请在完成后关闭窗口。");
            Add(d, "created", "Installation ready. Terraria {0}; verified hooks: {1}.", "Установка готова. Terraria {0}; проверено хуков: {1}.", "Installation bereit. Terraria {0}; geprüfte Hooks: {1}.", "Instalación lista. Terraria {0}; hooks verificados: {1}.", "Installation prête. Terraria {0} ; hooks vérifiés : {1}.", "Instalação pronta. Terraria {0}; hooks verificados: {1}.", "安装已就绪。Terraria {0}；已验证挂钩：{1}。");
            Add(d, "current", "The installation is up to date.", "Установлена актуальная версия.", "Die Installation ist aktuell.", "La instalación está actualizada.", "L’installation est à jour.", "A instalação está atualizada.", "安装已是最新状态。");
            Add(d, "launched", "Game started. Frame Skip is set to Off for this session.", "Игра запущена. Frame Skip отключён на время этой сессии.", "Spiel gestartet. Frame Skip ist für diese Sitzung deaktiviert.", "Juego iniciado. Frame Skip está desactivado durante esta sesión.", "Jeu lancé. Frame Skip est désactivé pour cette session.", "Jogo iniciado. Frame Skip está desativado nesta sessão.", "游戏已启动。本次游戏会话的 Frame Skip 已关闭。");
            Add(d, "installedDone", "High FPS is ready to play.", "High FPS готов к игре.", "High FPS ist spielbereit.", "High FPS está listo para jugar.", "High FPS est prêt à jouer.", "High FPS está pronto para jogar.", "High FPS 已准备就绪。");
            Add(d, "removed", "High FPS removed. Your original game is still available in Steam.", "High FPS удалён. Оригинальная игра доступна в Steam.", "High FPS entfernt. Das Originalspiel ist weiterhin über Steam verfügbar.", "High FPS eliminado. El juego original sigue disponible en Steam.", "High FPS supprimé. Votre jeu original reste disponible dans Steam.", "High FPS removido. O jogo original continua disponível no Steam.", "High FPS 已移除。仍可从 Steam 启动原版游戏。");
            Add(d, "errorTitle", "The operation could not finish", "Не удалось завершить операцию", "Der Vorgang konnte nicht abgeschlossen werden", "No se pudo completar la operación", "L’opération n’a pas pu aboutir", "Não foi possível concluir a operação", "操作未能完成");
            Add(d, "errorGeneric", "Try again after closing Terraria. Open Technical details for the diagnostic message.", "Закройте Terraria и повторите попытку. Диагностика доступна в разделе «Технические детали».", "Schließe Terraria und versuche es erneut. Die Diagnose steht unter Technische Details.", "Cierra Terraria e inténtalo de nuevo. Consulta el diagnóstico en Detalles técnicos.", "Fermez Terraria et réessayez. Consultez le diagnostic dans Détails techniques.", "Feche o Terraria e tente novamente. Veja o diagnóstico em Detalhes técnicos.", "关闭 Terraria 后重试。可在“技术详情”中查看诊断信息。");
            Add(d, "errorAccess", "Windows denied access to a file. Check the game folder permissions and your security software, then try again.", "Windows запретила доступ к файлу. Проверьте права на папку игры и сообщения защитного ПО, затем повторите попытку.", "Windows hat den Dateizugriff verweigert. Prüfe Ordnerberechtigungen und Sicherheitssoftware und versuche es erneut.", "Windows denegó el acceso a un archivo. Revisa los permisos de la carpeta y el software de seguridad, y reinténtalo.", "Windows a refusé l’accès à un fichier. Vérifiez les permissions du dossier et votre logiciel de sécurité, puis réessayez.", "O Windows negou acesso a um arquivo. Confira as permissões da pasta e seu software de segurança e tente novamente.", "Windows 拒绝访问文件。请检查游戏文件夹权限和安全软件提示，然后重试。");
            Add(d, "errorCompatibility", "This release supports the original Windows Steam version of Terraria 1.4.5.8. See Technical details for what was detected.", "Этот выпуск поддерживает оригинальную Terraria 1.4.5.8 для Windows из Steam. Найденная версия указана в технических деталях.", "Diese Ausgabe unterstützt die originale Windows-Steam-Version von Terraria 1.4.5.8. Die erkannte Version steht in den technischen Details.", "Esta versión admite Terraria 1.4.5.8 original de Steam para Windows. Consulta la versión detectada en Detalles técnicos.", "Cette version prend en charge Terraria 1.4.5.8 original sur Steam pour Windows. Consultez la version détectée dans les détails techniques.", "Esta versão suporta o Terraria 1.4.5.8 original do Steam para Windows. Veja a versão detectada nos detalhes técnicos.", "此版本支持 Windows Steam 原版 Terraria 1.4.5.8。检测结果请查看技术详情。");
            Add(d, "errorRunning", "Close Terraria before installing, updating or removing High FPS.", "Закройте Terraria перед установкой, обновлением или удалением High FPS.", "Schließe Terraria, bevor du High FPS installierst, aktualisierst oder entfernst.", "Cierra Terraria antes de instalar, actualizar o eliminar High FPS.", "Fermez Terraria avant d’installer, de mettre à jour ou de supprimer High FPS.", "Feche o Terraria antes de instalar, atualizar ou remover o High FPS.", "请先关闭 Terraria，再安装、更新或移除 High FPS。");
            Add(d, "errorInProgress", "Another High FPS operation is running. Let it finish, then try again.", "Уже выполняется другая операция High FPS. Дождитесь её окончания и повторите попытку.", "Ein anderer High-FPS-Vorgang läuft. Warte auf dessen Abschluss und versuche es erneut.", "Hay otra operación High FPS en curso. Espera a que termine e inténtalo de nuevo.", "Une autre opération High FPS est en cours. Attendez sa fin, puis réessayez.", "Outra operação High FPS está em andamento. Aguarde a conclusão e tente novamente.", "另一项 High FPS 操作正在进行，请等待完成后重试。");
            Add(d, "errorInstall", "The High FPS installation needs repair. Close Terraria and choose Install / update only.", "Установку High FPS нужно восстановить. Закройте Terraria и нажмите «Установить / обновить».", "High FPS muss repariert werden. Schließe Terraria und wähle Installieren / aktualisieren.", "La instalación High FPS necesita reparación. Cierra Terraria y elige Instalar / actualizar.", "L’installation High FPS doit être réparée. Fermez Terraria et choisissez Installer / mettre à jour.", "A instalação High FPS precisa de reparo. Feche o Terraria e escolha Instalar / atualizar.", "High FPS 安装需要修复。请关闭 Terraria，然后选择“仅安装 / 更新”。");
            Add(d, "errorPackage", "The launcher package is incomplete. Extract the complete release ZIP into one folder and try again.", "В комплекте лаунчера не хватает файлов. Распакуйте весь ZIP-архив выпуска в одну папку и повторите попытку.", "Das Launcher-Paket ist unvollständig. Entpacke das gesamte Release-ZIP in einen Ordner und versuche es erneut.", "El paquete del lanzador está incompleto. Extrae todo el ZIP de la versión en una carpeta y reinténtalo.", "Le paquet du lanceur est incomplet. Extrayez toute l’archive ZIP dans un même dossier, puis réessayez.", "O pacote do launcher está incompleto. Extraia todo o ZIP da versão em uma pasta e tente novamente.", "启动器文件不完整。请将发布版 ZIP 中的所有文件解压到同一文件夹，然后重试。");
            Add(d, "settingsFailed", "The preference could not be saved. This choice still applies to the current session.", "Не удалось сохранить настройку. Выбор действует до закрытия лаунчера.", "Die Einstellung konnte nicht gespeichert werden. Sie gilt trotzdem für diese Sitzung.", "No se pudo guardar la preferencia. La selección se aplica durante esta sesión.", "Impossible d’enregistrer la préférence. Le choix reste actif pour cette session.", "Não foi possível salvar a preferência. A escolha vale para esta sessão.", "无法保存偏好设置。此选择仍会应用于当前会话。");
            Add(d, "copied", "Diagnostic details copied.", "Диагностика скопирована.", "Diagnosedetails kopiert.", "Diagnóstico copiado.", "Diagnostic copié.", "Diagnóstico copiado.", "已复制诊断详情。");
            Add(d, "copyFailed", "The clipboard is busy. Select the details and press Ctrl+C, or try again.", "Буфер обмена занят. Выделите детали и нажмите Ctrl+C или повторите попытку.", "Die Zwischenablage ist belegt. Markiere die Details und drücke Strg+C oder versuche es erneut.", "El portapapeles está ocupado. Selecciona los detalles y pulsa Ctrl+C o reinténtalo.", "Le presse-papiers est occupé. Sélectionnez les détails et appuyez sur Ctrl+C, ou réessayez.", "A área de transferência está ocupada. Selecione os detalhes e pressione Ctrl+C ou tente novamente.", "剪贴板正忙。请选择详情并按 Ctrl+C，或稍后重试。");
            Add(d, "helpBody", "1. Extract the complete release ZIP into a folder. Keep its files together.\n\n2. Close Terraria. Choose your Terraria folder if it was not detected automatically.\n\n3. Click Install & play. Later, use Play in this launcher or Terraria.HighFPS.exe. Starting Terraria from Steam still starts the original game.\n\nFor best results select a high monitor refresh rate in Windows. Frame Skip is disabled for the modded session; the game simulation stays at 60 Hz. Installation does not edit your saves.\n\nTo uninstall, close the game and choose Remove High FPS. Additional security and build details are in the README included with this release.", "1. Распакуйте весь ZIP-архив выпуска в папку. Храните его файлы вместе.\n\n2. Закройте Terraria. Выберите папку Terraria, если она не найдена автоматически.\n\n3. Нажмите «Установить и играть». В дальнейшем используйте «Играть» в лаунчере или Terraria.HighFPS.exe. Запуск Terraria из Steam по-прежнему открывает оригинальную игру.\n\nДля лучшего результата выберите высокую частоту монитора в настройках Windows. Frame Skip отключается на время сессии с модом; логика игры работает на 60 Гц. Установка не редактирует сохранения.\n\nДля удаления закройте игру и нажмите «Удалить High FPS». Подробности о безопасности и сборке — в README из комплекта выпуска.", "1. Entpacke das gesamte Release-ZIP in einen Ordner. Halte die Dateien zusammen.\n\n2. Schließe Terraria. Wähle den Terraria-Ordner, falls er nicht automatisch gefunden wurde.\n\n3. Klicke auf Installieren & spielen. Nutze später Spielen im Launcher oder Terraria.HighFPS.exe. Der Start über Steam öffnet weiterhin das Originalspiel.\n\nWähle die hohe Bildwiederholrate deines Monitors in Windows. Frame Skip wird für die modifizierte Sitzung deaktiviert; die Spielsimulation bleibt bei 60 Hz. Die Installation ändert keine Spielstände.\n\nZum Deinstallieren schließe das Spiel und wähle High FPS entfernen. Hinweise zu Sicherheit und Build stehen in der beiliegenden README.", "1. Extrae todo el ZIP de la versión en una carpeta. Mantén los archivos juntos.\n\n2. Cierra Terraria. Elige su carpeta si no se detectó automáticamente.\n\n3. Pulsa Instalar y jugar. Después, usa Jugar en el lanzador o Terraria.HighFPS.exe. Iniciar Terraria desde Steam sigue abriendo el juego original.\n\nPara mejores resultados, selecciona la frecuencia alta de tu pantalla en Windows. Frame Skip se desactiva durante la sesión con el mod; la simulación sigue a 60 Hz. La instalación no modifica las partidas guardadas.\n\nPara desinstalar, cierra el juego y elige Eliminar High FPS. Consulta el README incluido para información de seguridad y compilación.", "1. Extrayez toute l’archive ZIP dans un dossier. Conservez les fichiers ensemble.\n\n2. Fermez Terraria. Choisissez son dossier s’il n’a pas été détecté automatiquement.\n\n3. Cliquez sur Installer et jouer. Ensuite, utilisez Jouer dans le lanceur ou Terraria.HighFPS.exe. Un lancement depuis Steam ouvre toujours le jeu original.\n\nPour un meilleur résultat, sélectionnez la fréquence élevée de votre écran dans Windows. Frame Skip est désactivé pendant la session modifiée ; la simulation reste à 60 Hz. L’installation ne modifie pas les sauvegardes.\n\nPour désinstaller, fermez le jeu et choisissez Supprimer High FPS. Le README inclus contient les informations de sécurité et de compilation.", "1. Extraia todo o ZIP da versão em uma pasta. Mantenha os arquivos juntos.\n\n2. Feche o Terraria. Escolha a pasta do jogo se ela não foi detectada automaticamente.\n\n3. Clique em Instalar e jogar. Depois, use Jogar no launcher ou Terraria.HighFPS.exe. Iniciar o Terraria pelo Steam continua abrindo o jogo original.\n\nPara melhores resultados, selecione a alta taxa de atualização da tela no Windows. Frame Skip é desativado durante a sessão com o mod; a simulação permanece em 60 Hz. A instalação não edita os saves.\n\nPara desinstalar, feche o jogo e escolha Remover High FPS. Veja o README incluído para informações de segurança e compilação.", "1. 将发布版 ZIP 中的所有文件解压到同一文件夹，并保持这些文件放在一起。\n\n2. 关闭 Terraria。如果未自动找到游戏，请选择 Terraria 文件夹。\n\n3. 点击“安装并开始游戏”。以后可使用启动器中的“开始游戏”或 Terraria.HighFPS.exe。从 Steam 启动 Terraria 仍会打开原版游戏。\n\n请在 Windows 中选择显示器的高刷新率。模组会话期间 Frame Skip 将关闭，游戏逻辑保持 60 Hz。安装过程不会编辑存档。\n\n卸载时，请关闭游戏并选择“移除 High FPS”。随发布版附带的 README 包含安全和构建详情。");
            Add(d, "ok", "Got it", "Понятно", "Verstanden", "Entendido", "Compris", "Entendi", "知道了");
            return d;
        }

        private static void Add(Dictionary<string, string[]> dictionary, string key, params string[] translations)
        {
            dictionary.Add(key, translations);
        }
    }
}

