# Camera Refactor Plan

Target file: `BabyBearsEngine/Source/Worlds/Cameras/Camera.cs`

The `Camera` class mixes scene-graph concerns, view/coordinate math, and a full OpenGL rendering pipeline. The steps below are ordered easiest to most involved.

---

## Step 1 — Split `Render` into private submethods

**Status:** Done

The `Render` method is ~113 lines doing three distinct phases. Extract each into a private method so `Render` becomes a short orchestrator.

- `UpdateVerticesIfNeeded()` — rebuilds the quad vertex array when width/height changes
- `RenderChildrenToFBO(bool msaaEnabled)` — binds FBO, clears, sets up ortho projection, iterates `GetRenderables()`; returns `(fboOrtho, identity)` matrices for use by the MSAA pass
- `BlitMSAAToShaderPass(ref Matrix3 fboOrtho, ref Matrix3 identity)` — MSAA resolve pass
- `CompositeOntoParent(ref Matrix3 projection, ref Matrix3 modelView, prevVP, previousFBO)` — restores parent FBO/viewport, binds shader-pass texture, draws camera quad

---

## Step 2 — Static factory methods instead of two public constructors

**Status:** Done

Replace the two public constructors with named static factories so the two modes are immediately obvious at the call site.

```csharp
Camera.WithTileSize(x, y, width, height, tileW, tileH, samples)
Camera.WithView(x, y, width, height, viewX, viewY, viewW, viewH, samples)
```

The private constructor chain stays; only the public surface changes.

---

## Step 3 — Extract a `CameraView` class

**Status:** Done

The following all relate to "which part of the world am I showing, and how do world coordinates map to screen coordinates?" — nothing to do with FBOs or shaders:

- `_viewX`, `_viewY`, `_viewW`, `_viewH`
- `_tileWidth`, `_tileHeight`
- `FixedTileSize`
- `TileWidth`, `TileHeight` properties (with their recalculation logic)
- `View` property and `ViewChanged` event
- `GetWindowCoordinates()`

Move these into a `CameraView` class. `Camera` owns an instance and delegates to it. This makes the coordinate math independently testable.

---

## Step 5 — Replace `CameraView` with a typed hierarchy

**Status:** Done

The single `CameraView` class used a `FixedTileSize` bool to branch between two mutually exclusive modes, leaving the wrong setters silently accessible. Replace it with an abstract base and two concrete types:

- `CameraView` (abstract) — `X`, `Y` (scroll, settable in both modes), `TileWidth`/`TileHeight` (abstract `{ get; set; }`), `WorldToLocal()`, `ViewChanged`
- `FixedTileSizeCameraView` — stores tile size; setting `TileWidth/Height` directly updates the stored value; `ViewWidth/Height` are read-only computed consequences. Stable across camera resizes: the same number of pixels per tile.
- `FreeCameraView` — stores view dimensions (`ViewWidth/Height`); setting `TileWidth/Height` does the inverse conversion (`viewW = camW / tileW`); tile size adjusts to fit. Stable across camera resizes: the same world area stays visible.

`Camera.View` returns the abstract `CameraView`. `FixedTileSize`, `TileWidth`/`TileHeight` delegate properties are removed from `Camera`. Common operations (`camera.View.TileWidth = 32`, `camera.View.X = playerX`) work via the base type with no cast required.

---

## Step 4 — Extract a `CameraRenderer` class

**Status:** Done

Everything OpenGL-related in `Camera` can move into a dedicated `CameraRenderer`:

- `_vertexBuffer`, `Vertices`, `_verticesChanged`
- `_msaaFBO`, `_shaderPassFBO`
- `_mSAAShader`, `_shader`
- The full `Render` body (already tidied up by Steps 1 and 3)

`Camera` would then be a pure scene-graph container — it holds a `CameraView`, calls `_renderer.Render(...)`, and has no direct OpenGL dependency.
