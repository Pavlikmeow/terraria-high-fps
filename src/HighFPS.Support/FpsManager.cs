using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Enums;
using Terraria.GameInput;

[assembly: AssemblyTitle("High FPS Support runtime for Terraria")]
[assembly: AssemblyDescription("Render-only interpolation for Terraria 1.4.5.8")]
[assembly: AssemblyCompany("pavlikmeow")]
[assembly: AssemblyProduct("High FPS Support")]
[assembly: AssemblyCopyright("Copyright (c) 2026 pavlikmeow")]
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]
[assembly: ComVisible(false)]

namespace TerrariaHighFPS
{
    /// <summary>
    /// Render-only interpolation for Terraria's native 60 Hz accumulator mode.
    /// Simulation state is captured immediately before a real tick. During drawing,
    /// entities are moved between the previous and current tick states, then restored.
    /// RU: Сглаживание только для отрисовки. Состояние до тика сохраняется,
    /// координаты временно интерполируются для кадра, затем восстанавливаются.
    /// </summary>
    public static class FpsManager
    {
        private const double TickSeconds = 1.0 / 60.0;
        private const float EntityTeleportDistanceSquared = 512f * 512f;
        private const float ProjectileTeleportDistanceSquared = 2048f * 2048f;

        private static bool _disabled;
        private static bool _loggedStartup;
        private static bool _hasSnapshot;
        private static bool _didApply;

        private static Player[] _previousPlayers;
        private static NPC[] _previousNpcs;
        private static Projectile[] _previousProjectiles;
        private static WorldItem[] _previousItems;

        private static Vector2[] _previousPlayerPosition;
        private static Vector2[] _previousPlayerItemLocation;
        private static float[] _previousPlayerItemRotation;
        private static bool[] _previousPlayerActive;
        private static bool[] _previousPlayerItemAnimating;
        private static Vector2[] _renderPlayerPosition;
        private static Vector2[] _renderPlayerItemLocation;
        private static float[] _renderPlayerItemRotation;
        private static bool[] _renderPlayerValid;

        private static Vector2[] _previousNpcPosition;
        private static bool[] _previousNpcActive;
        private static Vector2[] _renderNpcPosition;
        private static bool[] _renderNpcValid;

        private static Vector2[] _previousProjectilePosition;
        private static bool[] _previousProjectileActive;
        private static Vector2[] _renderProjectilePosition;
        private static bool[] _renderProjectileValid;

        private static Vector2[] _previousItemPosition;
        private static bool[] _previousItemActive;
        private static Vector2[] _renderItemPosition;
        private static bool[] _renderItemValid;

        /// <summary>EN: Before Main.DoUpdate. RU: Перед штатным обновлением игры.</summary>
        public static void BeforeUpdate(GameTime gameTime)
        {
            if (_disabled)
                return;

            try
            {
                // EN: Recover if a previous draw threw before the injected PostDraw hook.
                // RU: Восстанавливаем координаты, если прошлый кадр прервался до PostDraw.
                if (_didApply) RestoreRenderState();
                if (Main.gameMenu) _hasSnapshot = false;

                // RU: Frame Skip Off включает штатный накопитель времени игры.
                // High-FPS rendering requires Terraria's accumulator path. Keeping this
                // forced here makes the separate High FPS executable one-click to use.
                Main.FrameSkipMode = (FrameSkipMode)0;

                if (!_loggedStartup)
                {
                    _loggedStartup = true;
                    Log("High FPS Support active; game logic remains at 60 Hz.");
                }

                double elapsed = gameTime == null ? 0.0 : gameTime.ElapsedGameTime.TotalSeconds;
                bool realTickWillRun = Main.UpdateTimeAccumulator + elapsed + 0.0000001 >= TickSeconds;

                if (realTickWillRun && !Main.gameMenu)
                    CaptureBeforeTick();

                // RU: Полный тик обрабатывает мышь штатно; между тиками обновляем только координаты.
                // Full ticks execute Terraria's own MouseInput a few instructions
                // later. Only partial accumulator updates need a fresh cursor for
                // the extra rendered frame; this keeps real gameplay input vanilla.
                if (!realTickWillRun)
                    RefreshMouseForRender();
            }
            catch (Exception ex)
            {
                Disable("BeforeUpdate", ex);
            }
        }

