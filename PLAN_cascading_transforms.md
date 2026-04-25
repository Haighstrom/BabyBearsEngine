# Plan: Cascading Draw Transforms

## Goal

Make rendering positions hierarchical: each `Entity` applies its `(X, Y)` to the inherited `modelView` matrix before passing it to children and owned graphics. Leaf graphics (`ColouredRectangle`, `Image`, `TextImage`) normalise their vertices to `(0, 0)` and express their local offset purely through the matrix.

---

## Current Architecture (Problems)

| Problem | Location |
|---------|----------|
| `ContainerEntity.Render` passes identical `modelView` to all children — no transform accumulation | `ContainerEntity.cs:31-42` |
| `ColouredRectangle.Vertices` bakes absolute `(x, y)` into vertex positions | `ColouredRectangle.cs:74-80` |
| `GraphicRenderer.GetVertices` bakes absolute `(x, y)` into vertex positions | `GraphicRenderer.cs:9-14` |
| `TextImage.SetVerticesSimple` bakes `X + x` / `Y + y` into vertex positions | `TextImage.cs:157-161` |
| `Panel._background` is constructed with world-space `(x, y, width, height)` | `Panel.cs:10` |
| `Button._rectangle` and `_textImage` constructed with world-space `(x, y, …)` | `Button.cs:12-13` |
| `Entity.PositionOnScreen` returns `Rect(X, Y, Width, Height)` — breaks hit-testing once X,Y become relative | `Entity.cs:43` |
| `AddableBase.Parent` is set to the internal `Container`, not the containing `Entity` — can't walk chain for world-space hit-test | `Container.cs:41` |
| `DragController.PositionChanged` emits absolute mouse-space coords; `DraggablePanel` assigns them directly to X,Y | `DragController.cs:50`, `DraggablePanel.cs:49-52` |

---

## Target Architecture

```
World.Draw  ──── identity MV ──►  Panel (X=100, Y=50)
                                    Entity.Render: MV = translate(identity, 100, 50)
                                    ├── Panel.RenderSelf: _background.Render(MV=(100,50))
                                    │     ColouredRectangle.Render: translate by own (0,0) → no-op
                                    │     vertices at (0,0)→(w,h); world pos = (100,50)✓
                                    └── ContainerEntity.Render children with MV=(100,50)
                                          Button (X=10, Y=10)
                                            Entity.Render: MV = translate(MV, 10, 10) = (110,60)
                                            Button.RenderSelf: _rectangle.Render(MV=(110,60))
                                              vertices at (0,0)→(w,h); world pos=(110,60)✓
```

---

## Step-by-Step Changes

### Step 1 — Fix `Container.Add` so `entity.Parent` is the containing `ContainerEntity`

**File:** `Container.cs`

Currently `Container.Add` calls `entity.SetParent(this)` where `this` is the internal `Container` wrapper. This breaks the parent-chain walk needed for world-space hit-testing.

Change `Container` to accept the real parent on construction (or as a parameter to `Add`):

```csharp
internal class Container(IContainer realParent) : IContainer
{
    public void Add(IAddable entity)
    {
        // ... existing list management unchanged ...
        entity.SetParent(realParent);
    }
    // Remove still calls entity.SetParent(null) — unchanged
}
```

Update `ContainerEntity` to supply `this` as the real parent:

```csharp
private readonly Container _container;

protected ContainerEntity()
{
    _container = new Container(this);
}
```

> **Why:** `Entity.PositionOnScreen` (Step 5) needs to walk up through parent `Entity` instances to accumulate the world-space offset. With the old code the chain is always `entity → Container (internal)` and terminates immediately.

---

### Step 2 — Normalise leaf-graphic vertices to `(0, 0)`

#### `ColouredRectangle.cs`

**`Vertices` property** — remove `x`/`y` offsets:
```csharp
return [
    new(width,  height, colorTK),   // top right
    new(width,  0,      colorTK),   // bottom right
    new(0,      height, colorTK),   // top left
    new(0,      0,      colorTK),   // bottom left
];
```

**`_verticesChanged`** — stop dirtying on `X`/`Y` changes; only `Width`, `Height`, and `Colour` matter.

**`Render`** — translate MV by `(X, Y)` before passing to the shader:
```csharp
public void Render(ref Matrix3 projection, ref Matrix3 modelView)
{
    _shader.Bind();
    _vertexDataBuffer.Bind();

    if (_verticesChanged)
    {
        _vertexDataBuffer.SetNewVertices(Vertices);
        _verticesChanged = false;
    }

    var mv = Matrix3.Translate(ref modelView, X, Y);
    _shader.SetProjectionMatrix(ref projection);
    _shader.SetModelViewMatrix(ref mv);

    GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
}
```

