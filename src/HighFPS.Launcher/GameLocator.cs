using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace TerrariaHighFPS.Launcher
{
    internal static class GameLocator
    {
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TerrariaHighFPS");

        private static readonly string SettingsFile = Path.Combine(SettingsDirectory, "game-path.txt");

        public static string Find()
        {
            var candidates = new List<string>();
            // EN: An explicit saved choice wins over automatic discovery of another installation.
            // RU: Сохранённый выбор пользователя важнее автоматически найденной другой установки.
            AddCandidate(candidates, LoadSavedPath());
            AddCandidate(candidates, AppDomain.CurrentDomain.BaseDirectory);

            foreach (string steamRoot in FindSteamRoots())
            {
                AddCandidate(candidates, Path.Combine(steamRoot, "steamapps", "common", "Terraria"));
                foreach (string library in ReadSteamLibraries(steamRoot))
                    AddCandidate(candidates, Path.Combine(library, "steamapps", "common", "Terraria"));
            }

            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            AddCandidate(candidates, Path.Combine(programFilesX86, "Steam", "steamapps", "common", "Terraria"));

            foreach (string candidate in candidates)
            {
                if (IsTerrariaDirectory(candidate))
                    return candidate;
            }
            return null;
        }

        public static bool IsTerrariaDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) return false;
            try
            {
                string exe = Path.Combine(directory, "Terraria.exe");
                if (!File.Exists(exe) || !Directory.Exists(Path.Combine(directory, "Content"))) return false;
                FileVersionInfo info = FileVersionInfo.GetVersionInfo(exe);
                return string.Equals(info.OriginalFilename, "Terraria.exe", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(info.ProductName, "Terraria", StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException) { return false; }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
            catch (System.ComponentModel.Win32Exception) { return false; }
            catch (NotSupportedException) { return false; }
            catch (SecurityException) { return false; }
        }

        public static void Save(string directory)
        {
            if (!IsTerrariaDirectory(directory))
                throw new ArgumentException("The selected directory does not contain Terraria.exe and Content.", "directory");
            Directory.CreateDirectory(SettingsDirectory);
            File.WriteAllText(SettingsFile, Path.GetFullPath(directory));
        }

        private static string LoadSavedPath()
        {
            try
            {
                return File.Exists(SettingsFile) && new FileInfo(SettingsFile).Length <= 32768
                    ? File.ReadAllText(SettingsFile).Trim() : null;
            }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
            catch (SecurityException) { return null; }
        }

        private static IEnumerable<string> FindSteamRoots()
        {
            var roots = new List<string>();
            AddRegistrySteamRoot(roots, Registry.CurrentUser, @"Software\Valve\Steam", "SteamPath");
            AddRegistrySteamRoot(roots, Registry.LocalMachine, @"SOFTWARE\Valve\Steam", "InstallPath");
            AddRegistrySteamRoot(roots, Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath");
            return roots;
        }

        private static void AddRegistrySteamRoot(List<string> roots, RegistryKey hive, string keyPath, string valueName)
        {
            try
            {
                using (RegistryKey key = hive.OpenSubKey(keyPath))
                {
                    if (key == null) return;
                    string value = key.GetValue(valueName) as string;
                    AddCandidate(roots, value);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }
        }

        private static IEnumerable<string> ReadSteamLibraries(string steamRoot)
        {
            var result = new List<string>();
            string file = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(file)) return result;

            try
            {
                // EN: Library discovery is optional and bounded; a broken Steam file must not break browsing.
                // RU: Поиск библиотек необязателен и ограничен; повреждённый файл Steam не мешает выбрать папку.
                if (new FileInfo(file).Length > 1024 * 1024) return result;
                string text = File.ReadAllText(file);
                MatchCollection matches = Regex.Matches(
                    text,
                    "\\\"path\\\"\\s+\\\"(?<path>[^\\\"]+)\\\"",
                    RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    string value = match.Groups["path"].Value.Replace("\\\\", "\\");
                    AddCandidate(result, value);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }
            return result;
        }

        private static void AddCandidate(List<string> paths, string candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate)) return;
            try
            {
                string normalized = Path.GetFullPath(candidate.Trim());
                foreach (string existing in paths)
                    if (string.Equals(existing, normalized, StringComparison.OrdinalIgnoreCase)) return;
                paths.Add(normalized);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (NotSupportedException) { }
            catch (SecurityException) { }
        }
    }
}