        /// <summary>EN: Before Main.DoDraw. RU: Перед отрисовкой кадра.</summary>
        public static void PreDraw()
        {
            if (_disabled || !_hasSnapshot || Main.gameMenu || (int)Main.FrameSkipMode != 0)
                return;

            try
            {
                // EN: Repeated draws must always interpolate from simulation coordinates.
                // RU: Повторный кадр всегда начинается с исходных игровых координат.
                if (_didApply) RestoreRenderState();
                float alpha = (float)(Main.UpdateTimeAccumulator / TickSeconds);
                if (alpha < 0f) alpha = 0f;
                if (alpha > 1f) alpha = 1f;

                _didApply = true;
                ApplyPlayers(alpha);
                ApplyNpcs(alpha);
                ApplyProjectiles(alpha);
                ApplyItems(alpha);
            }
            catch (Exception ex)
            {
                try { RestoreRenderState(); }
                catch { }
                Disable("PreDraw", ex);
            }
        }

        /// <summary>EN: After Main.DoDraw. RU: После отрисовки кадра.</summary>
        public static void PostDraw()
        {
            if (!_didApply)
                return;

            try
            {
                RestoreRenderState();
            }
            catch (Exception ex)
            {
                Disable("PostDraw", ex);
            }
        }

        private static void CaptureBeforeTick()
        {
            // EN: Slot identities prevent blending a newly spawned object with its predecessor.
            // RU: Проверка объекта не даёт смешать новую сущность с прежним владельцем ячейки.
            EnsureCapacity();

            int i;
            for (i = 0; i < Main.player.Length; i++)
            {
                Player entity = Main.player[i];
                bool active = entity != null && entity.active;
                _previousPlayers[i] = entity;
                _previousPlayerActive[i] = active;
                if (!active) continue;

                _previousPlayerPosition[i] = entity.position;
                _previousPlayerItemLocation[i] = entity.itemLocation;
                _previousPlayerItemRotation[i] = entity.itemRotation;
                _previousPlayerItemAnimating[i] = entity.itemAnimation > 0;
            }

            for (i = 0; i < Main.npc.Length; i++)
            {
                NPC entity = Main.npc[i];
                bool active = entity != null && entity.active;
                _previousNpcs[i] = entity;
                _previousNpcActive[i] = active;
                if (active)
                {
                    _previousNpcPosition[i] = entity.position;
                }
            }

            for (i = 0; i < Main.projectile.Length; i++)
            {
                Projectile entity = Main.projectile[i];
                bool active = entity != null && entity.active;
                _previousProjectiles[i] = entity;
                _previousProjectileActive[i] = active;
                if (active)
                {
                    _previousProjectilePosition[i] = entity.position;
                }
            }

            for (i = 0; i < Main.item.Length; i++)
            {
                WorldItem entity = Main.item[i];
                bool active = entity != null && entity.active;
                _previousItems[i] = entity;
                _previousItemActive[i] = active;
                if (active)
                {
                    _previousItemPosition[i] = entity.position;
                }
            }

            _hasSnapshot = true;
        }

        private static void ApplyPlayers(float alpha)
        {
            int count = Main.player.Length;
            for (int i = 0; i < count; i++)
            {
                Player entity = Main.player[i];
                bool valid = entity != null && entity.active && _previousPlayerActive[i] &&
                    ReferenceEquals(entity, _previousPlayers[i]) &&
                    CanInterpolate(_previousPlayerPosition[i], entity.position, EntityTeleportDistanceSquared);
                _renderPlayerValid[i] = valid;
                if (!valid) continue;

                _renderPlayerPosition[i] = entity.position;
                _renderPlayerItemLocation[i] = entity.itemLocation;
                _renderPlayerItemRotation[i] = entity.itemRotation;

                entity.position = Lerp(_previousPlayerPosition[i], _renderPlayerPosition[i], alpha);
                if (_previousPlayerItemAnimating[i] && entity.itemAnimation > 0)
                {
                    entity.itemLocation = Lerp(_previousPlayerItemLocation[i], _renderPlayerItemLocation[i], alpha);
                    entity.itemRotation = LerpAngle(_previousPlayerItemRotation[i], _renderPlayerItemRotation[i], alpha);
                }
            }
        }

