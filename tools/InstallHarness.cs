using System;
using System.IO;
using TerrariaHighFPS.Launcher;

internal static class InstallHarness
{
    private static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: InstallHarness <mock-game-directory with .highfps-test-fixture marker>");
            return 2;
        }

        try
        {
            string gameDirectory = Path.GetFullPath(args[0]);
            // EN: Destructive corruption tests require an explicit disposable-fixture marker.
            // RU: Тесты с повреждением файлов требуют явной метки одноразовой тестовой папки.
            if (!File.Exists(Path.Combine(gameDirectory, ".highfps-test-fixture")))
                throw new InvalidOperationException("Refusing corruption tests without a .highfps-test-fixture marker.");
            string source = Path.Combine(gameDirectory, "Terraria.exe");
            string output = Path.Combine(gameDirectory, LauncherEngine.PatchedExeName);
            string logic = Path.Combine(gameDirectory, LauncherEngine.LogicDllName);
            string metadata = Path.Combine(gameDirectory, LauncherEngine.MetadataName);
            string sourceBefore = HighFpsPatcher.ComputeSha256(source);

            InstallResult first = LauncherEngine.Install(gameDirectory);
            InstallResult second = LauncherEngine.Install(gameDirectory);
            Assert(first.Rebuilt, "First installation should build the patch.");
            Assert(!second.Rebuilt, "Second installation should reuse the verified patch.");
            LauncherEngine.ValidateInstallation(gameDirectory);
            Console.WriteLine("Install and verified reuse: true");

            // EN: Appended bytes leave IL readable, so only checking hook counts would miss this change.
            // RU: Добавленные байты не мешают чтению IL: проверка только числа хуков пропустила бы изменение.
            using (var stream = new FileStream(output, FileMode.Append, FileAccess.Write)) stream.WriteByte(42);
            HighFpsPatcher.VerifyPatchedExecutable(output);
            AssertInvalidInstallation(gameDirectory);
            Assert(LauncherEngine.Install(gameDirectory).Rebuilt, "Modified but readable output must be rebuilt.");
            Console.WriteLine("Output SHA-256 mismatch detected: true");

            File.WriteAllBytes(logic, new byte[] { 1, 2, 3, 4 });
            AssertInvalidInstallation(gameDirectory);
            Assert(LauncherEngine.Install(gameDirectory).Rebuilt, "Modified logic must be repaired.");
            LauncherEngine.ValidateInstallation(gameDirectory);
            Console.WriteLine("Embedded logic integrity enforced: true");

            string originalMetadata = File.ReadAllText(metadata);
            File.WriteAllText(metadata, originalMetadata.Replace("modVersion=", "prefixmodVersion="));
            AssertInvalidInstallation(gameDirectory);
            Assert(LauncherEngine.Install(gameDirectory).Rebuilt, "Metadata keys must match exactly.");
            File.AppendAllText(metadata, "modVersion=" + LauncherEngine.ModVersion + Environment.NewLine);
            AssertInvalidInstallation(gameDirectory);
            Assert(LauncherEngine.Install(gameDirectory).Rebuilt, "Duplicate metadata keys must be rejected.");
            Console.WriteLine("Strict metadata parsing: true");

            // EN: Block the second commit after the DLL changed, then prove the first change rolled back.
            // RU: Блокируем второй шаг после замены DLL и проверяем откат первого шага.
            File.WriteAllBytes(logic, new byte[] { 8, 7, 6, 5 });
            string logicBeforeFailure = HighFpsPatcher.ComputeSha256(logic);
            string outputBeforeFailure = HighFpsPatcher.ComputeSha256(output);
            string metadataBeforeFailure = HighFpsPatcher.ComputeSha256(metadata);
            bool rejected = false;
            using (var lockedOutput = new FileStream(output, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try { LauncherEngine.Install(gameDirectory); }
                catch (LauncherException ex)
                {
                    if (ex.Code != "InstallationFailed") throw;
                    rejected = true;
                }
            }
            Assert(rejected, "Locked output must reject the transaction.");
            Assert(HighFpsPatcher.ComputeSha256(logic) == logicBeforeFailure, "Rollback did not restore prior DLL.");
            Assert(HighFpsPatcher.ComputeSha256(output) == outputBeforeFailure, "Failed transaction changed output.");
            Assert(HighFpsPatcher.ComputeSha256(metadata) == metadataBeforeFailure, "Failed transaction changed metadata.");
            Assert(LauncherEngine.Install(gameDirectory).Rebuilt, "Repair after a failed transaction must work.");
            Console.WriteLine("Partial commit rollback: true");

            // EN: Reject an invalid game before touching existing deployment files.
            // RU: Некорректная игра отклоняется до изменения установленных файлов мода.
            byte[] originalSource = File.ReadAllBytes(source);
            string logicBeforeValidation = HighFpsPatcher.ComputeSha256(logic);
            string outputBeforeValidation = HighFpsPatcher.ComputeSha256(output);
            string metadataBeforeValidation = HighFpsPatcher.ComputeSha256(metadata);
            try
            {
                File.WriteAllBytes(source, new byte[] { 0, 0, 0, 0 });
                rejected = false;
                try { LauncherEngine.Install(gameDirectory); }
                catch (LauncherException ex)
                {
                    if (ex.Code != "InvalidDirectory") throw;
                    rejected = true;
                }
                Assert(rejected, "Invalid game must be rejected.");
                Assert(HighFpsPatcher.ComputeSha256(logic) == logicBeforeValidation, "Validation failure changed DLL.");
                Assert(HighFpsPatcher.ComputeSha256(output) == outputBeforeValidation, "Validation failure changed output.");
                Assert(HighFpsPatcher.ComputeSha256(metadata) == metadataBeforeValidation, "Validation failure changed metadata.");
            }
            finally { File.WriteAllBytes(source, originalSource); }
            Console.WriteLine("Validation before deployment mutation: true");

            Assert(!GameLocator.IsTerrariaDirectory("invalid\0path"), "Invalid path should not escape locator.");
            Assert(sourceBefore == HighFpsPatcher.ComputeSha256(source), "Original Terraria.exe changed.");
            LauncherEngine.ValidateInstallation(gameDirectory);
            Assert(Directory.GetDirectories(gameDirectory, ".HighFPS-staging-*").Length == 0, "Temporary deployment files remain.");
            Console.WriteLine("Original unchanged; staging cleaned: true");
            Console.WriteLine("Patched SHA-256: " + second.Patch.OutputSha256);

            LauncherEngine.Remove(gameDirectory);
            Assert(!LauncherEngine.IsInstalled(gameDirectory), "Uninstall left mod files behind.");
            foreach (string name in new[] { LauncherEngine.PatchedExeName, LauncherEngine.LogicDllName, LauncherEngine.MetadataName, LauncherEngine.LogName })
                Assert(!File.Exists(Path.Combine(gameDirectory, name)), "Uninstall left " + name + ".");
            Assert(File.Exists(source) && Directory.Exists(Path.Combine(gameDirectory, "Content")), "Uninstall removed original game files.");
            Console.WriteLine("Clean uninstall: true");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static void AssertInvalidInstallation(string directory)
    {
        try { LauncherEngine.ValidateInstallation(directory); }
        catch (LauncherException ex)
        {
            if (ex.Code == "InstallationInvalid" || ex.Code == "NotInstalled") return;
            throw;
        }
        throw new Exception("Changed installation incorrectly passed launch verification.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
