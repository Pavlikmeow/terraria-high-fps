using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using TerrariaHighFPS.Launcher;

internal static class PatchHarness
{
    private static int Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Usage: PatchHarness <Terraria.exe> <output.exe> <HighFPS.Support.dll>");
            return 2;
        }

        string altered = null;
        try
        {
            string sourceHash = HighFpsPatcher.ComputeSha256(args[0]);
            string logicHash = HighFpsPatcher.ComputeSha256(args[2]);
            ExpectRejected(delegate { HighFpsPatcher.Create(args[0], args[0], args[2]); }, "Patching source in place");
            ExpectRejected(delegate { HighFpsPatcher.Create(args[0], args[2], args[2]); }, "Replacing input DLL");
            if (HighFpsPatcher.ComputeSha256(args[0]) != sourceHash || HighFpsPatcher.ComputeSha256(args[2]) != logicHash)
                throw new Exception("Rejected alias operation changed an input.");
            Console.WriteLine("Input/output alias protection: true");

            PatchReport report = HighFpsPatcher.Create(args[0], args[1], args[2]);
            HighFpsPatcher.VerifyPatchedExecutable(args[1]);
            if (report.InsertedCalls != 3 || report.SourceSha256 != sourceHash ||
                HighFpsPatcher.ComputeSha256(args[0]) != sourceHash ||
                HighFpsPatcher.ComputeSha256(args[1]) != report.OutputSha256)
                throw new Exception("Patch report does not match generated files.");

            altered = Path.GetFullPath(args[1]) + ".verification-test-" + Guid.NewGuid().ToString("N");
            // EN: Keep the three hook calls but move PostDraw before DoDraw; validation must reject it.
            // RU: Сохраняем три хука, но переносим PostDraw перед DoDraw; проверка обязана отклонить файл.
            using (var assembly = AssemblyDefinition.ReadAssembly(args[1], new ReaderParameters { InMemory = true }))
            {
                var draw = assembly.MainModule.Types.Single(t => t.FullName == "Terraria.Main").Methods
                    .Single(m => m.Name == "Draw" && m.Parameters.Count == 1);
                var postDraw = draw.Body.Instructions.Single(i => i.Operand is MethodReference &&
                    ((MethodReference)i.Operand).DeclaringType.FullName == "TerrariaHighFPS.FpsManager" &&
                    ((MethodReference)i.Operand).Name == "PostDraw");
                ILProcessor il = draw.Body.GetILProcessor();
                il.Remove(postDraw);
                il.InsertBefore(draw.Body.Instructions[0], postDraw);
                assembly.Write(altered);
            }
            ExpectRejected(delegate { HighFpsPatcher.VerifyPatchedExecutable(altered); }, "Moved interpolation hook");
            Console.WriteLine("Hook placement verification: true");
            Console.WriteLine("Terraria version: " + report.TerrariaVersion);
            Console.WriteLine("Source SHA-256:  " + report.SourceSha256);
            Console.WriteLine("Output SHA-256:  " + report.OutputSha256);
            Console.WriteLine("Inserted calls:  " + report.InsertedCalls);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
        finally
        {
            if (altered != null && File.Exists(altered)) File.Delete(altered);
        }
    }

    private static void ExpectRejected(Action action, string scenario)
    {
        try { action(); }
        catch (InvalidOperationException) { return; }
        catch (InvalidDataException) { return; }
        throw new Exception(scenario + " was not rejected.");
    }
}