        private static void ApplyNpcs(float alpha)
        {
            int count = Main.npc.Length;
            for (int i = 0; i < count; i++)
            {
                NPC entity = Main.npc[i];
                bool valid = entity != null && entity.active && _previousNpcActive[i] &&
                    ReferenceEquals(entity, _previousNpcs[i]) &&
                    CanInterpolate(_previousNpcPosition[i], entity.position, EntityTeleportDistanceSquared);
                _renderNpcValid[i] = valid;
                if (!valid) continue;

                _renderNpcPosition[i] = entity.position;
                entity.position = Lerp(_previousNpcPosition[i], _renderNpcPosition[i], alpha);
            }
        }

        private static void ApplyProjectiles(float alpha)
        {
            int count = Main.projectile.Length;
            for (int i = 0; i < count; i++)
            {
                Projectile entity = Main.projectile[i];
                bool valid = entity != null && entity.active && _previousProjectileActive[i] &&
                    ReferenceEquals(entity, _previousProjectiles[i]) &&
                    CanInterpolate(_previousProjectilePosition[i], entity.position, ProjectileTeleportDistanceSquared);
                _renderProjectileValid[i] = valid;
                if (!valid) continue;

                _renderProjectilePosition[i] = entity.position;
                entity.position = Lerp(_previousProjectilePosition[i], _renderProjectilePosition[i], alpha);
            }
        }

        private static void ApplyItems(float alpha)
        {
            int count = Main.item.Length;
            for (int i = 0; i < count; i++)
            {
                WorldItem entity = Main.item[i];
                bool valid = entity != null && entity.active && _previousItemActive[i] &&
                    ReferenceEquals(entity, _previousItems[i]) &&
                    CanInterpolate(_previousItemPosition[i], entity.position, EntityTeleportDistanceSquared);
                _renderItemValid[i] = valid;
                if (!valid) continue;

                _renderItemPosition[i] = entity.position;
                entity.position = Lerp(_previousItemPosition[i], _renderItemPosition[i], alpha);
            }
        }

        private static void RestoreRenderState()
        {
            // EN: Terraria owns the camera; rolling it back would break MouseWorld targeting.
            // RU: Камерой управляет Terraria; её откат нарушил бы прицеливание MouseWorld.
            int i;
            if (Main.player != null && _renderPlayerValid != null)
            {
                int count = Math.Min(Main.player.Length, _renderPlayerValid.Length);
                for (i = 0; i < count; i++)
                {
                    if (!_renderPlayerValid[i]) continue;
                    Player entity = Main.player[i];
                    if (entity != null && ReferenceEquals(entity, _previousPlayers[i]))
                    {
                        entity.position = _renderPlayerPosition[i];
                        entity.itemLocation = _renderPlayerItemLocation[i];
                        entity.itemRotation = _renderPlayerItemRotation[i];
                    }
                    _renderPlayerValid[i] = false;
                }
            }

            if (Main.npc != null && _renderNpcValid != null)
            {
                int count = Math.Min(Main.npc.Length, _renderNpcValid.Length);
                for (i = 0; i < count; i++)
                {
                    if (!_renderNpcValid[i]) continue;
                    NPC entity = Main.npc[i];
                    if (entity != null && ReferenceEquals(entity, _previousNpcs[i])) entity.position = _renderNpcPosition[i];
                    _renderNpcValid[i] = false;
                }
            }

            if (Main.projectile != null && _renderProjectileValid != null)
            {
                int count = Math.Min(Main.projectile.Length, _renderProjectileValid.Length);
                for (i = 0; i < count; i++)
                {
                    if (!_renderProjectileValid[i]) continue;
                    Projectile entity = Main.projectile[i];
                    if (entity != null && ReferenceEquals(entity, _previousProjectiles[i])) entity.position = _renderProjectilePosition[i];
                    _renderProjectileValid[i] = false;
                }
            }

            if (Main.item != null && _renderItemValid != null)
            {
                int count = Math.Min(Main.item.Length, _renderItemValid.Length);
                for (i = 0; i < count; i++)
                {
                    if (!_renderItemValid[i]) continue;
                    WorldItem entity = Main.item[i];
                    if (entity != null && ReferenceEquals(entity, _previousItems[i])) entity.position = _renderItemPosition[i];
                    _renderItemValid[i] = false;
                }
            }

            _didApply = false;
        }

