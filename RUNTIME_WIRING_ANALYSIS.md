# Runtime Wiring Analysis

Reference for the engine boot/wiring code in [BabyBearsEngine/Source/Runtime/](BabyBearsEngine/Source/Runtime/), [BabyBearsEngine/Source/GameEngine/](BabyBearsEngine/Source/GameEngine/), [BabyBearsEngine/Source/Platform/OpenTK/](BabyBearsEngine/Source/Platform/OpenTK/), and the static facades in [BabyBearsEngine/Source/Windowing/Window.cs](BabyBearsEngine/Source/Windowing/Window.cs), [BabyBearsEngine/Source/Input/Keyboard.cs](BabyBearsEngine/Source/Input/Keyboard.cs), [BabyBearsEngine/Source/Input/Mouse.cs](BabyBearsEngine/Source/Input/Mouse.cs).

The codebase tries to be platform-agnostic (factories, context interfaces, mockable services) but is in practice hard-bound to OpenTK/OpenGL. This document records the tensions and the options for resolving them.

---

## Current architecture

1. `GameLauncher` (static, public entry point) — holds `s_platform : IGamePlatformFactory` defaulting to `OpenTKPlatformFactory`, replaceable via `SetPlatform`. Owns `s_loadedEngine : IGameEngine` and a `LauncherStatus` enum for re-init guarding.
2. `IGamePlatformFactory` — creates an `IGameEngine` and an `IPlatformContext`.
3. `IPlatformContext` — bundle of `IWindow`, `IKeyboard`, `IMouse`, `IWorldSwitcher`.
4. `EngineConfiguration` (static singleton) — receives the `IPlatformContext` from the launcher, plus independently holds `ITextureFactory` and `IGPUResourceDeletionService` with public setters.
5. `OpenTKGameEngine` — the only concrete `IGameEngine`. Subclasses `GameWindow` and implements `IGameEngine` and `IWorldSwitcher`.
6. Adapters: `OpenTKWindowAdapter`, `OpenTKKeyboardAdapter`, `OpenTKMouseAdapter` — wrap OpenTK types with the engine's interfaces.
7. Public statics: `Engine`, `Window`, `Keyboard`, `Mouse`, `Textures`, `GPUResourceDeletion` — all dispatch through `EngineConfiguration`.

---

## Problems

### Cross-platform abstractions don't actually abstract