#### `GraphicRenderer.cs`

**`GetVertices`** — remove `x`/`y` parameters:
```csharp
private static Vertex[] GetVertices(float w, float h, OpenTK.Mathematics.Color4 colour) =>
[
    new(0, 0, colour, 0, 0),
    new(w, 0, colour, 1, 0),
    new(0, h, colour, 0, 1),
    new(w, h, colour, 1, 1),
];
```

**`UpdateVertices`** — drop `x`/`y` parameters:
```csharp
public void UpdateVertices(float w, float h, Colour colour)
{
    _vertexDataBuffer.SetNewVertices(GetVertices(w, h, colour.ToOpenTK()));
}
```

#### `Image.cs`

**`_verticesChanged`** — stop dirtying on `X`/`Y` changes.

**`Render`** — translate MV and express rotation centre relative to local `(0,0)`:
```csharp
public void Render(ref Matrix3 projection, ref Matrix3 modelView)
{
    if (_verticesChanged)
    {
        _graphicRenderer.UpdateVertices(width, height, _colour);
        _verticesChanged = false;
    }

    var mv = Matrix3.Translate(ref modelView, x, y);
    if (_angle != 0)
        mv = Matrix3.RotateAroundPoint(ref mv, _angle, width / 2, height / 2);

    _graphicRenderer.Render(ref projection, ref mv);
}
```

#### `TextImage.cs`

`SetVerticesSimple` currently bakes `X + x` and `Y + y` into vertex positions. Strip out the `X`/`Y` component — vertices should be relative to local `(0,0)`:

```csharp
// In SetVerticesSimple, replace X + x / Y + y with just x / y:
vertices.Add(
    GeometryHelper.QuadToTris(
        new Vertex(x,     y,     colorTK, source.Min.X, source.Min.Y),
        new Vertex(x + w, y,     colorTK, source.Max.X, source.Min.Y),
        new Vertex(x,     y + h, colorTK, source.Min.X, source.Max.Y),
        new Vertex(x + w, y + h, colorTK, source.Max.X, source.Max.Y)
    ));
```

`_verticesChanged` — stop dirtying on `X`/`Y` changes (alignment offsets `x`/`y` inside `SetVerticesSimple` remain local-relative and are unaffected).

**`Render`** — translate MV by `(X, Y)`:
```csharp
public void Render(ref Matrix3 projection, ref Matrix3 modelView)
{
    _shader.Bind();
    _vertexDataBuffer.Bind();
    _texture.Bind();

    if (_verticesChanged)
    {
        SetVerticesSimple();
        _vertexDataBuffer.SetNewVertices(Vertices);
        _verticesChanged = false;
    }

    var mv = Matrix3.Translate(ref modelView, _x, _y);
    if (_shader is IMVPShader mvpShader)
    {
        mvpShader.SetProjectionMatrix(ref projection);
        mvpShader.SetModelViewMatrix(ref mv);
    }

    GL.DrawArrays(PrimitiveType.TriangleStrip, 0, Vertices.Length);
}
```

---

### Step 3 — Add `Entity.Render` (sealed) + `RenderSelf` virtual hook

**File:** `Entity.cs`

`Entity` currently inherits `ContainerEntity.Render` directly. Add a sealed override that:
1. Translates `modelView` by `(X, Y)`.
2. Calls the new `RenderSelf` virtual method (for graphics owned directly by this entity).
3. Delegates to `base.Render` (i.e. `ContainerEntity.Render`) with the translated matrix to render container children.

```csharp
public sealed override void Render(ref Matrix3 projection, ref Matrix3 modelView)
{
    var mv = Matrix3.Translate(ref modelView, X, Y);
    RenderSelf(ref projection, ref mv);
    base.Render(ref projection, ref mv);
}

protected virtual void RenderSelf(ref Matrix3 projection, ref Matrix3 modelView) { }
```

> **Why sealed:** Prevents subclasses from accidentally bypassing the transform. Subclasses should override `RenderSelf`, not `Render`.

---

### Step 4 — Update `Panel` and `Button` to use `RenderSelf` and local `(0, 0)` graphics

#### `Panel.cs`

- Change `_background` to local origin `(0, 0, width, height)`.
- Replace the `Render` override with `RenderSelf`.

