# How it works

[English](architecture.md) · [Русский](architecture.ru.md) · [README](../README.md)

## The idea

A game can update its world 60 times per second and still draw more than 60 frames. Without intermediate positions, several frames show the same movement step. High FPS Support fills those visual gaps with positions between the last two simulation states.

For example, on a 144 Hz display the world still advances once every approximately 16.67 ms, while drawing has a new opportunity about every 6.94 ms. The exact drawing rate depends on the machine, display and game. Smoother motion does not mean more physics updates or a guaranteed performance increase.

## Three responsibilities

| Component | Responsibility |
| --- | --- |
| `src/HighFPS.Launcher/MainForm.cs` and `Localization.cs` | Native Windows UI, seven languages and recoverable user-facing errors |
| `LauncherEngine.cs`, `GameLocator.cs`, `HighFpsPatcher.cs` | Find the local game, validate it, create a separate executable, verify/install/launch/remove mod files |
| `src/HighFPS.Support/FpsManager.cs` | Snapshot, interpolate and restore render state inside the game process |

Mono.Cecil is used for managed IL inspection and rewriting at installation time. The game loads the small interpolation runtime; it does not run Mono.Cecil to render each frame. The original game and XNA are local build/runtime prerequisites, not project dependencies distributed as game binaries.

## One frame, in detail

1. **Before update:** force the game's accumulator-compatible `Frame Skip: Off` path. Predict whether the elapsed time crosses a real 60 Hz tick boundary and capture positions before that tick. Partial updates refresh cursor coordinates for drawing while retaining gameplay button state.
2. **Before draw:** calculate `alpha = clamp(UpdateTimeAccumulator / (1 / 60), 0, 1)`. Temporarily replace each eligible position with `previous + (current - previous) * alpha`. Held-item positions and angles are interpolated where appropriate.
3. **After draw:** restore simulation positions and item state. Keep Terraria in control of camera updates; do not restore an old camera position that would make world-space aiming disagree with the displayed frame.

The patch adds exactly **three runtime calls** around `Main.DoUpdate` and `Main.DoDraw`. Entity identity checks prevent a replacement object in a reused slot from inheriting another entity's interpolation. Large displacements skip interpolation to avoid stretching teleports across the screen. Snapshots are invalidated in menus. If a prior draw was interrupted, pending render state is restored at the next update/draw entry before taking another snapshot.

Errors in interpolation attempt restoration and disable that runtime instance with local diagnostics. This is defensive recovery, not a guarantee that every game exception can be recovered. The game remains responsible for its update loop, networking, saves and rendering pipeline.

## Limits to know

- Interpolation between completed ticks introduces visual delay of up to roughly one 60 Hz tick. It does not predict future game state or lower simulation/input latency.
- Only selected entity positions and held-item state are interpolated. Not every animation, particle, lighting effect or UI element gains additional simulation steps.
- The mod does not change network tick rate, server code or multiplayer protocol. That design is not a promise of compatibility with every server, anti-cheat policy, achievement feature or other mod.
- Rendering additional frames consumes CPU/GPU resources. A bottlenecked machine may show little benefit.
- Compatibility checks recognize an expected structure; they do not establish Steam authenticity or endorse modified same-version files. Only the documented original Steam build is supported.
- The generated executable contains proprietary game code and is for local use. Share the launcher/source release, not that executable.

See [security](../SECURITY.md) for installation integrity and [build/test instructions](building.md) for verification scope.
