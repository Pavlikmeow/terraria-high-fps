using System;
using System.IO;
using System.Linq;
using Mono.Cecil;

internal static class ResourceExtractor
{
    private static int Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Usage: ResourceExtractor <assembly> <resource-name> <output>");
            return 2;
        }

        try
        {
            using (var assembly = AssemblyDefinition.ReadAssembly(args[0], new ReaderParameters { InMemory = true }))
            {
                EmbeddedResource resource = assembly.MainModule.Resources
                    .OfType<EmbeddedResource>()
                    .Single(r => r.Name == args[1]);
                File.WriteAllBytes(args[2], resource.GetResourceData());
                Console.WriteLine("Extracted " + resource.Name + " (" + resource.GetResourceData().Length + " bytes)");
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}
