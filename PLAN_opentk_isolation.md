# OpenTK Isolation Plan

Implements **Bucket 2** from [RUNTIME_WIRING_ANALYSIS.md](RUNTIME_WIRING_ANALYSIS.md).

Goal: hide OpenTK from the public engine API. Game code consumes engine-owned types; OpenTK lives only inside the `Platform/OpenTK` and `Platform/OpenGL` layers. Mirror only types that surface in the public API — internal-only OpenTK use is fine.

[OpenTKMappings.cs](BabyBearsEngine/Source/Platform/OpenTK/OpenTKMappings.cs) is the canonical home for BBE↔OpenTK conversions (already used for `CursorShape`).

Each step is independently shippable; build + tests should pass after each.

---

## Step 1 — Survey the public API for OpenTK leakage

**Status:** Done

Produce the worklist. Grep `OpenTK\.` in non-`Platform/` source files, filter to public types/members, append findings to this plan as a checklist.

Known leaks to verify:

- [IKeyboard.cs](BabyBearsEngine/Source/Input/IKeyboard.cs) and [Keyboard.cs](BabyBearsEngine/Source/Input/Keyboard.cs) — `Keys` from `OpenTK.Windowing.GraphicsLibraryFramework`.
- [IMouse.cs](BabyBearsEngine/Source/Input/IMouse.cs) and [Mouse.cs](BabyBearsEngine/Source/Input/Mouse.cs) — `MouseButton` from `OpenTK.Windowing.GraphicsLibraryFramework`.
- [IWindow.cs](BabyBearsEngine/Source/Windowing/IWindow.cs) and [Window.cs](BabyBearsEngine/Source/Windowing/Window.cs) — `WindowBorder`, `WindowState`, `WindowIcon`, `ResizeEventArgs`.
- [WindowSettings.cs](BabyBearsEngine/Source/Runtime/Settings/WindowSettings.cs) — `WindowBorder`, `WindowState`, `WindowIcon`, `Box2i` (from `OpenTK.Mathematics`).
- Discovery candidates: text graphics (`ITextGraphic`, `GeneratedFontStruct`), `PointGraphic`, `Randomisation` — confirm whether their public surface exposes OpenTK.

Decide for each whether to mirror, internalise, or leave alone (e.g. internal-only).

---

## Step 2 — Mirror `MouseButton`

**Status:** Done

Smallest enum, fewest call sites — land first to validate the pattern.

- Create `BabyBearsEngine.Input.MouseButton` enum mirroring OpenTK's (Left, Middle, Right, Button1–8). Use the same int values so conversion is a cast.
- Add `ToOpenTK` / `ToBBE` helpers in `OpenTKMappings.cs`.
- Update [IMouse.cs](BabyBearsEngine/Source/Input/IMouse.cs) signatures and [OpenTKMouseAdapter.cs](BabyBearsEngine/Source/Platform/OpenTK/OpenTKMouseAdapter.cs) translations.
- Update consumers: `Mouse` static facade, demos, anywhere else.

---

## Step 3 — Mirror `Keys`

**Status:** Done

Same shape as Step 2, larger enum. Same int values as OpenTK's `Keys` (GLFW codes are stable) so conversion is a cast.

- Create `BabyBearsEngine.Input.Keys` enum.
- Mappings in `OpenTKMappings.cs`.
- Update [IKeyboard.cs](BabyBearsEngine/Source/Input/IKeyboard.cs), [OpenTKKeyboardAdapter.cs](BabyBearsEngine/Source/Platform/OpenTK/OpenTKKeyboardAdapter.cs), `Keyboard` static facade, demos.

---

## Step 4 — Mirror `WindowBorder` and `WindowState`

**Status:** Done

Two small enums consumed by both `IWindow` and `WindowSettings`.

- Create BBE `WindowBorder` (Hidden, Resizable, Fixed) and `WindowState` (Normal, Minimized, Maximized, Fullscreen) in [BabyBearsEngine/Source/Windowing/](BabyBearsEngine/Source/Windowing/).
- Mappings in `OpenTKMappings.cs`.
- Update `IWindow`, `WindowSettings`, `OpenTKWindowAdapter`, and the `OpenTKGameEngine` settings hand-off.

---

## Step 5 — Mirror `WindowIcon`

**Status:** Done

`WindowIcon` is a class carrying icon image data, not just an enum.

- Design a BBE `WindowIcon` type — accept either a file path or raw RGBA pixel data (width, height, byte[]).
- Mapping in `OpenTKMappings.cs` constructs the OpenTK `WindowIcon` at the adapter boundary.
- Update `IWindow.Icon`, `WindowSettings.Icon`, `OpenTKWindowAdapter`.

---

## Step 6 — Mirror the resize event payload

**Status:** Done

`IWindow.Resize` currently fires with `OpenTK.Windowing.Common.ResizeEventArgs`.

