# Render layering plan

## Status snapshot

The layering infrastructure is more complete than it first appears.
Everything below already exists and is tested:

- **`ILayered`** ([Source/Worlds/ILayered.cs](BabyBearsEngine/Source/Worlds/ILayered.cs)) — `int Layer { get; }`, `SetLayer(int)`, `LayerChanged` event. Documented contract: higher = behind, lower = on top, **Layer 0 is on top**, must be ≥ 0.
- **`LayerChangedEventArgs`** ([Source/Worlds/LayerChangedEventArgs.cs](BabyBearsEngine/Source/Worlds/LayerChangedEventArgs.cs)) — carries old + new layer.
- **`Container.InsertRenderable`** ([Source/Worlds/Container.cs:113](BabyBearsEngine/Source/Worlds/Container.cs#L113)) — inserts a renderable into `_graphics` sorted with **higher layers first** in the list.
- **`Container.OnLayerChanged`** — when a child's `Layer` changes, removes and reinserts it.
- **`Container.Add` / `Remove`** — wire up / tear down `LayerChanged` subscription.
- **`ContainerEntity.Render`** iterates `GetRenderables()` and calls `Render` on each, depth-first. Lower-layer items end up visually on top (later draws overwrite earlier ones in 2D).
- **`GraphicBase` and `ContainerEntity`** both implement `ILayered`.
- **Tests:** `ContainerTests.LayerChanged_AfterAdd_TriggersResort`, `Add_NonLayeredRenderable_TreatedAsLayerZero`, `Remove_LayeredRenderable_UnsubscribesFromLayerChanged`, plus `ContainerEntityTests.SetLayer_*`.

So the **mechanism works**. The "partial" feel comes from two things:
no widget in the engine actually calls `SetLayer`, and there's no story for
content that needs to render above unrelated entities (tooltips, dropdowns,
modals).

## Decisions

After discussion:

- **Convention stays.** Lower = on top, Layer 0 = default and on top.
  Mental model: depth from the camera. Documented in `ILayered`.
- **Unlayered = top stays.** `Container.NonLayeredRenderableLayer = 0` is
  the right default: if you opt out of layering, you "just want to be
  drawn", which means nothing covers you.
- **No negative layers.** The constraint stays at `≥ 0`. "Render on top of
  everything" is the `Overlay` use case below; there's no third case that
  needs negatives.
- **`IWorld.Overlay`** — a second `IContainer` on the world that's
  rendered as a separate pass after the main container, and updated in the
  same `Update` call. Callers explicitly opt in with `world.Overlay.Add(thing)`.
  Widgets stay ignorant — they're plain `IAddable`s; the caller picks the
  container.
- **`Layer { get; set; }` ergonomics.** Drop the separate `SetLayer(int)`
  method on `ILayered` and move the validation + event-firing into the
  property setter. Matches the style of `Visible`, `Active`, etc.
- **Tighten `ILayered` xmldoc** so the depth-from-camera model and the
  "unlayered = top" rationale are recorded in one authoritative place. No
  per-call-site comments — readers can read the interface.

## What `Overlay` actually buys

A separate render pass after the main container, guaranteed by the
structure of `World.Draw` rather than by add-order discipline.

Today, `world.Add(tooltip)` followed later by `world.Add(notification)`
would render the notification on top of the tooltip (same layer, later add
wins). With `world.Overlay.Add(tooltip)`, the notification — added to the
main container — renders first, then the overlay pass renders the tooltip
unconditionally on top.

If two things end up in the overlay, they fight each other by add order
within that pass. Overlay is a single extra pass, not a stack. That's
acceptable for the current use cases; collapse to "just add to world" if
it doesn't earn its keep.

## Work to land

In order. None of these is large.

### 1. `ILayered` becomes `Layer { get; set; }`

- Remove `SetLayer(int)` from the interface; replace with `int Layer { get; set; }`.
- The setter does the validation (`ArgumentOutOfRangeException.ThrowIfNegative`),
  records the old value, assigns, fires `LayerChanged` if it differs.
- Tighten the xmldoc to record the depth-from-camera model, the
  "unlayered = top" default, and "use `IWorld.Overlay` for above-everything
  content".

Touched: `ILayered.cs`, `GraphicBase.cs`, `ContainerEntity.cs`, the two
test stubs (`StubLayeredRenderable` in `ContainerTests.cs`, `StubRenderable`
in `ContainerBenchmarks.cs`), and the test call sites in
`ContainerEntityTests.cs`.

### 2. `IWorld.Overlay`

- Add `IContainer Overlay { get; }` to `IWorld`.
- In `World`, hold a second `Container` (sharing the world as its
  `realParent` so coords work the same) and expose it via the property.
- `World.Update` updates main first, then overlay.
- `World.Draw` renders the main pass first, then the overlay pass.

Touched: `IWorld.cs`, `World.cs`, `WorldTests.cs` (new tests for the
property, add/parent, and update-on-overlay).

### 3. Demo update

- `UIDemoWorld` switches `Add(tooltip)` to `Overlay.Add(tooltip)`. Same
  visible behaviour today, but it exercises the API and demonstrates the
  intent.

## Open questions

None — proceeding.