- **OpenTK types leak through every interface.** [IWindow.cs:9-23](BabyBearsEngine/Source/Windowing/IWindow.cs#L9-L23) exposes `WindowBorder`, `WindowIcon`, `WindowState`, `ResizeEventArgs` from `OpenTK.Windowing.Common`. `IKeyboard` uses `OpenTK.Windowing.GraphicsLibraryFramework.Keys`. `IMouse` uses `MouseButton` from the same. The "platform" you could plug in must already speak OpenTK's vocabulary.
- **`IGameEngine` is too thin to be a real abstraction.** It is just `void Run(IWorld world)` ([IGameEngine.cs:7](BabyBearsEngine/Source/GameEngine/IGameEngine.cs#L7)). The factory then has to downcast: [OpenTKPlatformFactory.cs:15](BabyBearsEngine/Source/Platform/OpenTK/OpenTKPlatformFactory.cs#L15) does `(OpenTKGameEngine)gameEngine`. The cast says the factory contract isn't actually doing the job.
- **`OpenTKGameEngine` is multiple responsibilities in one class.** It IS the `GameWindow`, the game loop, the world switcher, AND it makes raw GL calls in `OnRenderFrame` ([OpenTKGameEngine.cs:69-80](BabyBearsEngine/Source/Platform/OpenTK/OpenTKGameEngine.cs#L69-L80)). There is no `IRenderer` interface — that seam does not exist.

### Two competing globals and incomplete reset

- `GameLauncher` holds `s_platform`/`s_loadedEngine`. `EngineConfiguration` holds another set of statics (`s_backend`, `s_textureFactory`, `s_gpuResourceDeletionService`). Gameplay code reaches services through static facades.
- `GameLauncher.Run`'s finally block clears `s_loadedEngine` and `s_status` ([GameLauncher.cs:65-69](BabyBearsEngine/Source/Runtime/Boot/GameLauncher.cs#L65-L69)) but does NOT clear `EngineConfiguration.s_backend`. A second `GameLauncher.Run(...)` call in the same process throws "already initialised" from [EngineConfiguration.cs:31-34](BabyBearsEngine/Source/Runtime/EngineConfiguration.cs#L31-L34). The single test in [Tests/System/GameLauncherTests.cs](Tests/System/GameLauncherTests.cs) gets away with it only because it's one method.

### Test seams exist but are blunt

- The only way to substitute fakes today is to replace the entire `IGamePlatformFactory` via `GameLauncher.SetPlatform`. There is no per-service hook on `EngineConfiguration` for `IWindow`/`IKeyboard`/`IMouse`/`IWorldSwitcher`.
- The static facades (`Window`, `Keyboard`, `Mouse`, `Engine`) reach a process-global `EngineConfiguration`, so parallel test isolation is impossible without extra plumbing.

---

## Recommendations

Three independent decisions, ordered by ROI.

### Bucket 1 — Be honest about scope: drop the cross-platform pretence, keep test seams

Since OpenTK is the chosen platform, `IGamePlatformFactory` and `IPlatformContext` aren't earning their keep. Two options:

- **(a) Delete them.** `GameLauncher` constructs an `OpenTKGameEngine` directly. Tests mock at the `IWindow`/`IKeyboard`/`IMouse`/`IWorldSwitcher` level by substituting them on `EngineConfiguration` (which would gain a `Reset` and per-service setters).
- **(b) Keep `IPlatformContext` but rename to `IEngineContext` (or `ITestablePlatformContext`)** and document it as a *test seam*, not a portability layer.

**Lean: (a).** Fewer concepts; the seams that actually matter for testing (`IWindow`, `IKeyboard`, `IMouse`) are already separate.

### Bucket 2 — Decide whether `IWindow` etc. should mirror OpenTK types or admit the dependency

Today is the worst of both worlds: the interface exists, but it returns OpenTK types so you cannot fake it without an OpenTK reference and you cannot ever swap the platform. Pick one:

- **Mirror.** Build engine-owned `WindowBorder`/`WindowState`/`Keys`/`MouseButton` enums and translate at the adapter boundary ([OpenTKMappings.cs](BabyBearsEngine/Source/Platform/OpenTK/OpenTKMappings.cs) already does this for `CursorShape`). Meaningful chunk of work; gives genuinely lightweight mocks.
- **Embrace.** Drop `IWindow`/`IKeyboard`/`IMouse`, expose the concrete adapters, accept the OpenTK reference in tests. Smallest change; loses the mock seam.

**Lean: mirror only the input enums** (`Keys`, `MouseButton`) for unit-testable input handling, and leave the rarely-tested window properties as pass-through.

### Bucket 3 — Fix the global-state problems

These bite regardless of buckets 1 and 2.

- **`EngineConfiguration` needs a `Reset()`** so a second `GameLauncher.Run` does not throw, and so tests can install fakes between cases. Today the public setters on `TextureFactory`/`GPUResourceDeletionService` exist but the platform-context-shaped fields cannot be replaced.
- **Static facades (`Window`, `Keyboard`, `Mouse`, `Engine`) are ergonomic but kill parallel-test isolation.** The long-term fix is to thread an `IEngineContext` through `World.Load/Update/Draw` rather than reaching out to globals. That's invasive — fine to defer — but worth deciding on direction.
- **The `IPlatformContext` cast in `OpenTKPlatformFactory`** goes away naturally if you do (1a). Otherwise tighten the factory contract so `CreateGameEngine` returns a type rich enough for `CreatePlatformContext`, no cast needed.

---

## Suggested first move

**Bucket 1(a) + the `Reset()` fix from Bucket 3.** Cheap, removes dead architectural weight without churning gameplay code. Detailed plan: [PLAN_runtime_simplification.md](PLAN_runtime_simplification.md).

Then, when concrete test pain shows up, mirror `Keys` and `MouseButton` (Bucket 2). Defer the bigger renderer/world-context refactor until use cases force the interfaces' shape.