        private static void EnsureCapacity()
        {
            if (Main.player == null || Main.npc == null || Main.projectile == null || Main.item == null)
                throw new InvalidOperationException("Terraria entity arrays are not initialized.");

            int players = Main.player.Length;
            if (_previousPlayerPosition == null || _previousPlayerPosition.Length != players)
            {
                _previousPlayers = new Player[players];
                _previousPlayerPosition = new Vector2[players];
                _previousPlayerItemLocation = new Vector2[players];
                _previousPlayerItemRotation = new float[players];
                _previousPlayerActive = new bool[players];
                _previousPlayerItemAnimating = new bool[players];
                _renderPlayerPosition = new Vector2[players];
                _renderPlayerItemLocation = new Vector2[players];
                _renderPlayerItemRotation = new float[players];
                _renderPlayerValid = new bool[players];
            }

            int npcs = Main.npc.Length;
            if (_previousNpcPosition == null || _previousNpcPosition.Length != npcs)
            {
                _previousNpcs = new NPC[npcs];
                _previousNpcPosition = new Vector2[npcs];
                _previousNpcActive = new bool[npcs];
                _renderNpcPosition = new Vector2[npcs];
                _renderNpcValid = new bool[npcs];
            }

            int projectiles = Main.projectile.Length;
            if (_previousProjectilePosition == null || _previousProjectilePosition.Length != projectiles)
            {
                _previousProjectiles = new Projectile[projectiles];
                _previousProjectilePosition = new Vector2[projectiles];
                _previousProjectileActive = new bool[projectiles];
                _renderProjectilePosition = new Vector2[projectiles];
                _renderProjectileValid = new bool[projectiles];
            }

            int items = Main.item.Length;
            if (_previousItemPosition == null || _previousItemPosition.Length != items)
            {
                _previousItems = new WorldItem[items];
                _previousItemPosition = new Vector2[items];
                _previousItemActive = new bool[items];
                _renderItemPosition = new Vector2[items];
                _renderItemValid = new bool[items];
            }
        }

        private static void RefreshMouseForRender()
        {
            // EN: Sample cursor coordinates without injecting button or wheel events between ticks.
            // RU: Читаем координаты курсора без дополнительных событий кнопок и колеса между тиками.
            MouseState cursor = Mouse.GetState();
            MouseState buttonsFromTick = PlayerInput.MouseInfo;

            PlayerInput.MouseX = (int)(cursor.X * PlayerInput.RawMouseScale.X);
            PlayerInput.MouseY = (int)(cursor.Y * PlayerInput.RawMouseScale.Y);
            PlayerInput.MouseInfo = new MouseState(
                cursor.X,
                cursor.Y,
                buttonsFromTick.ScrollWheelValue,
                buttonsFromTick.LeftButton,
                buttonsFromTick.MiddleButton,
                buttonsFromTick.RightButton,
                buttonsFromTick.XButton1,
                buttonsFromTick.XButton2);
            PlayerInput.UpdateMainMouse();
            PlayerInput.CacheMousePositionForZoom();
        }

        private static Vector2 Lerp(Vector2 previous, Vector2 current, float alpha)
        {
            return previous + (current - previous) * alpha;
        }

        private static float LerpAngle(float previous, float current, float alpha)
        {
            float delta = WrapAngle(current - previous);
            return previous + delta * alpha;
        }

        private static float WrapAngle(float value)
        {
            const float twoPi = (float)(Math.PI * 2.0);
            value %= twoPi;
            if (value > Math.PI) value -= twoPi;
            if (value < -Math.PI) value += twoPi;
            return value;
        }

        private static bool CanInterpolate(Vector2 previous, Vector2 current, float maxDistanceSquared)
        {
            // EN: Teleports and non-finite positions must not smear across the screen.
            // RU: Телепорты и некорректные координаты не должны размазываться по экрану.
            Vector2 delta = current - previous;
            return delta.LengthSquared() <= maxDistanceSquared;
        }

        private static void Disable(string stage, Exception exception)
        {
            _disabled = true;
            _didApply = false;
            Log("Disabled at " + stage + ": " + exception);
        }

        private static void Log(string message)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HighFPS.Support.log");
                File.AppendAllText(path, DateTime.Now.ToString("s") + " " + message + Environment.NewLine);
            }
            catch
            {
                // EN: Logging must never affect the game.
                // RU: Ошибка записи журнала не должна влиять на игру.
            }
        }
    }
}

