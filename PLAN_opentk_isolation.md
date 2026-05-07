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

**Status:** Not started

`IWindow.Resize` currently fires with `OpenTK.Windowing.Common.ResizeEventArgs`.

- Replace with an engine-owned event shape — simplest is `event Action<int, int>?` carrying `(width, height)`. If a richer payload is needed later, introduce `BabyBearsEngine.Windowing.WindowResizeEvent`.
- Update `IWindow`, the static `Window.Resize` event, and the adapter to translate OpenTK's event into the BBE one.

---

## Step 7 — Replace `Box2i` in `WindowSettings.Position`

**Status:** Not started

`WindowSettings.Position` returns/accepts `OpenTK.Mathematics.Box2i`.

- Either use `System.Drawing.Rectangle` (already used elsewhere in the file via `Point`), or introduce `BabyBearsEngine.Geometry.RectangleI` if a custom shape is preferable.
- Update `WindowSettings`.

---

## Step 8 — Address remaining leaks from the Step 1 survey

**Status:** Not started

Anything else discovered (text graphics, font generation, etc.) — handle case-by-case. Likely candidates are public API; resolution may be mirror, internalise, or leave alone if not user-facing.

---

## Out of scope

- Internal use of OpenTK inside `Platform/OpenTK/` and `Platform/OpenGL/` is fine — those layers exist precisely to talk OpenTK.
- Renderer abstraction (`IRenderer`) — separate concern, deferred.
- Threading an `IEngineContext` through `World.Load/Update/Draw` to remove static-facade reliance — separate concern, deferred.
