using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class Inspector
{
    private static int Main(string[] args)
    {
        if ((args.Length != 1 && args.Length != 2) || !File.Exists(args[0]))
        {
            Console.Error.WriteLine("Usage: Inspector <Terraria.exe> [method-name|method=Type::Name|find=text]");
            return 2;
        }

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(args[0])));

        using (var assembly = AssemblyDefinition.ReadAssembly(args[0], new ReaderParameters
        {
            AssemblyResolver = resolver,
            InMemory = true
        }))
        {
            if (args.Length == 2)
            {
                var selectedType = assembly.MainModule.GetType("Terraria.Main");
                if (args[1] == "timing")
                {
                    foreach (var selectedMethod in selectedType.Methods.Where(m => m.HasBody))
                    {
                        foreach (var instruction in selectedMethod.Body.Instructions)
                        {
                            var value = instruction.Operand == null ? "" : instruction.Operand.ToString();
                            if (value.IndexOf("IsFixedTimeStep", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                value.IndexOf("TargetElapsedTime", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                value.IndexOf("UpdateTimeAccumulator", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                value.IndexOf("TARGET_FRAME_TIME", StringComparison.OrdinalIgnoreCase) >= 0)
                                Console.WriteLine(selectedMethod.FullName + " | IL_" + instruction.Offset.ToString("X4") + ": " + instruction.OpCode + " " + value);
                        }
                    }
                    return 0;
                }
                if (args[1] == "accumulator")
                {
                    var selectedMethod = selectedType.Methods.First(m => m.Name == "DoUpdate" && m.HasBody);
                    DumpMethodRange(selectedMethod, 0x480, 0x590);
                    return 0;
                }
                if (args[1] == "resources")
                {
                    foreach (var resource in assembly.MainModule.Resources)
                        Console.WriteLine(resource.ResourceType + " " + resource.Name + " " + (resource is EmbeddedResource ? ((EmbeddedResource)resource).GetResourceData().Length.ToString() : ""));
                    return 0;
                }
                if (args[1].StartsWith("type="))
                {
                    string typeName = args[1].Substring(5);
                    TypeDefinition selected = assembly.MainModule.GetType(typeName);
                    if (selected == null) throw new InvalidOperationException("Type not found: " + typeName);
                    foreach (var field in selected.Fields) Console.WriteLine("FIELD " + field.FullName + " attrs=" + field.Attributes);
                    foreach (var property in selected.Properties) Console.WriteLine("PROPERTY " + property.FullName + " get=" + (property.GetMethod != null) + " set=" + (property.SetMethod != null));
                    foreach (var method in selected.Methods.Where(m => m.IsConstructor)) Console.WriteLine("CTOR " + method.FullName);
                    return 0;
                }
                if (args[1] == "paths")
                {
                    var cctor = selectedType.Methods.Single(m => m.IsConstructor && m.IsStatic);
                    foreach (var instruction in cctor.Body.Instructions)
                    {
                        var reference = instruction.Operand as MethodReference;
                        if (reference == null || reference.DeclaringType.FullName != "System.IO.Path" || reference.Name != "Combine") continue;
                        var start = instruction;
                        for (int i = 0; i < 8 && start.Previous != null; i++) start = start.Previous;
                        for (var current = start; current != null; current = current.Next)
                        {
                            Console.WriteLine("IL_" + current.Offset.ToString("X4") + ": " + current.OpCode + " " + (current.Operand ?? ""));
                            if (current == instruction) break;
                        }
                        Console.WriteLine();
                    }
                    return 0;
                }
                if (args[1].StartsWith("method="))
                {
                    string selector = args[1].Substring(7);
                    int separator = selector.LastIndexOf("::", StringComparison.Ordinal);
                    if (separator < 1 || separator == selector.Length - 2)
                        throw new InvalidOperationException("Method selector must be Type::Name: " + selector);

                    string typeName = selector.Substring(0, separator);
                    string methodName = selector.Substring(separator + 2);
                    TypeDefinition selected = assembly.MainModule.GetType(typeName);
                    if (selected == null) throw new InvalidOperationException("Type not found: " + typeName);
                    foreach (var selectedMethod in selected.Methods.Where(m => m.Name == methodName && m.HasBody))
                        DumpMethod(selectedMethod);
                    return 0;
                }
                if (args[1].StartsWith("find="))
                {
                    string pattern = args[1].Substring(5);
                    foreach (var type in assembly.MainModule.GetTypes())
                    foreach (var method in type.Methods.Where(m => m.HasBody))
                    foreach (var instruction in method.Body.Instructions)
                    {
                        string value = instruction.Operand == null ? "" : instruction.Operand.ToString();
                        if (value.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                            Console.WriteLine(method.FullName + " | IL_" + instruction.Offset.ToString("X4") + ": " + instruction.OpCode + " " + value);
                    }
                    return 0;
                }
                foreach (var selectedMethod in selectedType.Methods.Where(m => m.Name == args[1] && m.HasBody))
                    DumpMethod(selectedMethod);
                return 0;
            }

            Console.WriteLine("Assembly: " + assembly.Name.FullName);
            Console.WriteLine("Runtime:  " + assembly.MainModule.RuntimeVersion);
            Console.WriteLine("Kind:     " + assembly.MainModule.Kind);
            Console.WriteLine("Arch:     " + assembly.MainModule.Architecture);
            Console.WriteLine("References:");
            foreach (var reference in assembly.MainModule.AssemblyReferences)
                Console.WriteLine("  " + reference.FullName);

            var mainType = assembly.MainModule.GetType("Terraria.Main");
            if (mainType == null)
                throw new InvalidOperationException("Terraria.Main was not found.");

            Console.WriteLine("Fields of interest:");
            foreach (var field in mainType.Fields.Where(f =>
                f.Name.IndexOf("Frame", StringComparison.OrdinalIgnoreCase) >= 0 ||
                f.Name.IndexOf("UpdateTime", StringComparison.OrdinalIgnoreCase) >= 0 ||
                f.Name.IndexOf("Target", StringComparison.OrdinalIgnoreCase) >= 0))
                Console.WriteLine("  " + field.FullName);

            Console.WriteLine("Methods of interest:");
            foreach (var method in mainType.Methods.Where(m =>
                m.Name.IndexOf("Update", StringComparison.OrdinalIgnoreCase) >= 0 ||
                m.Name.IndexOf("Draw", StringComparison.OrdinalIgnoreCase) >= 0 ||
                m.Name.IndexOf("Initialize", StringComparison.OrdinalIgnoreCase) >= 0))
                Console.WriteLine("  " + method.FullName + " | body=" + method.HasBody);

            foreach (var methodName in new[] { "Update", "DoDraw", "Draw", "Initialize" })
            {
                foreach (var method in mainType.Methods.Where(m => m.Name == methodName && m.HasBody))
                    DumpMethod(method);
            }
        }

        return 0;
    }

    private static void DumpMethod(MethodDefinition method)
    {
        Console.WriteLine();
        Console.WriteLine("IL " + method.FullName + " attrs=" + method.Attributes);
        foreach (var instruction in method.Body.Instructions)
        {
            string operand = instruction.Operand == null ? "" : " " + FormatOperand(instruction.Operand);
            Console.WriteLine("  IL_" + instruction.Offset.ToString("X4") + ": " + instruction.OpCode + operand);
        }
    }

    private static void DumpMethodRange(MethodDefinition method, int firstOffset, int lastOffset)
    {
        Console.WriteLine("IL " + method.FullName);
        foreach (var instruction in method.Body.Instructions.Where(i => i.Offset >= firstOffset && i.Offset <= lastOffset))
        {
            string operand = instruction.Operand == null ? "" : " " + FormatOperand(instruction.Operand);
            Console.WriteLine("  IL_" + instruction.Offset.ToString("X4") + ": " + instruction.OpCode + operand);
        }
    }

    private static string FormatOperand(object operand)
    {
        var target = operand as Instruction;
        if (target != null)
            return "IL_" + target.Offset.ToString("X4");

        var targets = operand as Instruction[];
        if (targets != null)
            return string.Join(", ", targets.Select(i => "IL_" + i.Offset.ToString("X4")));

        return operand.ToString();
    }
}