- Introduced engine-owned `BabyBearsEngine.WindowResizeEventArgs` (named EventArgs class with `Width`/`Height`) per the global C# rule against tuple-shaped event payloads.
- Updated `IWindow.Resize` and the static `Window.Resize` to `event Action<WindowResizeEventArgs>?`. Adapter (`OpenTKWindowAdapter`) subscribes once to the OpenTK `Resize` event in its constructor and re-raises a translated BBE event.
- Consumers updated: `BearSpinnerWorld`, `DefaultShaderProgram`, `PointShaderProgram`, `R8ChannelShaderProgram`. Inline `+=`/`-=` lambda pairs in the shader programs were replaced with named handler methods (the previous lambdas couldn't actually unsubscribe — incidental fix).

---

## Step 7 — Replace `Box2i` in `WindowSettings.Position`

**Status:** Done

`WindowSettings.Position` returned/accepted `OpenTK.Mathematics.Box2i`.

- Replaced with engine-owned [BabyBearsEngine.Geometry.Rect](BabyBearsEngine/Source/Geometry/Rect.cs). Used a `using Rect = ...` alias because `BabyBearsEngine.Geometry.Point` would clash with the file's existing `System.Drawing.Point` (used by `MaxClientSize`/`MinClientSize`).
- No external consumers of `WindowSettings.Position`, so no further updates required.
- `Rect` uses `float`; integer cast at the setter boundary preserves `WindowSettings`'s int storage.

---

## Step 8 — Address remaining `Color4` leaks

**Status:** In progress (8a done; 8b–8d pending)

Step 1 re-survey (after Steps 2–7) confirmed all remaining public-API leaks involve `OpenTK.Mathematics.Color4`. BBE already has its own [`Colour`](BabyBearsEngine/Source/Colour.cs) (`record struct` with byte RGBA) — migration is consumer-by-consumer.

Convention: inbound conversions (foreign → BBE) are named after the target type (`ToColour`, `ToKeys`, `ToMouseButton`, `ToWindowBorder`, `ToWindowState`, `ToWindowIcon`); outbound (BBE → OpenTK) keeps the foreign-library marker (`ToOpenTK`).

### 8a — Mappings + `Colour.ToOpenTK` visibility

**Status:** Done

- Renamed all `ToBBE` extensions in [OpenTKMappings.cs](BabyBearsEngine/Source/Platform/OpenTK/OpenTKMappings.cs) to typed equivalents (`ToKeys`, `ToMouseButton`, `ToWindowBorder`, `ToWindowState`, `ToWindowIcon`).
- Added `ToColour(this Color4)` for inbound conversion. Outbound stayed as the existing `Colour.ToOpenTK()` instance method to avoid forcing platform `using` directives into multiple call sites.
- `Colour.ToOpenTK()` changed from `public` to `internal` — visible only to platform/internal callers via existing `InternalsVisibleTo` setup. No external consumers found in Demos/Sandbox.
- Updated three callers in `OpenTKWindowAdapter` (`WindowBorder`/`Icon`/`WindowState` getters).

### 8b — `Randomisation.RandColour`

**Status:** Not started

[Randomisation.cs:64](BabyBearsEngine/Source/Utilities/Randomisation.cs#L64) — change `public static Color4 RandColour()` to return BBE `Colour`. Drop the `using OpenTK.Mathematics;` from the file. Trivial change; BBE `Colour(byte, byte, byte, byte)` ctor matches the existing call shape.

### 8c — `PointGraphic` ctor

**Status:** Not started

[PointGraphic.cs:12](BabyBearsEngine/Source/Worlds/Graphics/PointGraphic.cs#L12) — change ctor parameter from `Color4` to BBE `Colour`. The internal `VertexNoTexture` consumer of the colour stays as `Color4`; convert at the boundary using `Colour.ToOpenTK()`.

### 8d — `ITextGraphic` + `BMPTextGraphic` + `StbTrueTypeTextGraphic`

**Status:** Not started

[ITextGraphic.Colour](BabyBearsEngine/Source/Worlds/Graphics/Text/ITextGraphic.cs#L8) is `Color4`; both implementations follow suit. Change the interface property type to BBE `Colour`, store as `Colour` internally in each impl, convert with `ToOpenTK()` only when constructing `Vertex` instances. Single coordinated change because the interface forces both impls.

After 8b–8d, the BBE public surface should be entirely OpenTK-free.

---

## Out of scope

- Internal use of OpenTK inside `Platform/OpenTK/` and `Platform/OpenGL/` is fine — those layers exist precisely to talk OpenTK.
- Renderer abstraction (`IRenderer`) — separate concern, deferred.
- Threading an `IEngineContext` through `World.Load/Update/Draw` to remove static-facade reliance — separate concern, deferred.
