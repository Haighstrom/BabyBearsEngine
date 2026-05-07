# Runtime Simplification Plan

Implements **Bucket 1(a) + the `Reset()` fix** from [RUNTIME_WIRING_ANALYSIS.md](RUNTIME_WIRING_ANALYSIS.md).

Goal: drop the `IGamePlatformFactory` / `IPlatformContext` indirection (which doesn't actually buy cross-platform support), and add the missing teardown so the engine can be re-initialised in the same process. Result: a smaller engine that is still mockable at the `IWindow`/`IKeyboard`/`IMouse`/`IWorldSwitcher` level.

Steps are ordered easiest-to-hardest, each independently shippable. After each step the project should still build and the existing test in [Tests/System/GameLauncherTests.cs](Tests/System/GameLauncherTests.cs) should still pass.

---

## Step 1 — Add `EngineConfiguration.Reset()` and call it from `GameLauncher.Run`

**Status:** Done

Smallest change, fixes the latent re-initialisation bug, no behaviour change for current callers.

In [BabyBearsEngine/Source/Runtime/EngineConfiguration.cs](BabyBearsEngine/Source/Runtime/EngineConfiguration.cs):

- Add a public static `Reset()` that clears `s_backend` back to `null` and resets `s_textureFactory` and `s_gpuResourceDeletionService` to fresh defaults (so test isolation works for those too).

In [BabyBearsEngine/Source/Runtime/Boot/GameLauncher.cs](BabyBearsEngine/Source/Runtime/Boot/GameLauncher.cs):

- In the `finally` block of `Run` (currently lines 65–69), call `EngineConfiguration.Reset()` alongside the existing `s_loadedEngine = null; s_status = LauncherStatus.NotStarted;`.

Add a small system test to [Tests/System/GameLauncherTests.cs](Tests/System/GameLauncherTests.cs) that calls `GameLauncher.Run(...)` twice in the same method to lock the fix in.

---

## Step 2 — Refactor `EngineConfiguration` to store services directly

**Status:** Done

Stop holding an `IPlatformContext` and instead store each service in its own field. This is the largest mechanical change and is the prerequisite for deleting `IPlatformContext`.

In [BabyBearsEngine/Source/Runtime/EngineConfiguration.cs](BabyBearsEngine/Source/Runtime/EngineConfiguration.cs):

- Replace the `s_backend` field and `Backend` property with four fields: `s_window`, `s_keyboard`, `s_mouse`, `s_worldSwitcher`.
- Replace `Initialise(IPlatformContext)` with `Initialise(IWindow window, IKeyboard keyboard, IMouse mouse, IWorldSwitcher worldSwitcher)`. Same not-yet-initialised guard as today.
- Make `WindowService`, `KeyboardService`, `MouseService`, `WorldSwitcher` properties have `set` accessors guarded against null, mirroring the existing pattern for `TextureFactory` / `GPUResourceDeletionService`. This is the test seam — a unit test can substitute any one service after `Initialise`.
- Update `Reset()` to null out all four service fields.

The "not initialised" message used by the static facades (`Window.Width`, `Keyboard.KeyDown`, etc.) needs to come from somewhere; the simplest move is to throw on get when any of the four service fields is null, with the existing `NotInitialisedMessage` text.

Callers of `EngineConfiguration.Initialise(IPlatformContext)` are updated in Step 3.

---

## Step 3 — Inline service construction in `GameLauncher`

**Status:** Done

Drop the factory indirection. `GameLauncher` constructs the `OpenTKGameEngine` and the adapters itself.

In [BabyBearsEngine/Source/Runtime/Boot/GameLauncher.cs](BabyBearsEngine/Source/Runtime/Boot/GameLauncher.cs):

- Remove the `s_platform` field and the `SetPlatform` / `SetDefaultPlatform` methods.
- In `Initialise`, replace the factory calls with direct construction:
  ```csharp
  var engine = new OpenTKGameEngine(appSettings);
  s_loadedEngine = engine;
  EngineConfiguration.Initialise(
      window: new OpenTKWindowAdapter(engine),
      keyboard: new OpenTKKeyboardAdapter(engine.KeyboardState),
      mouse: new OpenTKMouseAdapter(engine.MouseState),
      worldSwitcher: engine);
  ```
- `s_loadedEngine` can change type from `IGameEngine?` to `OpenTKGameEngine?` — the launcher is now allowed to know.

After this step, [BabyBearsEngine/Source/Platform/OpenTK/OpenTKPlatformFactory.cs](BabyBearsEngine/Source/Platform/OpenTK/OpenTKPlatformFactory.cs) and [BabyBearsEngine/Source/Platform/OpenTK/OpenTKContext.cs](BabyBearsEngine/Source/Platform/OpenTK/OpenTKContext.cs) have no callers.

---

## Step 4 — Delete the now-unused indirection types

**Status:** Done

Remove the dead code:

- [BabyBearsEngine/Source/Runtime/Boot/IGamePlatformFactory.cs](BabyBearsEngine/Source/Runtime/Boot/IGamePlatformFactory.cs)
- [BabyBearsEngine/Source/Runtime/IPlatformContext.cs](BabyBearsEngine/Source/Runtime/IPlatformContext.cs)
- [BabyBearsEngine/Source/Platform/OpenTK/OpenTKPlatformFactory.cs](BabyBearsEngine/Source/Platform/OpenTK/OpenTKPlatformFactory.cs)
- [BabyBearsEngine/Source/Platform/OpenTK/OpenTKContext.cs](BabyBearsEngine/Source/Platform/OpenTK/OpenTKContext.cs)

Also consider whether `IGameEngine` itself ([BabyBearsEngine/Source/GameEngine/IGameEngine.cs](BabyBearsEngine/Source/GameEngine/IGameEngine.cs)) still earns its keep: with `GameLauncher` knowing it always has an `OpenTKGameEngine`, the only remaining benefit of `IGameEngine` is documentation. Defer that decision — leaving it in place is harmless.

Verify there are no remaining references with a grep for `IGamePlatformFactory`, `IPlatformContext`, `OpenTKPlatformFactory`, `OpenTKContext`.

---

## Step 5 — Document the test seam

**Status:** Done

Add a short paragraph or `<remarks>` doc-comment on `EngineConfiguration` describing how to install fakes for unit tests:

- Call `EngineConfiguration.Initialise(fakeWindow, fakeKeyboard, fakeMouse, fakeWorldSwitcher)` directly (no `GameLauncher.Run` needed).
- Substitute individual services after init via the public setters.
- Always call `EngineConfiguration.Reset()` in test teardown to avoid bleeding state between tests.

Optional: add one example unit test under [Tests/Unit/](Tests/Unit/) demonstrating the pattern (e.g. assert that `Engine.ChangeWorld(...)` calls `IWorldSwitcher.RequestWorldChange` on a fake).

---

## Out of scope

Explicitly deferred until a separate decision:

- Mirroring OpenTK enums on `IWindow` / `IKeyboard` / `IMouse` (Bucket 2 in the analysis).
- Threading an `IEngineContext` through `World.Load/Update/Draw` to remove static-facade reliance from gameplay code.
- Extracting an `IRenderer` interface from `OpenTKGameEngine`.
