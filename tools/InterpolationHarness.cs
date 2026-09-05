using System;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using TerrariaHighFPS;

internal static class InterpolationHarness
{
    private static int Main()
    {
        try
        {
            InitializeSavePath();
            RunInterpolationChecks();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void InitializeSavePath()
    {
        Terraria.Program.SavePath = Path.GetTempPath();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RunInterpolationChecks()
    {
            Terraria.Main.player = new[] { new Player { active = true, position = new Vector2(0f, 0f), itemAnimation = 2 } };
            Terraria.Main.npc = new[] { new NPC { active = true, position = new Vector2(20f, 40f) } };
            Terraria.Main.projectile = new[] { new Projectile { active = true, position = new Vector2(100f, 200f) } };
            Terraria.Main.item = new[] { new WorldItem { type = 1, position = new Vector2(300f, 400f) } };
            Terraria.Main.screenPosition = new Vector2(10f, 10f);
            Terraria.Main.gameMenu = false;
            Terraria.Main.FrameSkipMode = (FrameSkipMode)0;

            MethodInfo capture = typeof(FpsManager).GetMethod("CaptureBeforeTick", BindingFlags.Static | BindingFlags.NonPublic);
            if (capture == null) throw new Exception("CaptureBeforeTick was not found.");
            capture.Invoke(null, null);

            Terraria.Main.player[0].position = new Vector2(10f, 20f);
            Terraria.Main.npc[0].position = new Vector2(40f, 80f);
            Terraria.Main.projectile[0].position = new Vector2(120f, 240f);
            Terraria.Main.item[0].position = new Vector2(320f, 440f);
            Terraria.Main.screenPosition = new Vector2(30f, 50f);
            Terraria.Main.UpdateTimeAccumulator = 1.0 / 120.0;

            FpsManager.PreDraw();
            AssertVector("player interpolated", Terraria.Main.player[0].position, new Vector2(5f, 10f));
            AssertVector("npc interpolated", Terraria.Main.npc[0].position, new Vector2(30f, 60f));
            AssertVector("projectile interpolated", Terraria.Main.projectile[0].position, new Vector2(110f, 220f));
            AssertVector("item interpolated", Terraria.Main.item[0].position, new Vector2(310f, 420f));
            AssertVector("camera untouched before DoDraw", Terraria.Main.screenPosition, new Vector2(30f, 50f));

            // Terraria owns screenPosition inside DoDraw_UpdateCameraPosition. The
            // interpolation layer must keep the camera value produced by DoDraw;
            // restoring the pre-draw value makes MouseWorld point back at spawn.
            Terraria.Main.screenPosition = new Vector2(70f, 90f);

            FpsManager.PostDraw();
            AssertVector("player restored", Terraria.Main.player[0].position, new Vector2(10f, 20f));
            AssertVector("npc restored", Terraria.Main.npc[0].position, new Vector2(40f, 80f));
            AssertVector("projectile restored", Terraria.Main.projectile[0].position, new Vector2(120f, 240f));
            AssertVector("item restored", Terraria.Main.item[0].position, new Vector2(320f, 440f));
            AssertVector("DoDraw camera retained", Terraria.Main.screenPosition, new Vector2(70f, 90f));

            Terraria.Main.mouseX = 400;
            Terraria.Main.mouseY = 250;
            AssertVector("MouseWorld uses retained camera", Terraria.Main.MouseWorld, new Vector2(470f, 340f));

            Console.WriteLine("Interpolation at alpha=0.5 verified for player, NPC, projectile, and item.");
            Console.WriteLine("PostDraw restoration verified.");
            Console.WriteLine("DoDraw camera ownership and MouseWorld mapping verified.");
            RunEdgeCases(capture);
    }

    private static void RunEdgeCases(MethodInfo capture)
    {
        // EN: Exercise observable state boundaries, including interrupted draws and reused slots.
        // RU: Проверяем границы состояния: прерванный кадр и повторное использование ячеек.
        Player player = Terraria.Main.player[0];
        player.position = Vector2.Zero;
        player.itemLocation = Vector2.Zero;
        player.itemRotation = 3.1f;
        capture.Invoke(null, null);
        player.position = new Vector2(10f, 0f);
        player.itemLocation = new Vector2(20f, 10f);
        player.itemRotation = -3.1f;
        Terraria.Main.UpdateTimeAccumulator = 1.0 / 120.0;
        FpsManager.PreDraw();
        AssertVector("item animation interpolated", player.itemLocation, new Vector2(10f, 5f));
        if (Math.Abs(Math.Abs(player.itemRotation) - Math.PI) > 0.001) throw new Exception("Angle took the long path.");
        FpsManager.PreDraw();
        AssertVector("repeated draw does not compound", player.position, new Vector2(5f, 0f));
        FpsManager.PostDraw();
        AssertVector("item animation restored", player.itemLocation, new Vector2(20f, 10f));

        capture.Invoke(null, null);
        player.position = new Vector2(1000f, 0f);
        FpsManager.PreDraw();
        AssertVector("teleport is immediate", player.position, new Vector2(1000f, 0f));
        FpsManager.PostDraw();

        capture.Invoke(null, null);
        Player replacement = new Player { active = true, position = new Vector2(1020f, 0f) };
        Terraria.Main.player[0] = replacement;
        FpsManager.PreDraw();
        AssertVector("new object not blended with old slot", replacement.position, new Vector2(1020f, 0f));
        FpsManager.PostDraw();

        capture.Invoke(null, null);
        replacement.position = new Vector2(1040f, 0f);
        FpsManager.PreDraw();
        Terraria.Main.player[0] = player;
        FpsManager.PostDraw();
        AssertVector("replacement during draw not overwritten", player.position, new Vector2(1000f, 0f));

        capture.Invoke(null, null);
        player.position = new Vector2(1010f, 0f);
        Terraria.Main.UpdateTimeAccumulator = 1.0 / 120.0;
        FpsManager.PreDraw();
        // EN: Deliberately skip PostDraw to simulate an exception in Terraria's drawing code.
        // RU: Пропускаем PostDraw, имитируя исключение во время отрисовки Terraria.
        FpsManager.BeforeUpdate(new GameTime(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1.0 / 60.0)));
        AssertVector("interrupted draw restored before tick", player.position, new Vector2(1010f, 0f));
        Terraria.Main.gameMenu = true;
        FpsManager.BeforeUpdate(new GameTime(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1.0 / 60.0)));
        Terraria.Main.gameMenu = false;
        player.position = new Vector2(1020f, 0f);
        FpsManager.PreDraw();
        AssertVector("menu clears stale world snapshot", player.position, new Vector2(1020f, 0f));
        FpsManager.PostDraw();
        Console.WriteLine("Animation, repeated/interrupted draw, teleport, slot identity and world transition checks passed.");
    }

    private static void AssertVector(string name, Vector2 actual, Vector2 expected)
    {
        if (Math.Abs(actual.X - expected.X) > 0.001f || Math.Abs(actual.Y - expected.Y) > 0.001f)
            throw new Exception(name + ": expected " + expected + ", got " + actual + ".");
    }

}
