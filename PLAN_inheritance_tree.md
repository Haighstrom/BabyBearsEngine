# Inheritance tree refactor

Goal: unify the Entity and Graphics trees on a shared positional foundation,
match what BearsEngine and CoakEngine learned the hard way, and eliminate
the int-vs-float split.

## Target shape

```
IRect                                      (float X, Y, Width, Height + Right/Bottom/Centre)
IAddable                                   (existing)
IRectAddable : IAddable, IRect

IUpdateable                                (existing)
IRenderable : IAddable                     (existing — Render, Visible)
ILayered                                   (existing — Layer, LayerChanged)

IContainer                                 (existing)

IGraphic : IRenderable, ILayered, IRectAddable
    (Colour — geometry comes from IRect)
IEntity  : IRenderable, ILayered, IRectAddable, IUpdateable, IContainer
    (combiner — was IUpdateable + IRenderable + IAddable; gains layer + rect)

IMouseInteractable                         (existing)
```

Concrete bases:

```
AddableBase           : IAddable
AddableRectBase       : AddableBase, IRectAddable     (float X/Y/W/H + PositionChanged/SizeChanged events)
  ├ GraphicBase       : IGraphic   (abstract — adds Colour, Layer, layered render)
  │     ├ Image
  │     ├ ColouredRectangle
  │     ├ TextImage
  │     └ PointGraphic            (needs settable Colour to fully fit; small refactor)
  └ ContainerEntity   : IEntity, IContainer  (abstract — manages children, Layer)
        └ Entity                            (+ IMouseInteractable; clickable option)
            └ Button, Panel, DraggablePanel, Checkbox, ProgressBar, …
```

## Decisions

- **Float everywhere** for X/Y/Width/Height. `int` only buys grid-alignment-by-default, which isn't what callers usually want. Existing `int` call sites widen implicitly.
- **Int Layer** stays (matches BBE + CoakEngine; floats are overkill for sort keys).
- **`IRect` is minimal** — just X/Y/Width/Height plus computed `Right`/`Bottom`/`Centre`. Rectangle algebra (Intersects/Contains/Grow) stays on the `Rect` value type as extension or static helpers, NOT on the interface.
- **Layer not unified on AddableRectBase.** GraphicBase and ContainerEntity each implement `ILayered` independently. Slight duplication; revisit only if it bites.
- **PositionChanged / SizeChanged events** live on `AddableRectBase`. No listeners today, but cheap and useful for future layout/tooltips.
- **No `IDisposable` on `IAddable`.** Most addables don't own resources. Keep `IDisposable` on the concrete graphics that need it.

## Out of scope here

- Rectangle algebra refactor (Intersects/Contains/etc.). Future, if it's needed.
- `SimpleGraphic` (internal helper used by `BMPTextGraphic`). Stays as-is.
- Pulling Wave 1 UI work back into the rebased hierarchy beyond mechanical updates.

## Steps

1. New interfaces: `IRect`, `IRectAddable`.
2. New base: `AddableRectBase` (float X/Y/W/H, events).
3. Rebase `GraphicBase` on `AddableRectBase`; drop redundant X/Y/Width/Height from `Image`, `ColouredRectangle`, `TextImage`.
4. Bring `PointGraphic` in line: settable `Colour` + `IGraphic`.
5. Rebase `ContainerEntity` on `AddableRectBase`; drop redundant X/Y/Width/Height from `Entity`.
6. Drop `Camera`'s bespoke X/Y/Width/Height (now inherited).
7. Update `IGraphic`: geometry comes from `IRect`, simplifies the interface.
8. Update widgets (mechanical: `int → float` in ctors).
9. Update demos.
10. Fix namespace mismatches (`BabyBearsEngine.Graphics` files at `Worlds/Graphics/` → `BabyBearsEngine.Worlds.Graphics`; `BabyBearsEngine.Rendering.Graphics.Text` → `BabyBearsEngine.Worlds.Graphics.Text`).
11. Update tests (test stubs, type signatures).
12. Build, run unit + system tests.
