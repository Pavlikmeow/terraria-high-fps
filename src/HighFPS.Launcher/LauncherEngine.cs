using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace TerrariaHighFPS.Launcher
{
    internal sealed class InstallResult
    {
        public bool Rebuilt { get; set; }
        public PatchReport Patch { get; set; }
        public string PatchedExecutable { get; set; }
    }

    // EN: Stable error codes keep presentation and translation out of the installer.
    // RU: Постоянные коды ошибок отделяют интерфейс и перевод от установщика.
    internal class LauncherException : InvalidOperationException
    {
        public string Code { get; private set; }

        public LauncherException(string code, string message) : base(message) { Code = code; }
        public LauncherException(string code, string message, Exception inner) : base(message, inner) { Code = code; }
    }

    internal static class LauncherEngine
    {
        public const string PatchedExeName = "Terraria.HighFPS.exe";
        public const string LogicDllName = "HighFPS.Support.dll";
        public const string MetadataName = "HighFPS.Support.install.txt";
        public const string LogName = "HighFPS.Support.log";
        public const string ModVersion = "1.1.0";
        private const string MetadataHeader = "High FPS Support installation metadata";
        private static readonly string[] DeploymentNames = { LogicDllName, PatchedExeName, MetadataName };

        public static InstallResult Install(string gameDirectory)
        {
            gameDirectory = ValidateDirectory(gameDirectory);
            using (AcquireOperation(gameDirectory))
            {
                EnsureGameIsClosed();
                string source = Path.Combine(gameDirectory, "Terraria.exe");
                string output = Path.Combine(gameDirectory, PatchedExeName);
                byte[] logicBytes = ReadEmbeddedLogic();
                string logicHash = HighFpsPatcher.ComputeSha256(logicBytes);

                // EN: Keep the original read-locked through validation and commit; never write to it.
                // RU: Исходник открыт только для чтения до конца проверки и установки; запись в него запрещена.
                using (var sourceLock = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    HighFpsPatcher.ValidateCompatibleGame(source);
                    string sourceHash = HighFpsPatcher.ComputeSha256(source);
                    PatchReport existing;
                    if (TryVerifyInstallation(gameDirectory, sourceHash, logicHash, out existing))
                        return new InstallResult { Rebuilt = false, Patch = existing, PatchedExecutable = output };

                    string staging = Path.Combine(gameDirectory, ".HighFPS-staging-" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(staging);
                    bool preserveRecovery = false;
                    try
                    {
                        string stagedLogic = Path.Combine(staging, LogicDllName);
                        File.WriteAllBytes(stagedLogic, logicBytes);
                        PatchReport report = HighFpsPatcher.Create(source, Path.Combine(staging, PatchedExeName), stagedLogic);
                        WriteMetadata(Path.Combine(staging, MetadataName), report, logicHash);
                        EnsureGameIsClosed();

                        // EN: Validate all new files first, then commit with per-file backups and rollback.
                        // RU: Сначала проверяем все новые файлы; затем заменяем их с резервированием и откатом.
                        try { CommitInstallation(gameDirectory, staging); }
                        catch (RecoveryRequiredException) { preserveRecovery = true; throw; }
                        return new InstallResult { Rebuilt = true, Patch = report, PatchedExecutable = output };
                    }
                    finally
                    {
                        if (!preserveRecovery) CleanupStaging(staging);
                    }
                }
            }
        }

        public static void Launch(string gameDirectory)
        {
            gameDirectory = ValidateDirectory(gameDirectory);
            using (AcquireOperation(gameDirectory))
            {
                EnsureGameIsClosed();
                ValidateInstallation(gameDirectory);
                using (Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine(gameDirectory, PatchedExeName),
                    WorkingDirectory = gameDirectory,
                    UseShellExecute = false
                }))
                {
                    if (process == null) throw new LauncherException("InstallationInvalid", "Windows could not start Terraria.");
                }
            }
        }

        // EN: This is also used by diagnostics; verification does not run any game or DLL code.
        // RU: Этот метод используется и в диагностике; проверка не запускает код игры или DLL.
        public static PatchReport ValidateInstallation(string gameDirectory)
        {
            gameDirectory = ValidateDirectory(gameDirectory);
            if (!IsInstalled(gameDirectory))
                throw new LauncherException("NotInstalled", "High FPS Support is not fully installed. Install it first.");
            string source = Path.Combine(gameDirectory, "Terraria.exe");
            HighFpsPatcher.ValidateCompatibleGame(source);
            PatchReport report;
            if (!TryVerifyInstallation(gameDirectory, HighFpsPatcher.ComputeSha256(source),
                HighFpsPatcher.ComputeSha256(ReadEmbeddedLogic()), out report))
                throw new LauncherException("InstallationInvalid", "Installation verification failed. Install again to repair the files.");
            return report;
        }

        public static void Remove(string gameDirectory)
        {
            gameDirectory = ValidateDirectory(gameDirectory);
            using (AcquireOperation(gameDirectory))
            {
                EnsureGameIsClosed();
                foreach (string name in DeploymentNames) DeleteIfExists(Path.Combine(gameDirectory, name));
                DeleteIfExists(Path.Combine(gameDirectory, LogName));
            }
        }

        // EN: Fast presence check for the UI; install and launch perform full integrity checks.
        // RU: Быстрая проверка наличия для интерфейса; установка и запуск полностью проверяют целостность.
        public static bool IsInstalled(string gameDirectory)
        {
            if (!GameLocator.IsTerrariaDirectory(gameDirectory)) return false;
            foreach (string name in DeploymentNames)
                if (!File.Exists(Path.Combine(gameDirectory, name))) return false;
            return true;
        }

        private static string ValidateDirectory(string gameDirectory)
        {
            if (!GameLocator.IsTerrariaDirectory(gameDirectory))
                throw new LauncherException("InvalidDirectory", "Select the folder that contains Terraria.exe and Content.");
            return Path.GetFullPath(gameDirectory);
        }

        private static byte[] ReadEmbeddedLogic()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(LogicDllName))
            {
                if (stream == null)
                    throw new LauncherException("EmbeddedLogicMissing", "Embedded High FPS logic was not found.");
                using (var buffer = new MemoryStream())
                {
                    stream.CopyTo(buffer);
                    return buffer.ToArray();
                }
            }
        }

        private static bool TryVerifyInstallation(string directory, string sourceHash, string logicHash, out PatchReport report)
        {
            report = null;
            try
            {
                Dictionary<string, string> values = ReadMetadata(Path.Combine(directory, MetadataName));
                if (values == null || values["modVersion"] != ModVersion ||
                    values["terrariaVersion"] != HighFpsPatcher.SupportedTerrariaVersion.ToString() ||
                    values["insertedCalls"] != "3" || !HashEquals(values["sourceSha256"], sourceHash) ||
                    !HashEquals(values["logicSha256"], logicHash)) return false;
                string output = Path.Combine(directory, PatchedExeName);
                if (!HashEquals(HighFpsPatcher.ComputeSha256(Path.Combine(directory, LogicDllName)), logicHash) ||
                    !HashEquals(HighFpsPatcher.ComputeSha256(output), values["outputSha256"])) return false;
                HighFpsPatcher.VerifyPatchedExecutable(output);
                report = new PatchReport
                {
                    SourceSha256 = sourceHash,
                    OutputSha256 = values["outputSha256"],
                    TerrariaVersion = HighFpsPatcher.SupportedTerrariaVersion,
                    InsertedCalls = 3
                };
                return true;
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
            catch (BadImageFormatException) { return false; }
            catch (InvalidOperationException) { return false; }
            catch (ArgumentException) { return false; }
        }

        // EN: Metadata is a local corruption check, not a signature or proof of publisher identity.
        // RU: Метаданные проверяют локальную целостность, но не являются подписью или подтверждением авторства.
        private static Dictionary<string, string> ReadMetadata(string path)
        {
            if (!File.Exists(path) || new FileInfo(path).Length > 4096) return null;
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length != 9 || lines[0] != MetadataHeader) return null;
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            string[] keys = { "modVersion", "terrariaVersion", "sourceSha256", "logicSha256", "outputSha256", "insertedCalls", "createdUtc", "formatVersion" };
            for (int i = 1; i < lines.Length; i++)
            {
                int separator = lines[i].IndexOf('=');
                if (separator <= 0) return null;
                string key = lines[i].Substring(0, separator);
                if (Array.IndexOf(keys, key) < 0 || values.ContainsKey(key)) return null;
                values.Add(key, lines[i].Substring(separator + 1));
            }
            DateTime created;
            if (values["formatVersion"] != "1" ||
                !DateTime.TryParseExact(values["createdUtc"], "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out created) ||
                created.Kind != DateTimeKind.Utc) return null;
            foreach (string key in new[] { "sourceSha256", "logicSha256", "outputSha256" })
                if (!IsSha256(values[key])) return null;
            return values;
        }

        private static bool IsSha256(string value)
        {
            if (value.Length != 64) return false;
            foreach (char c in value)
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))) return false;
            return true;
        }

        private static bool HashEquals(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static void WriteMetadata(string path, PatchReport report, string logicHash)
        {
            var text = new StringBuilder();
            text.AppendLine(MetadataHeader);
            text.AppendLine("formatVersion=1");
            text.AppendLine("modVersion=" + ModVersion);
            text.AppendLine("terrariaVersion=" + report.TerrariaVersion);
            text.AppendLine("sourceSha256=" + report.SourceSha256);
            text.AppendLine("logicSha256=" + logicHash);
            text.AppendLine("outputSha256=" + report.OutputSha256);
            text.AppendLine("insertedCalls=" + report.InsertedCalls);
            text.AppendLine("createdUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            File.WriteAllText(path, text.ToString(), new UTF8Encoding(false));
        }

        private sealed class RecoveryRequiredException : LauncherException
        {
            public RecoveryRequiredException(string message, Exception inner) : base("RecoveryRequired", message, inner) { }
        }

        private static void CommitInstallation(string directory, string staging)
        {
            var attempted = new List<string>();
            var originallyPresent = new HashSet<string>(StringComparer.Ordinal);
            try
            {
                foreach (string name in DeploymentNames)
                {
                    string target = Path.Combine(directory, name);
                    string staged = Path.Combine(staging, name);
                    bool exists = File.Exists(target);
                    if (exists) originallyPresent.Add(name);
                    attempted.Add(name);
                    if (exists) File.Replace(staged, target, Path.Combine(staging, name + ".previous"), true);
                    else File.Move(staged, target);
                }
            }
            catch (Exception installError)
            {
                var errors = new List<Exception> { installError };
                for (int i = attempted.Count - 1; i >= 0; i--)
                {
                    string target = Path.Combine(directory, attempted[i]);
                    string previous = Path.Combine(staging, attempted[i] + ".previous");
                    try
                    {
                        if (File.Exists(previous))
                        {
                            if (File.Exists(target)) File.Replace(previous, target, null, true);
                            else File.Move(previous, target);
                        }
                        else if (!originallyPresent.Contains(attempted[i]) && !File.Exists(Path.Combine(staging, attempted[i])))
                            DeleteIfExists(target);
                    }
                    catch (Exception rollbackError) { errors.Add(rollbackError); }
                }
                if (errors.Count > 1)
                    throw new RecoveryRequiredException("Installation rollback was incomplete. Recovery files are preserved at " + staging,
                        new AggregateException(errors));
                throw new LauncherException("InstallationFailed", "Installation failed; previous mod files were restored.", installError);
            }
        }

        private static void CleanupStaging(string staging)
        {
            // EN: Delete only our explicit staging files; preserve anything unexpected for inspection.
            // RU: Удаляем только собственные временные файлы; всё неожиданное оставляем для проверки.
            try
            {
                foreach (string name in DeploymentNames)
                {
                    DeleteIfExists(Path.Combine(staging, name));
                    DeleteIfExists(Path.Combine(staging, name + ".previous"));
                }
                Directory.Delete(staging, false);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        private sealed class OperationLock : IDisposable
        {
            private readonly Mutex mutex;
            public OperationLock(string directory)
            {
                string normalized = directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant();
                mutex = new Mutex(false, @"Local\TerrariaHighFPS-" + HighFpsPatcher.ComputeSha256(Encoding.UTF8.GetBytes(normalized)));
                bool acquired;
                try { acquired = mutex.WaitOne(0); }
                catch (AbandonedMutexException) { acquired = true; }
                if (!acquired)
                {
                    mutex.Dispose();
                    throw new LauncherException("OperationInProgress", "Another High FPS Support operation is already running for this folder.");
                }
            }
            public void Dispose() { mutex.ReleaseMutex(); mutex.Dispose(); }
        }

        private static IDisposable AcquireOperation(string directory) { return new OperationLock(directory); }

        private static void EnsureGameIsClosed()
        {
            foreach (string name in new[] { "Terraria", "Terraria.HighFPS" })
            {
                Process[] processes = Process.GetProcessesByName(name);
                bool running = processes.Length != 0;
                foreach (Process process in processes) process.Dispose();
                if (running) throw new LauncherException("GameRunning", "Close Terraria before installing, launching, or removing High FPS Support.");
            }
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
