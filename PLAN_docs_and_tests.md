# Docs & Tests Plan

Prioritised work for adding XML doc comments to the public API and unit/system tests for BabyBearsEngine.

Caveat: tier assignments are based on a quick coverage survey; verify status (especially "internal vs public" calls) before committing each item. Some types listed as public may turn out to be internal (e.g. `FontBitmapGenerator`, `GeneratedFontStruct`).

---

## Tier 1 — Front door of the library

Foundational value types, recently-refactored surfaces, entry points. Stable enough to lock in.

### 1.1 — Colour
- **File:** [BabyBearsEngine/Source/Colour.cs](BabyBearsEngine/Source/Colour.cs)
- **Docs:** Done (full XML).
- **Tests:** Done. 28 tests in [Tests/Unit/ColourTests.cs](Tests/Unit/ColourTests.cs) covering constructors (incl. clamp behaviour), normalised components, equality, `WithAlpha`, `Darkened`/`Lightened`, `ToArgb`, `ToColor`, OpenTK round-trip, named-colour spot checks.

### 1.2 — Rect
- **File:** [BabyBearsEngine/Source/Geometry/Rect.cs](BabyBearsEngine/Source/Geometry/Rect.cs)
- **Docs:** Done. Added XML to all 8 constructors, all 5 operators, `ToString`, and beefed up the class summary. Fixed a doc typo (duplicate `<param name="originX">` → `originY` on `ResizeAround`).
- **Tests:** Done. 60 tests in [Tests/Unit/RectTests.cs](Tests/Unit/RectTests.cs) covering static factories, all 8 ctors, string ctor, edge accessors, area/size/min-max sides, P/corner/centre points, `Zeroed`, `Resize`, `Shift` (3 overloads), `Scale`/`ScaleAround`/`ScaleAroundCentre`, `ResizeAround`, `Grow` (uniform + per-side + negative), `Intersects` (with `touchingCounts`), `Intersection`, `Contains` (4 overloads incl. edge-flag combinations), `ToVertices`, equality + operators, `ToString`.
- **Bug found and fixed:** [Rect.cs:413](BabyBearsEngine/Source/Geometry/Rect.cs#L413) — `Contains(float x, float y, float w, float h)` used `X + H` (height) instead of `X + W` (width) for the right-edge check. Wrong results for non-square rects. Test `Contains_XYWH_OverflowingRight_ReturnsFalse` is the regression guard.
- **Round-trip fix:** `ToString` now produces `{X=1,Y=2,W=3,H=4}` (System.Drawing.Rectangle style, `InvariantCulture`, no gratuitous trailing zeros), and `Rect(string)` parses the same format strictly with `FormatException` on malformed input. The two now round-trip cleanly.

### 1.3 — Static facades (Window / Keyboard / Mouse)
- **Files:** [Windowing/Window.cs](BabyBearsEngine/Source/Windowing/Window.cs), [Input/Keyboard.cs](BabyBearsEngine/Source/Input/Keyboard.cs), [Input/Mouse.cs](BabyBearsEngine/Source/Input/Mouse.cs)
- **Docs:** Done. Class-level summary explaining the facade pattern, `<inheritdoc cref="..."/>` on every member to defer to the underlying interface (avoids duplication).
- **Tests:** Done — see 1.4 (combined into one piece of work).

### 1.4 — Platform interfaces (IWindow / IKeyboard / IMouse)
- **Files:** [Windowing/IWindow.cs](BabyBearsEngine/Source/Windowing/IWindow.cs), [Input/IKeyboard.cs](BabyBearsEngine/Source/Input/IKeyboard.cs), [Input/IMouse.cs](BabyBearsEngine/Source/Input/IMouse.cs)
- **Docs:** Done. Each interface has a class-level summary and per-member XML; the docs are the source of truth that the facades inherit from via `<inheritdoc>`.
- **Tests (covers 1.3):** Done. 59 tests across [WindowFacadeTests.cs](Tests/Unit/WindowFacadeTests.cs), [KeyboardFacadeTests.cs](Tests/Unit/KeyboardFacadeTests.cs), [MouseFacadeTests.cs](Tests/Unit/MouseFacadeTests.cs). Each test substitutes a fake via `EngineConfiguration.{Window,Keyboard,Mouse}Service`, and asserts: (a) every member routes through the installed service; (b) overload pairs (`IEnumerable<T>` vs `params T[]`) hit the right interface method; (c) the `not initialised` contract throws `InvalidOperationException`; (d) replacing the service after install routes new calls to the new instance; (e) `Window.Resize` event subscription/unsubscription correctly hooks the underlying event.

### 1.5 — Mirrored enums
- **Files:** [WindowBorder.cs](BabyBearsEngine/Source/Windowing/WindowBorder.cs), [WindowState.cs](BabyBearsEngine/Source/Windowing/WindowState.cs), [Keys.cs](BabyBearsEngine/Source/Input/Keys.cs), [MouseButton.cs](BabyBearsEngine/Source/Input/MouseButton.cs), [CursorShape.cs](BabyBearsEngine/Source/Runtime/Settings/CursorShape.cs)
- **Docs:** Done. `WindowBorder`, `WindowState`, `MouseButton`: class summary + per-value XML. `CursorShape`: class summary added (per-value docs were already present). `Keys`: class summary only — explains GLFW/OpenTK mirror, ASCII/keypad allocation, and the meaning of `Unknown`. Per-value docs skipped on `Keys` (120+ self-explanatory entries).
- **Tests:** Round-trip tests already exist (OpenTKEnumMirrorTests.cs).

### 1.6 — Recent windowing types
- **Files:** [WindowResizeEventArgs.cs](BabyBearsEngine/Source/Windowing/WindowResizeEventArgs.cs), [WindowIcon.cs](BabyBearsEngine/Source/Windowing/WindowIcon.cs)
- **Docs:** Done. Class summary + per-property XML. `WindowIcon` doc spells out the RGBA / row-major / `Width × Height × 4` byte layout contract that `Pixels` must follow.
- **Tests:** None needed (data carriers).

### 1.7 — Boot path & settings records
- **Files:** [Runtime/Boot/GameLauncher.cs](BabyBearsEngine/Source/Runtime/Boot/GameLauncher.cs), [Runtime/Settings/ApplicationSettings.cs](BabyBearsEngine/Source/Runtime/Settings/ApplicationSettings.cs), [WindowSettings.cs](BabyBearsEngine/Source/Runtime/Settings/WindowSettings.cs), [GameLoopSettings.cs](BabyBearsEngine/Source/Runtime/Settings/GameLoopSettings.cs), [IoSettings.cs](BabyBearsEngine/Source/Runtime/Settings/IoSettings.cs), [ConsoleSettings.cs](BabyBearsEngine/Source/Runtime/Settings/ConsoleSettings.cs), [LogSettings.cs](BabyBearsEngine/Source/Runtime/Settings/LogSettings.cs)
- **Docs:** Add. Settings records are how devs configure the engine — high leverage.
- **Tests:** System test for `GameLauncher` exists; consider adding negative cases (re-init without `Reset`, null services, etc.) opportunistically.

---

## Tier 2 — Core building blocks

**Status:** Done (docs everywhere; tests for everything that doesn't require booting OpenGL).

| # | Group | Status |
|---|---|---|
| 2.1 | [World](BabyBearsEngine/Source/Worlds/World.cs), [IWorld](BabyBearsEngine/Source/Worlds/IWorld.cs) | Docs done (interface lifecycle, World subclass guidance). Tests done — 11 tests in [WorldTests.cs](Tests/Unit/WorldTests.cs) covering defaults, Load/Unload, GetWindowCoordinates, Add/Remove/RemoveAll, Update. **Skipped:** `World.Draw` (touches `GL.*` and `Window.*` — needs a booted engine). Flagged: `World.Update` does NOT skip inactive children, unlike `ContainerEntity.Update` — current behaviour pinned by test. |
| 2.2 | [Entity](BabyBearsEngine/Source/Worlds/Entity.cs), [IEntity](BabyBearsEngine/Source/Worlds/IEntity.cs), [AddableBase](BabyBearsEngine/Source/Worlds/AddableBase.cs), [IAddable](BabyBearsEngine/Source/Worlds/IAddable.cs) | Docs done. Tests: 7 in [AddableBaseTests.cs](Tests/Unit/AddableBaseTests.cs) (initial state, attach/detach, throw conditions, Remove delegation), 6 in [EntityTests.cs](Tests/Unit/EntityTests.cs) (ctor stores fields, PositionOnScreen, GetWindowCoordinates with/without parent, Render applies translation). **Skipped:** `clickable: true` path (uses `Mouse` static + `ClickController` — testable later when input fakes are in place). |
| 2.3 | [Container](BabyBearsEngine/Source/Worlds/Container.cs) (internal), [IContainer](BabyBearsEngine/Source/Worlds/IContainer.cs), [ContainerEntity](BabyBearsEngine/Source/Worlds/ContainerEntity.cs), [ILayered](BabyBearsEngine/Source/Worlds/ILayered.cs), [LayerChangedEventArgs](BabyBearsEngine/Source/Worlds/LayerChangedEventArgs.cs) | Docs done. Tests: 15 in [ContainerTests.cs](Tests/Unit/ContainerTests.cs) covering Add/Remove/RemoveAll throw conditions, parent setting, role detection (IUpdateable/IRenderable/ILayered), layer-sorted insertion, LayerChanged-driven resort, unsubscribe-on-remove, GetWindowCoordinates delegation. 14 in [ContainerEntityTests.cs](Tests/Unit/ContainerEntityTests.cs) covering defaults, SetLayer (incl. no-op-on-same-value, throw-on-negative), Update/Render skip inactive/invisible, container delegation, GetWindowCoordinates chaining. 1 in [LayerChangedEventArgsTests.cs](Tests/Unit/LayerChangedEventArgsTests.cs). |
| 2.4 | [GraphicBase](BabyBearsEngine/Source/Worlds/Graphics/GraphicBase.cs), [IRenderable](BabyBearsEngine/Source/Worlds/IRenderable.cs), [Image](BabyBearsEngine/Source/Worlds/Graphics/Image.cs), [ColouredRectangle](BabyBearsEngine/Source/Worlds/Graphics/ColouredRectangle.cs), [PointGraphic](BabyBearsEngine/Source/Worlds/Graphics/PointGraphic.cs) | **Docs only** (per the decision to skip GL-bound tests). Each ctor doc warns construction allocates GL resources and must run after the GL context exists. Tests deferred. |
| 2.5 | [Matrix3](BabyBearsEngine/Source/Geometry/Matrix3.cs) | Docs done — class summary explains column-major layout + composition order; per-member XML on every static factory and helper. Tests: 38 in [Matrix3Tests.cs](Tests/Unit/Matrix3Tests.cs) covering identity/zero/flip statics, all factories (Translation/Rotation/Scale/Ortho/FBOOrtho), Add/Subtract, all Multiply overloads (matrix×matrix, matrix×scalar, matrix×Point, matrix×Point3), Translate/Rotate/Scale composition helpers, FlipX/FlipY, Determinant, Inverse (incl. round-trip), Transpose, all operators. |

### Bugs found and fixed during Tier 2

- **`Matrix3.Identity` (and `Zero`, `FlipXMatrix`, `FlipYMatrix`) were static fields, not properties.** `Matrix3` contains a `float[] _values` reference; struct-copy doesn't deep-copy the array, so any mutation via the indexer setter on a "copy" of `Identity` corrupted the static for the rest of the process. My `Indexer_Setter_UpdatesValue` test surfaced this. Fixed by changing the four statics to properties (each access yields a fresh instance with its own array). Required updating `FlipX`/`FlipY` to use locals because the `ref` calls inside them no longer apply to a property.

Open Tier 2 follow-ups are tracked in [FOLLOWUPS.md](FOLLOWUPS.md) (Matrix3 deeper struct-copy issue, World.Update inactive-skip inconsistency).

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
