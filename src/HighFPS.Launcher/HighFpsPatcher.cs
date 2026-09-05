using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TerrariaHighFPS.Launcher
{
    internal sealed class PatchReport
    {
        public string SourceSha256 { get; set; }
        public string OutputSha256 { get; set; }
        public Version TerrariaVersion { get; set; }
        public int InsertedCalls { get; set; }
    }

    internal static class HighFpsPatcher
    {
        public static readonly Version SupportedTerrariaVersion = new Version(1, 4, 5, 8);
        public const string LogicAssemblyName = "HighFPS.Support";

        public static PatchReport Create(string sourceExe, string outputExe, string logicDll)
        {
            ValidateFile(sourceExe, "Terraria.exe");
            ValidateFile(logicDll, "HighFPS.Support.dll");

            string fullSource = Path.GetFullPath(sourceExe);
            string fullOutput = Path.GetFullPath(outputExe);
            string fullLogic = Path.GetFullPath(logicDll);
            string outputDirectory = Path.GetDirectoryName(fullOutput);

            // EN: Compare file identity too: junctions and hard links may give one file two paths.
            // RU: Сравниваем и идентификатор файла: ссылки могут задавать разные пути к одному файлу.
            if (PathsReferToSameFile(fullSource, fullOutput))
                throw new InvalidOperationException("The original Terraria.exe is never patched in place.");
            if (PathsReferToSameFile(fullLogic, fullOutput))
                throw new InvalidOperationException("The output executable cannot replace the interpolation DLL.");
            Directory.CreateDirectory(outputDirectory);

            string temporaryOutput = Path.Combine(
                outputDirectory,
                Path.GetFileName(fullOutput) + ".building-" + Guid.NewGuid().ToString("N"));

            var resolver = CreateResolver(Path.GetDirectoryName(fullSource));
            try
            {
                using (var sourceLock = new FileStream(fullSource, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var logicLock = new FileStream(fullLogic, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var logicAssembly = AssemblyDefinition.ReadAssembly(fullLogic, new ReaderParameters
                {
                    AssemblyResolver = resolver,
                    InMemory = true
                }))
                using (var gameAssembly = AssemblyDefinition.ReadAssembly(fullSource, new ReaderParameters
                {
                    AssemblyResolver = resolver,
                    InMemory = true
                }))
                {
                    ValidateGameAssembly(gameAssembly);
                    ValidateLogicAssembly(logicAssembly);
                    string sourceHash = ComputeSha256(fullSource);
                    int insertedCalls = ApplyPatch(gameAssembly, logicAssembly);
                    gameAssembly.Write(temporaryOutput);

                    VerifyPatchedAssembly(temporaryOutput, resolver);
                    ReplaceOutput(temporaryOutput, fullOutput);

                    return new PatchReport
                    {
                        SourceSha256 = sourceHash,
                        OutputSha256 = ComputeSha256(fullOutput),
                        TerrariaVersion = gameAssembly.Name.Version,
                        InsertedCalls = insertedCalls
                    };
                }
            }
            finally
            {
                resolver.Dispose();
                TryDelete(temporaryOutput);
            }
        }

        public static Version ValidateCompatibleGame(string sourceExe)
        {
            ValidateFile(sourceExe, "Terraria.exe");
            var resolver = CreateResolver(Path.GetDirectoryName(Path.GetFullPath(sourceExe)));
            try
            {
                using (var gameAssembly = AssemblyDefinition.ReadAssembly(sourceExe, new ReaderParameters
                {
                    AssemblyResolver = resolver,
                    InMemory = true
                }))
                {
                    ValidateGameAssembly(gameAssembly);
                    return gameAssembly.Name.Version;
                }
            }
            finally
            {
                resolver.Dispose();
            }
        }

        public static void VerifyPatchedExecutable(string patchedExe)
        {
            ValidateFile(patchedExe, "Terraria.HighFPS.exe");
            var resolver = CreateResolver(Path.GetDirectoryName(Path.GetFullPath(patchedExe)));
            try
            {
                VerifyPatchedAssembly(patchedExe, resolver);
            }
            finally
            {
                resolver.Dispose();
            }
        }

        public static string ComputeSha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "");
        }

        public static string ComputeSha256(byte[] bytes)
        {
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "");
        }

        private static int ApplyPatch(AssemblyDefinition gameAssembly, AssemblyDefinition logicAssembly)
        {
            ModuleDefinition gameModule = gameAssembly.MainModule;
            TypeDefinition mainType = RequireSingle(
                gameModule.Types.Where(t => t.FullName == "Terraria.Main"),
                "Terraria.Main type");

            TypeDefinition managerType = RequireSingle(
                logicAssembly.MainModule.Types.Where(t => t.FullName == "TerrariaHighFPS.FpsManager"),
                "TerrariaHighFPS.FpsManager type");

            MethodDefinition update = RequireMethod(mainType, "Update", 1);
            MethodDefinition draw = RequireMethod(mainType, "Draw", 1);
            MethodDefinition doUpdate = RequireMethod(mainType, "DoUpdate", 1);
            MethodDefinition doDraw = RequireMethod(mainType, "DoDraw", 1);

            if (FindLogicCall(update) != null || FindLogicCall(draw) != null ||
                gameModule.AssemblyReferences.Any(r => r.Name == LogicAssemblyName))
                throw new InvalidOperationException("Input already contains a High FPS Support patch.");

            MethodReference beforeUpdate = gameModule.ImportReference(RequireMethod(managerType, "BeforeUpdate", 1));
            MethodReference preDraw = gameModule.ImportReference(RequireMethod(managerType, "PreDraw", 0));
            MethodReference postDraw = gameModule.ImportReference(RequireMethod(managerType, "PostDraw", 0));

            Instruction doUpdateCall = FindSingleCall(update, doUpdate);
            Instruction updateArgument = doUpdateCall.Previous;
            Instruction updateInstance = updateArgument == null ? null : updateArgument.Previous;
            if (updateArgument == null || updateInstance == null ||
                updateInstance.OpCode != OpCodes.Ldarg_0 ||
                (updateArgument.OpCode != OpCodes.Ldarga && updateArgument.OpCode != OpCodes.Ldarga_S))
                throw new InvalidOperationException("Unexpected Main.Update IL near DoUpdate; refusing an unsafe patch.");

            ILProcessor updateIl = update.Body.GetILProcessor();
            // EN: Only three calls are added; the original fixed-step simulation stays intact.
            // RU: Добавляются только три вызова; исходная симуляция с фиксированным шагом сохраняется.
            updateIl.InsertBefore(updateInstance, updateIl.Create(OpCodes.Ldarg_1));
            updateIl.InsertBefore(updateInstance, updateIl.Create(OpCodes.Call, beforeUpdate));

            Instruction doDrawCall = FindSingleCall(draw, doDraw);
            Instruction drawGameTime = doDrawCall.Previous;
            Instruction drawInstance = drawGameTime == null ? null : drawGameTime.Previous;
            if (drawGameTime == null || drawInstance == null ||
                drawInstance.OpCode != OpCodes.Ldarg_0 || drawGameTime.OpCode != OpCodes.Ldarg_1)
                throw new InvalidOperationException("Unexpected Main.Draw IL near DoDraw; refusing an unsafe patch.");

            ILProcessor drawIl = draw.Body.GetILProcessor();
            drawIl.InsertBefore(drawInstance, drawIl.Create(OpCodes.Call, preDraw));
            drawIl.InsertAfter(doDrawCall, drawIl.Create(OpCodes.Call, postDraw));

            return 3;
        }

        private static void ValidateGameAssembly(AssemblyDefinition assembly)
        {
            if (!string.Equals(assembly.Name.Name, "Terraria", StringComparison.Ordinal))
                throw new InvalidOperationException("Selected file is not Terraria.exe.");
            if (assembly.Name.Version != SupportedTerrariaVersion)
                throw new NotSupportedException(
                    "This build supports Terraria " + SupportedTerrariaVersion +
                    ", but the selected executable is " + assembly.Name.Version + ".");
            if (assembly.MainModule.Kind != ModuleKind.Windows)
                throw new NotSupportedException("Only the Windows Steam build is supported.");
            if (assembly.MainModule.Architecture != TargetArchitecture.I386)
                throw new NotSupportedException("Unexpected Terraria executable architecture.");

            TypeDefinition mainType = RequireSingle(
                assembly.MainModule.Types.Where(t => t.FullName == "Terraria.Main"),
                "Terraria.Main type");
            MethodDefinition update = RequireMethod(mainType, "Update", 1);
            MethodDefinition draw = RequireMethod(mainType, "Draw", 1);
            MethodDefinition doUpdate = RequireMethod(mainType, "DoUpdate", 1);
            MethodDefinition doDraw = RequireMethod(mainType, "DoDraw", 1);

            ValidateGameMethod(update, "Microsoft.Xna.Framework.GameTime");
            ValidateGameMethod(draw, "Microsoft.Xna.Framework.GameTime");
            ValidateGameMethod(doUpdate, "Microsoft.Xna.Framework.GameTime&");
            ValidateGameMethod(doDraw, "Microsoft.Xna.Framework.GameTime");

            FindSingleCall(update, doUpdate);
            FindSingleCall(draw, doDraw);

            string[] requiredFields =
            {
                "UpdateTimeAccumulator", "FrameSkipMode", "screenPosition",
                "player", "npc", "projectile", "item"
            };
            foreach (string fieldName in requiredFields)
                RequireSingle(mainType.Fields.Where(f => f.Name == fieldName), "Terraria.Main." + fieldName + " field");
        }

        private static void ValidateLogicAssembly(AssemblyDefinition assembly)
        {
            if (!string.Equals(assembly.Name.Name, LogicAssemblyName, StringComparison.Ordinal))
                throw new InvalidOperationException("Unexpected interpolation DLL assembly name.");
            if (!assembly.MainModule.AssemblyReferences.Any(r =>
                r.Name == "Terraria" && r.Version == SupportedTerrariaVersion))
                throw new InvalidOperationException("Interpolation DLL was not built against Terraria 1.4.5.8.");

            TypeDefinition managerType = RequireSingle(
                assembly.MainModule.Types.Where(t => t.FullName == "TerrariaHighFPS.FpsManager"),
                "TerrariaHighFPS.FpsManager type");
            foreach (string name in new[] { "BeforeUpdate", "PreDraw", "PostDraw" })
            {
                MethodDefinition method = RequireMethod(managerType, name, name == "BeforeUpdate" ? 1 : 0);
                if (!method.IsPublic || !method.IsStatic || method.ReturnType.FullName != "System.Void" ||
                    (method.Parameters.Count == 1 && method.Parameters[0].ParameterType.FullName != "Microsoft.Xna.Framework.GameTime"))
                    throw new InvalidOperationException("Unexpected interpolation hook signature: " + name + ".");
            }
        }

        private static void ValidateGameMethod(MethodDefinition method, string parameterType)
        {
            if (method.IsStatic || method.ReturnType.FullName != "System.Void" ||
                method.Parameters[0].ParameterType.FullName != parameterType)
                throw new NotSupportedException("Unexpected game method signature: " + method.FullName + ".");
        }

        private static void VerifyPatchedAssembly(string path, IAssemblyResolver resolver)
        {
            using (var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters
            {
                AssemblyResolver = resolver,
                InMemory = true
            }))
            {
                ValidateGameAssembly(assembly);
                if (assembly.MainModule.AssemblyReferences.Count(r => r.Name == LogicAssemblyName) != 1)
                    throw new InvalidDataException("Patched executable must reference one interpolation assembly.");

                TypeDefinition mainType = RequireSingle(
                    assembly.MainModule.Types.Where(t => t.FullName == "Terraria.Main"),
                    "Terraria.Main type");

                MethodDefinition update = RequireMethod(mainType, "Update", 1);
                MethodDefinition draw = RequireMethod(mainType, "Draw", 1);

                int beforeUpdateCalls = CountLogicCalls(update, "BeforeUpdate");
                int preDrawCalls = CountLogicCalls(draw, "PreDraw");
                int postDrawCalls = CountLogicCalls(draw, "PostDraw");
                if (beforeUpdateCalls != 1 || preDrawCalls != 1 || postDrawCalls != 1)
                    throw new InvalidDataException(
                        "Patch verification failed: expected exactly one call at each hook point.");

                // EN: Counts alone cannot prove the calls surround the intended game operations.
                // RU: Одного числа вызовов недостаточно: проверяем их положение относительно методов игры.
                Instruction updateCall = FindSingleCall(update, RequireMethod(mainType, "DoUpdate", 1));
                Instruction drawCall = FindSingleCall(draw, RequireMethod(mainType, "DoDraw", 1));
                Instruction updateArgument = updateCall.Previous;
                Instruction updateInstance = updateArgument == null ? null : updateArgument.Previous;
                Instruction beforeUpdate = updateInstance == null ? null : updateInstance.Previous;
                Instruction drawArgument = drawCall.Previous;
                Instruction drawInstance = drawArgument == null ? null : drawArgument.Previous;
                if (updateArgument == null || updateInstance == null || updateInstance.OpCode != OpCodes.Ldarg_0 ||
                    (updateArgument.OpCode != OpCodes.Ldarga && updateArgument.OpCode != OpCodes.Ldarga_S) ||
                    !IsLogicCall(beforeUpdate, "BeforeUpdate") || beforeUpdate.Previous == null ||
                    beforeUpdate.Previous.OpCode != OpCodes.Ldarg_1 ||
                    drawArgument == null || drawArgument.OpCode != OpCodes.Ldarg_1 ||
                    drawInstance == null || drawInstance.OpCode != OpCodes.Ldarg_0 ||
                    !IsLogicCall(drawInstance.Previous, "PreDraw") || !IsLogicCall(drawCall.Next, "PostDraw"))
                    throw new InvalidDataException("Patch verification failed: unexpected hook placement.");
            }
        }

        private static int CountLogicCalls(MethodDefinition method, string methodName)
        {
            return method.Body.Instructions.Count(i => IsLogicCall(i, methodName));
        }

        private static bool IsLogicCall(Instruction instruction, string methodName)
        {
            if (instruction == null || instruction.OpCode != OpCodes.Call) return false;
            var reference = instruction.Operand as MethodReference;
            if (reference == null) return false;
            var scope = reference.DeclaringType.Scope as AssemblyNameReference;
            return reference.Name == methodName && reference.DeclaringType.FullName == "TerrariaHighFPS.FpsManager" &&
                scope != null && scope.Name == LogicAssemblyName && !reference.HasThis &&
                reference.ReturnType.FullName == "System.Void" &&
                (methodName == "BeforeUpdate" ? reference.Parameters.Count == 1 &&
                    reference.Parameters[0].ParameterType.FullName == "Microsoft.Xna.Framework.GameTime" : reference.Parameters.Count == 0);
        }

        private static MethodReference FindLogicCall(MethodDefinition method)
        {
            foreach (Instruction instruction in method.Body.Instructions)
            {
                var reference = instruction.Operand as MethodReference;
                if (reference != null && reference.DeclaringType.FullName == "TerrariaHighFPS.FpsManager")
                    return reference;
            }
            return null;
        }

        private static Instruction FindSingleCall(MethodDefinition owner, MethodDefinition target)
        {
            var calls = owner.Body.Instructions.Where(i =>
            {
                var reference = i.Operand as MethodReference;
                return (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) &&
                    reference != null && reference.FullName == target.FullName;
            }).ToList();

            if (calls.Count != 1)
                throw new InvalidOperationException(
                    "Expected one " + target.Name + " call in " + owner.Name + ", found " + calls.Count + ".");
            return calls[0];
        }

        private static MethodDefinition RequireMethod(TypeDefinition type, string name, int parameterCount)
        {
            return RequireSingle(
                type.Methods.Where(m => m.Name == name && m.Parameters.Count == parameterCount && m.HasBody),
                type.FullName + "." + name + " method");
        }

        private static T RequireSingle<T>(IEnumerable<T> candidates, string description)
        {
            var list = candidates.ToList();
            if (list.Count != 1)
                throw new InvalidOperationException("Expected exactly one " + description + ", found " + list.Count + ".");
            return list[0];
        }

        private static DefaultAssemblyResolver CreateResolver(string gameDirectory)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(gameDirectory);

            string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string gac32 = Path.Combine(windows, "Microsoft.NET", "assembly", "GAC_32");
            AddXnaSearchDirectory(resolver, gac32, "Microsoft.Xna.Framework");
            AddXnaSearchDirectory(resolver, gac32, "Microsoft.Xna.Framework.Game");
            AddXnaSearchDirectory(resolver, gac32, "Microsoft.Xna.Framework.Graphics");
            return resolver;
        }

        private static void AddXnaSearchDirectory(DefaultAssemblyResolver resolver, string gac32, string assemblyName)
        {
            string root = Path.Combine(gac32, assemblyName);
            if (!Directory.Exists(root)) return;
            foreach (string directory in Directory.GetDirectories(root))
                resolver.AddSearchDirectory(directory);
        }

        private static void ReplaceOutput(string temporaryOutput, string output)
        {
            if (File.Exists(output))
            {
                string old = output + ".old-" + Guid.NewGuid().ToString("N");
                File.Replace(temporaryOutput, output, old, true);
                TryDelete(old);
            }
            else
            {
                File.Move(temporaryOutput, output);
            }
        }

        private static void ValidateFile(string path, string displayName)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                throw new FileNotFoundException(displayName + " was not found.", path);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FileIdentity
        {
            public uint Attributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle handle, out FileIdentity information);

        private static bool PathsReferToSameFile(string first, string second)
        {
            if (string.Equals(first, second, StringComparison.OrdinalIgnoreCase)) return true;
            if (!File.Exists(second)) return false;
            using (var firstStream = File.OpenRead(first))
            using (var secondStream = File.OpenRead(second))
            {
                FileIdentity firstInfo, secondInfo;
                if (!GetFileInformationByHandle(firstStream.SafeFileHandle, out firstInfo) ||
                    !GetFileInformationByHandle(secondStream.SafeFileHandle, out secondInfo))
                    throw new IOException("Could not check whether input and output refer to the same file.",
                        new Win32Exception(Marshal.GetLastWin32Error()));
                return firstInfo.VolumeSerialNumber == secondInfo.VolumeSerialNumber &&
                    firstInfo.FileIndexHigh == secondInfo.FileIndexHigh && firstInfo.FileIndexLow == secondInfo.FileIndexLow;
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path);
            }
            catch
            {
            }
        }
    }
}
