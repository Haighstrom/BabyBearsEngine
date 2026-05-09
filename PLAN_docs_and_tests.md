# Docs & Tests Plan

Prioritised work for adding XML doc comments to the public API and unit/system tests for BabyBearsEngine.

Caveat: tier assignments are based on a quick coverage survey; verify status (especially "internal vs public" calls) before committing each item. Some types listed as public may turn out to be internal (e.g. `FontBitmapGenerator`, `GeneratedFontStruct`).

---

## Tier 1 — Front door of the library

Foundational value types, recently-refactored surfaces, entry points. Stable enough to lock in.

### 1.1 — Colour
- **File:** [BabyBearsEngine/Source/Colour.cs](BabyBearsEngine/Source/Colour.cs)
- **Docs:** Done (full XML).
- **Tests:** Done. 26 tests in [Tests/Unit/ColourTests.cs](Tests/Unit/ColourTests.cs) covering constructors, normalised components, equality, `WithAlpha`, `Darkened`/`Lightened`, `ToArgb`, `ToColor`, OpenTK round-trip, named-colour spot checks.
- **Follow-up:** Two clamp tests (out-of-range float input) skipped — they trip a pre-existing bug in [Logger.cs:18-21](BabyBearsEngine/Source/Diagnostics/Logger.cs#L18-L21) where `log.txt` is created as a *directory* instead of a file (the `if (!Directory.Exists(logLocation)) Directory.CreateDirectory(logLocation)` block uses the file path for both checks). DEBUG-mode `FloatToByte` clamp warnings then fail when the file logger tries to append. Restore those tests once Logger is fixed.

### 1.2 — Rect
- **File:** [BabyBearsEngine/Source/Geometry/Rect.cs](BabyBearsEngine/Source/Geometry/Rect.cs)
- **Docs:** Finish (currently partial).
- **Tests:** Add. Pure geometry — `Intersection`, `Intersects`, `Contains` (multiple overloads, edge-on-border cases), `Shift`, `Scale`, `ScaleAround`, `ResizeAround`, `Grow`, `Zeroed`, derived properties (`Left`/`Right`/`Top`/`Bottom`/`Centre*`/`*Right`/etc.), operators, `EmptyRect`/`UnitRect`. The string ctor + `ToString` round-trip is also worth pinning.

### 1.3 — Static facades (Window / Keyboard / Mouse)
- **Files:** [Windowing/Window.cs](BabyBearsEngine/Source/Windowing/Window.cs), [Input/Keyboard.cs](BabyBearsEngine/Source/Input/Keyboard.cs), [Input/Mouse.cs](BabyBearsEngine/Source/Input/Mouse.cs)
- **Docs:** Add. These are what game devs hit first.
- **Tests:** Add via fakes. The wiring refactor made this seam usable — substitute fakes via `EngineConfiguration.{Window,Keyboard,Mouse}Service` and verify the static facades route through them. Also covers the per-service setter contract.

### 1.4 — Platform interfaces (IWindow / IKeyboard / IMouse)
- **Files:** [Windowing/IWindow.cs](BabyBearsEngine/Source/Windowing/IWindow.cs), [Input/IKeyboard.cs](BabyBearsEngine/Source/Input/IKeyboard.cs), [Input/IMouse.cs](BabyBearsEngine/Source/Input/IMouse.cs)
- **Docs:** Add. Public contract.
- **Tests:** None — no behaviour to test on the interface itself.

### 1.5 — Mirrored enums
- **Files:** [WindowBorder.cs](BabyBearsEngine/Source/Windowing/WindowBorder.cs), [WindowState.cs](BabyBearsEngine/Source/Windowing/WindowState.cs), [Keys.cs](BabyBearsEngine/Source/Input/Keys.cs), [MouseButton.cs](BabyBearsEngine/Source/Input/MouseButton.cs), [CursorShape.cs](BabyBearsEngine/Source/Runtime/Settings/CursorShape.cs)
- **Docs:** Add. Trivial.
- **Tests:** Round-trip tests already exist (OpenTKEnumMirrorTests.cs).

### 1.6 — Recent windowing types
- **Files:** [WindowResizeEventArgs.cs](BabyBearsEngine/Source/Windowing/WindowResizeEventArgs.cs), [WindowIcon.cs](BabyBearsEngine/Source/Windowing/WindowIcon.cs)
- **Docs:** Add. Trivial.
- **Tests:** None needed (data carriers).

### 1.7 — Boot path & settings records
- **Files:** [Runtime/Boot/GameLauncher.cs](BabyBearsEngine/Source/Runtime/Boot/GameLauncher.cs), [Runtime/Settings/ApplicationSettings.cs](BabyBearsEngine/Source/Runtime/Settings/ApplicationSettings.cs), [WindowSettings.cs](BabyBearsEngine/Source/Runtime/Settings/WindowSettings.cs), [GameLoopSettings.cs](BabyBearsEngine/Source/Runtime/Settings/GameLoopSettings.cs), [IoSettings.cs](BabyBearsEngine/Source/Runtime/Settings/IoSettings.cs), [ConsoleSettings.cs](BabyBearsEngine/Source/Runtime/Settings/ConsoleSettings.cs), [LogSettings.cs](BabyBearsEngine/Source/Runtime/Settings/LogSettings.cs)
- **Docs:** Add. Settings records are how devs configure the engine — high leverage.
- **Tests:** System test for `GameLauncher` exists; consider adding negative cases (re-init without `Reset`, null services, etc.) opportunistically.

---

## Tier 2 — Core building blocks

What devs actually compose their game from.

| # | Group | Notes |
|---|---|---|
| 2.1 | [World](BabyBearsEngine/Source/Worlds/World.cs), [IWorld](BabyBearsEngine/Source/Worlds/IWorld.cs) | Lifecycle is the spine. Tests exercise `Load`/`Update`/`Draw` with fake renderables. |
| 2.2 | [Entity](BabyBearsEngine/Source/Worlds/Entity.cs), [IEntity](BabyBearsEngine/Source/Worlds/IEntity.cs), [AddableBase](BabyBearsEngine/Source/Worlds/AddableBase.cs), [IAddable](BabyBearsEngine/Source/Worlds/IAddable.cs) | Base for every game entity. Add/Remove/lifecycle. |
| 2.3 | [Container](BabyBearsEngine/Source/Worlds/Container.cs), [IContainer](BabyBearsEngine/Source/Worlds/IContainer.cs), [ContainerEntity](BabyBearsEngine/Source/Worlds/ContainerEntity.cs), [ILayered](BabyBearsEngine/Source/Worlds/ILayered.cs), [LayerChangedEventArgs](BabyBearsEngine/Source/Worlds/LayerChangedEventArgs.cs) | Layering is non-trivial → tests worthwhile. |
| 2.4 | [GraphicBase](BabyBearsEngine/Source/Worlds/Graphics/GraphicBase.cs), [IRenderable](BabyBearsEngine/Source/Worlds/IRenderable.cs), [Image](BabyBearsEngine/Source/Worlds/Graphics/Image.cs), [ColouredRectangle](BabyBearsEngine/Source/Worlds/Graphics/ColouredRectangle.cs), [PointGraphic](BabyBearsEngine/Source/Worlds/Graphics/PointGraphic.cs) | Docs essential. Tests harder (touch GL) — defer or test only non-rendering behaviour. |
| 2.5 | [Matrix3](BabyBearsEngine/Source/Geometry/Matrix3.cs) | Appears in `IRenderable.Render` signature. Pure math → easy tests. |

---

## Tier 3 — Specialised but still public

| # | Group | Notes |
|---|---|---|
| 3.1 | [Camera](BabyBearsEngine/Source/Worlds/Cameras/Camera.cs), [CameraView](BabyBearsEngine/Source/Worlds/Cameras/CameraView.cs), [FreeCameraView](BabyBearsEngine/Source/Worlds/Cameras/FreeCameraView.cs), [FixedTileSizeCameraView](BabyBearsEngine/Source/Worlds/Cameras/FixedTileSizeCameraView.cs) | Common need; docs first, tests for view math. |
| 3.2 | [Button](BabyBearsEngine/Source/Worlds/UI/Button.cs), [Panel](BabyBearsEngine/Source/Worlds/UI/Panel.cs), [DraggablePanel](BabyBearsEngine/Source/Worlds/UI/DraggablePanel.cs) | UI components. Behaviour testable with fake input. |
| 3.3 | [Randomisation](BabyBearsEngine/Source/Utilities/Randomisation.cs), [Repeat](BabyBearsEngine/Source/Repeat.cs), [Ensure](BabyBearsEngine/Source/Ensure.cs), [ColourTools](BabyBearsEngine/Source/Utilities/ColourTools.cs) | Pure utilities; trivial docs and tests. |
| 3.4 | [Matrix2](BabyBearsEngine/Source/Geometry/Matrix2.cs), [Matrix4](BabyBearsEngine/Source/Geometry/Matrix4.cs), [Point3](BabyBearsEngine/Source/Geometry/Point3.cs), [Point4](BabyBearsEngine/Source/Geometry/Point4.cs) | Pure math. Skip if not used in public API often. |

---

## Tier 4 — Defer or audit visibility first

Don't invest until the design question is settled.

- **Text graphics** — [BMPTextGraphic](BabyBearsEngine/Source/Worlds/Graphics/Text/BMPTextGraphic.cs) still has placeholder/stub code (the `Color4.White` literals in `SetVerticesSimple`). Pipeline looks half-finished; don't lock in via tests yet.
- **Visibility audit candidates** — types showing as `public` in the survey but feeling internal:
  - [FontBitmapGenerator](BabyBearsEngine/Source/Worlds/Graphics/Text/FontBitmapGenerator.cs), [GeneratedFontStruct](BabyBearsEngine/Source/Worlds/Graphics/Text/GeneratedFontStruct.cs), [CharacterBitmapGenerator](BabyBearsEngine/Source/Worlds/Graphics/Text/CharacterBitmapGenerator.cs), [FontTextureCache](BabyBearsEngine/Source/Worlds/Graphics/Text/FontTextureCache.cs)
  - [CameraMSAAShader](BabyBearsEngine/Source/Worlds/Cameras/CameraMSAAShader.cs)
  - [Logger](BabyBearsEngine/Source/Diagnostics/Logger.cs), [FileLogger](BabyBearsEngine/Source/Diagnostics/FileLogger.cs), [FileLoggerProvider](BabyBearsEngine/Source/Diagnostics/FileLoggerProvider.cs)
  - [TwoDimensionalArrayConverter](BabyBearsEngine/Source/IO/Json/TwoDimensionalArrayConverter.cs)

Internalising shrinks the public surface.

---

## Pragmatic order

1. **Tier 1.5 + 1.6** — tiny doc work, one pass.
2. **Tier 1.7** — settings records docs (high leverage for devs).
3. **Tier 1.1 + 1.2** — Colour + Rect tests (pure logic, fast wins).
4. **Tier 4 audit** — visibility check; shrinks scope of the rest.
5. **Tier 1.3 + 1.4** — facades + interfaces (docs + first mockable tests).
6. Continue Tier 2 in order.
