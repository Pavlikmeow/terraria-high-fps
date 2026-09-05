using System;
using System.Globalization;
using System.IO;
using System.Security;

namespace TerrariaHighFPS.Launcher
{
    internal static class LauncherSettings
    {
        private static readonly string LanguageFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TerrariaHighFPS", "language.txt");

        // EN: Preferences stay in the current user's profile; no administrator rights are needed.
        // RU: Настройки хранятся в профиле текущего пользователя; права администратора не нужны.
        public static string LoadLanguage()
        {
            try
            {
                if (File.Exists(LanguageFile) && new FileInfo(LanguageFile).Length <= 64)
                {
                    string saved = File.ReadAllText(LanguageFile).Trim();
                    if (Localization.IsSupported(saved)) return saved;
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }
            return Localization.MatchCulture(CultureInfo.CurrentUICulture.Name);
        }

        public static bool SaveLanguage(string code)
        {
            if (!Localization.IsSupported(code)) return false;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LanguageFile));
                File.WriteAllText(LanguageFile, code);
                return true;
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
            catch (SecurityException) { return false; }
        }
    }
}
