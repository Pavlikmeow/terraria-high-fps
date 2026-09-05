using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

internal static class JitHarness
{
    private static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: JitHarness <Terraria.HighFPS.exe>");
            return 2;
        }

        try
        {
            Assembly assembly = null;
            AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs eventArgs)
            {
                if (assembly != null && new AssemblyName(eventArgs.Name).Name == "Terraria")
                    return assembly;
                return null;
            };
            assembly = Assembly.LoadFrom(args[0]);
            Type mainType = assembly.GetType("Terraria.Main", true);
            MethodInfo[] hooks = mainType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(m => (m.Name == "Update" || m.Name == "Draw") && m.GetParameters().Length == 1)
                .ToArray();
            if (hooks.Length != 2) throw new Exception("Expected Update and Draw methods.");

            foreach (MethodInfo hook in hooks)
            {
                RuntimeHelpers.PrepareMethod(hook.MethodHandle);
                Console.WriteLine("JIT verified: " + hook);
            }

            Type managerType = Assembly.LoadFrom(
                System.IO.Path.Combine(System.IO.Path.GetDirectoryName(args[0]), "HighFPS.Support.dll"))
                .GetType("TerrariaHighFPS.FpsManager", true);
            foreach (string name in new[] { "BeforeUpdate", "PreDraw", "PostDraw" })
            {
                MethodInfo method = managerType.GetMethod(name, BindingFlags.Static | BindingFlags.Public);
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                Console.WriteLine("JIT verified: " + method);
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
