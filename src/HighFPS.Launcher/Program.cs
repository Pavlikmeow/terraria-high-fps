using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

[assembly: AssemblyTitle("High FPS Support for Terraria")]
[assembly: AssemblyDescription("High refresh rate interpolation launcher for Terraria 1.4.5.8")]
[assembly: AssemblyCompany("pavlikmeow")]
[assembly: AssemblyProduct("High FPS Support")]
[assembly: AssemblyCopyright("Copyright (c) 2026 pavlikmeow")]
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]
[assembly: ComVisible(false)]

namespace TerrariaHighFPS.Launcher
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length > 0) return RunCommandLine(args);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                using (var form = new MainForm()) Application.Run(form);
                return 0;
            }
            catch (Exception ex)
            {
                var text = new Localization(LauncherSettings.LoadLanguage());
                string key = ex is FileNotFoundException || ex is FileLoadException ? "errorPackage" : "errorGeneric";
                MessageBox.Show(text[key] + Environment.NewLine + Environment.NewLine + ex.Message,
                    text["errorTitle"], MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }

        // EN: CLI output is deliberately stable English for scripts; exit 0 = success, 1 = failure, 2 = invalid arguments.
        // RU: CLI использует стабильные английские сообщения: код 0 — успех, 1 — ошибка, 2 — неверные аргументы.
        internal static int RunCommandLine(string[] args)
        {
            try
            {
                if (args.Length == 1 && (args[0] == "--help" || args[0] == "-h" || args[0] == "/?"))
                {
                    Console.WriteLine("High FPS Support 1.1.0 by pavlikmeow");
                    Console.WriteLine("Usage: HighFpsSupport.exe [command] [game-folder]");
                    Console.WriteLine("  (no command)             Open the localized launcher");
                    Console.WriteLine("  --install [game-folder]   Install or repair without launching");
                    Console.WriteLine("  --play [game-folder]      Install or repair, then launch High FPS");
                    Console.WriteLine("  --remove [game-folder]    Remove mod files; preserve the original game");
                    Console.WriteLine("  --diagnose [game-folder]  Print a read-only local installation report");
                    Console.WriteLine("  --verify <patched-exe>    Verify injected hooks in a patched executable");
                    Console.WriteLine("  --version                Print the launcher version");
                    Console.WriteLine("  --help                   Show this help");
                    Console.WriteLine("Quote paths containing spaces. Exit codes: 0 success, 1 failure, 2 invalid arguments.");
                    return 0;
                }
                if (args.Length == 1 && args[0] == "--version")
                {
                    Console.WriteLine("1.1.0");
                    return 0;
                }
                if (args.Length == 2 && args[0] == "--verify")
                {
                    HighFpsPatcher.VerifyPatchedExecutable(args[1]);
                    Console.WriteLine("Patched executable hooks verified.");
                    return 0;
                }
                if (args.Length == 4 && args[0] == "--test-patch")
                {
                    HighFpsPatcher.Create(args[1], args[2], args[3]);
                    return 0;
                }
                if ((args.Length == 1 || args.Length == 2) &&
                    (args[0] == "--install" || args[0] == "--play" || args[0] == "--remove" || args[0] == "--diagnose"))
                {
                    string directory = args.Length == 2 ? args[1] : GameLocator.Find();
                    if (args[0] == "--diagnose") return Diagnose(directory);
                    if (args[0] == "--remove")
                    {
                        LauncherEngine.Remove(directory);
                        Console.WriteLine("High FPS removed. The original game was preserved.");
                    }
                    else
                    {
                        InstallResult result = LauncherEngine.Install(directory);
                        Console.WriteLine(result.Rebuilt ? "High FPS installed." : "High FPS installation is current.");
                        Console.WriteLine("Patched executable SHA-256: " + result.Patch.OutputSha256);
                        if (args[0] == "--play")
                        {
                            LauncherEngine.Launch(directory);
                            Console.WriteLine("High FPS started.");
                        }
                    }
                    return 0;
                }
                Console.Error.WriteLine("Invalid arguments. Use --help for available commands.");
                return 2;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("High FPS operation failed: " + ex.Message);
                return 1;
            }
        }

        private static int Diagnose(string directory)
        {
            Console.WriteLine("High FPS Support 1.1.0 · local diagnostic report");
            Console.WriteLine("Runtime: " + Environment.Version);
            Console.WriteLine("OS: " + Environment.OSVersion);
            Console.WriteLine("Game folder: " + (directory ?? "not detected"));
            Console.WriteLine("Review local paths before sharing this report.");
            if (!GameLocator.IsTerrariaDirectory(directory))
            {
                Console.Error.WriteLine("Terraria was not found. Specify its game folder.");
                return 1;
            }
            string source = Path.Combine(directory, "Terraria.exe");
            Console.WriteLine("Original executable SHA-256: " + HighFpsPatcher.ComputeSha256(source));
            Console.WriteLine("Mod files present: " + LauncherEngine.IsInstalled(directory));
            HighFpsPatcher.ValidateCompatibleGame(source);
            Console.WriteLine("Compatibility: Terraria 1.4.5.8 Windows Steam version.");
            if (LauncherEngine.IsInstalled(directory))
            {
                PatchReport report = LauncherEngine.ValidateInstallation(directory);
                Console.WriteLine("Installed file hashes and injected hooks: verified.");
                Console.WriteLine("Patched executable SHA-256: " + report.OutputSha256);
            }
            else if (File.Exists(Path.Combine(directory, LauncherEngine.PatchedExeName)) ||
                File.Exists(Path.Combine(directory, LauncherEngine.LogicDllName)))
            {
                Console.Error.WriteLine("Installation is incomplete. Use --install to repair it.");
                return 1;
            }
            return 0;
        }
    }
}