```csharp
public class Panel(int x, int y, int width, int height, Colour colour)
    : Entity(x, y, width, height)
{
    private readonly ColouredRectangle _background = new(colour, 0, 0, width, height);

    protected override void RenderSelf(ref Matrix3 projection, ref Matrix3 modelView)
    {
        _background.Render(ref projection, ref modelView);
    }
}
```

#### `Button.cs`

- Change `_rectangle` and `_textImage` to `(0, 0, width, height)`.
- Replace the `Render` override with `RenderSelf`.

```csharp
private readonly ColouredRectangle _rectangle  = new(colour, 0, 0, width, height);
private readonly TextImage         _textImage  = new(new FontDefinition("Times New Roman", 16), textToDisplay, Colour.Black, 0, 0, width, height);

protected override void RenderSelf(ref Matrix3 projection, ref Matrix3 modelView)
{
    _rectangle.Render(ref projection, ref modelView);
    _textImage.Render(ref projection, ref modelView);
}
```

---

### Step 5 — Fix `Entity.PositionOnScreen` to return world-space bounds

**File:** `Entity.cs`

With Step 1 in place, `entity.Parent` now points to the containing `ContainerEntity`. Walk up through parents that are `Entity` instances to accumulate the world offset:

```csharp
public Rect PositionOnScreen
{
    get
    {
        int wx = X, wy = Y;
        IContainer? p = Parent;
        while (p is Entity e)
        {
            wx += e.X;
            wy += e.Y;
            p = e.Parent;
        }
        return new(wx, wy, Width, Height);
    }
}
```

> **Why this is sufficient:** `ClickController` and `DragController` only use `PositionOnScreen` — they don't need to know about the intermediate parents themselves.

---

### Step 6 — `DraggablePanel` / `DragController` — note known limitation

`DragController.PositionChanged` emits absolute mouse-derived world-space coordinates, and `DraggablePanel.OnPositionChanged` assigns them directly to `X`/`Y`. **This only works correctly when `DraggablePanel` is a direct child of `World` (no parent `Entity` with an offset).** If nested, the assigned position would need to subtract the parent's accumulated world offset.

For now: document this as a limitation. If `DraggablePanel` ever needs to be nested, `DragController` should receive a `Func<(int, int)> getParentWorldOffset` delegate and subtract it from the emitted position.

---

## File Change Summary

| File | Changes |
|------|---------|
| `Container.cs` | Accept real parent `IContainer` on construction; use it in `SetParent` |
| `ContainerEntity.cs` | Pass `this` to `Container` constructor |
| `ColouredRectangle.cs` | Vertices at `(0,0)`; X/Y don't dirty buffer; translate MV in Render |
| `GraphicRenderer.cs` | `GetVertices` / `UpdateVertices` drop `x`/`y` parameters; vertices at `(0,0)` |
| `Image.cs` | Translate MV by `(x,y)` in Render; rotation centre relative to `(0,0)`; X/Y don't dirty buffer |
| `TextImage.cs` | Vertices relative to `(0,0)`; translate MV in Render; X/Y don't dirty buffer |
| `Entity.cs` | Add sealed `Render` (translates MV, calls `RenderSelf`, calls base); add `RenderSelf` virtual; update `PositionOnScreen` |
| `Panel.cs` | Background at `(0,0)`; use `RenderSelf` |
| `Button.cs` | Rectangle + text at `(0,0)`; use `RenderSelf` |
| `DraggablePanel.cs` | No code change; document the nested-panel limitation |

---

## Out-of-Scope / Not Changed

- `SimpleGraphic` — vertices are caller-supplied and have no X/Y concept; no change needed.
- `PointGraphic` — verify but likely the same "caller supplies vertices" pattern.
- `Camera.cs` — applies its own projection; check that it passes its own modelView untouched and is unaffected by this change.
- `World.Draw` — already starts with identity MV; no change needed.

---

## Risks and Open Questions

1. **Other Entity subclasses in Tests** — any test entity that currently overrides `Render` must be switched to `RenderSelf`. Grep for `override.*Render` in the test projects.
2. **`PointGraphic` / `BMPTextGraphic` / `StbTrueTypeTextGraphic`** — verify they don't bake absolute X/Y into vertices (or apply the same normalisation if they do).
3. **Drag in nested panel** — noted above; left for a follow-up.
4. **`Matrix3.Translate` produces a local copy** — confirmed OK; Translate already returns a new `Matrix3` and all Render methods take `ref` for the incoming value but create a local `mv` for the modified version.
