# UI build-out plan

Building out the `BabyBearsEngine.Worlds.UI` namespace, modelled on the
older `BearEngine/Source/UI` library but trimmed to what's actually useful.

## Starting state

Already in BBE:
- `Button` — hardcoded hover/pressed colour deltas
- `Panel` — coloured-rectangle background
- `DraggablePanel` — Panel + DragController

Already in BBE that the new widgets will lean on:
- `Entity` with click/hover hooks, `Container`, `DragController`
- `TextImage`, `ColouredRectangle`, `Image` (the latter two both expose a
  settable `Colour` property; `Image.Colour` is documented as a tint
  multiplier over the texture sample)
- `IKeyboard` / `IMouse`

## Theme design (decided)

The look of widgets is driven by **component-level themes**, not a single
top-level `UITheme`.

- **No engine-supplied bundle.** The engine ships `ButtonTheme`,
  `TextTheme`, etc., as separate types. If a game wants to group them
  (e.g. `static class GameThemes { ButtonTheme Primary; ... }`) it does so
  itself. This avoids the BearEngine ambiguity of "which `TextTheme`
  applies — the top-level one or the nested one?".
- **Themes are `sealed record`** so `with`-expressions compose them cheaply
  (`Primary with { Text = HeadingText }`).
- **Themes are mandatory** for any widget that has multi-state styling
  (Button, Checkbox, Scrollbar, InputBox, …). No theme-less ctor; the
  convenience case goes through the theme's static factories, not a second
  ctor shape.
- **Single-state widgets don't need a theme.** `Panel`, label-style
  backgrounds, etc., take a `Colour` or `ITexture` directly. A
  `PanelTheme` with one field is bureaucracy.
- **Each theme provides static factories**: at minimum `Default`,
  `FromColour(Colour)`, `FromTexture(ITexture)`, and
  `FromGraphic(Func<Rect, IGraphic>)`. Defaults are deliberately bland
  (mid-grey, plain text) so prototype UI looks like prototype UI.
- **Themes hold a graphic *factory*, not a graphic.** An `ITexture` is a
  shareable resource, but a rendered `IGraphic` carries its own
  position/size — so a `ButtonTheme` used by two buttons must produce a
  fresh background graphic per widget. Internal shape:
  `Func<Rect, IGraphic> BackgroundFactory`.
- **Multi-state colours are tints** applied over the background graphic's
  `Colour` property. Works uniformly for `FromColour` (tinting a
  `ColouredRectangle` is the colour change), `FromTexture` (tinting an
  `Image` multiplies the texture sample), and `FromGraphic` (any
  `IGraphic` that exposes a settable `Colour`).
- **Widgets take exactly one ctor shape** for their background:
  `(geometry, theme)`. No optional `ITexture` / `IGraphic` parameter — if
  a caller wants a custom background, they construct the appropriate
  theme.

### Concrete `ButtonTheme` sketch

```csharp
public sealed record ButtonTheme
{
    public required Colour Default  { get; init; }
    public required Colour Hover    { get; init; }
    public required Colour Pressed  { get; init; }
    public required Colour Disabled { get; init; }
    public required TextTheme Text  { get; init; }

    internal Func<Rect, IGraphic> BackgroundFactory { get; init; } = default!;

    public static readonly ButtonTheme Default = /* bland grey */;

    public static ButtonTheme FromColour(Colour c) => /* ... */;
    public static ButtonTheme FromTexture(ITexture t) => /* ... */;
    public static ButtonTheme FromGraphic(Func<Rect, IGraphic> factory) => /* ... */;
}
```

Call sites:

```csharp
Button quick   = new(x, y, w, h, ButtonTheme.Default) { Text = "OK" };
Button primary = new(x, y, w, h, GameThemes.Primary) { Text = "Save" };
Button textured = new(x, y, w, h, ButtonTheme.FromTexture(buttonTex)) { Text = "Go" };
```

## Engine gaps to confirm or fill

These will block specific later widgets. Flag and resolve as we go — do
not pre-build infrastructure for them.

- **Tintable-graphic interface.** Both `Image` and `ColouredRectangle`
  already expose `Colour { get; set; }`, but there is no shared interface
  for "a graphic the widget can tint". Introduce a small interface
  (working name `ITintable` or similar — exposing `Colour { get; set; }`)
  and have both types implement it. The `BackgroundFactory` returns this
  type. Wave 1 sub-task.
- **Per-keypress / character input events** — required for `TextInputBox`.
  Confirm whether the windowing layer surfaces `CharEntered` and `KeyDown`
  events or only polled state. Wave 3 prerequisite.
- **Render clipping / scissor** — required for any scrolling list.
  Confirm what the renderer exposes before starting `ScrollingListPanel`.
  Wave 3 prerequisite.
- **Render-on-top / topmost layer** — required for tooltips and open
  dropdowns so they paint over neighbouring entities. Wave 2/3
  prerequisite (first needed for `SimpleToolTip`).

## Wave 1 — foundations

Goal: lock in the theme model and remove the hardcoded styling from the
existing `Button`. Everything in this wave is small.

### 1. Tintable-graphic interface

- Add the interface that exposes `Colour { get; set; }` (and any other
  surface a background graphic needs).
- Implement it on `Image` and `ColouredRectangle`.
- This is what `Func<Rect, IGraphic>` actually returns in the theme — i.e.
  the factory's return type is this interface, not bare `IAddable`.

### 2. `TextTheme`

- `sealed record` with font definition, colour, alignment fields.
- `static readonly Default` (system-ish font, black, centred).
- Used by `ButtonTheme.Text`, by `TextLabel`, and anywhere else text is
  drawn through a widget.

### 3. `ButtonTheme`

- `sealed record` per the sketch above.
- Static factories: `Default`, `FromColour`, `FromTexture`, `FromGraphic`.
- Defaults are intentionally bland.

### 4. Refactor `Button`

- One ctor: `(int x, int y, int w, int h, ButtonTheme theme)`.
- No optional `ITexture` / `IGraphic` parameter.
- `Text` becomes an object-initializer property (not a ctor arg).
- `Disabled` becomes an object-initializer/runtime property; theme's
  `Disabled` colour applied when set.
- Hover/pressed/disabled state changes apply the theme's tint colours via
  the tintable interface — no more in-class colour deltas.

### 5. `TextLabel`

- Ctor: `(int x, int y, int w, int h, TextTheme theme, string text)`.
  Text is fundamental to the widget, so it's required, not optional.
- No background graphic (or: takes an optional `ITexture` / `Colour`
  directly as it's a single-state widget — TBD when we get there).

## Wave 2 — high-value widgets, no new engine features

Order within the wave is approximate; each is independent. All follow the
Wave 1 theme conventions (own theme type if multi-state, primitives
directly if single-state).

### 6. `Checkbox`

- Extends `Button` (or uses `CheckboxTheme : ButtonTheme`-ish — decide
  when we get there).
- Holds a tick graphic, toggles its visibility on click.
- `IsChecked`, `IsDisabled` properties; `Checked` / `Unchecked` events
  using named `EventArgs`.

### 7. `ProgressBar`

- Two stacked rectangles (background + fill) or a single rectangle scaled
  by `AmountFilled`.
- `ProgressBarTheme` for background + fill colours/textures.
- `AmountFilled` in [0, 1]; `BarFilled` event when it reaches 1.
- Optional follow-up: `ProgressBarTimer` that advances `AmountFilled`
  over a configured duration.

### 8. `SimpleToolTip`

- Hover-delay timer; on expiry, displays text near the cursor.
- **Depends on** the render-on-top mechanism — flag if missing and resolve
  in the engine before finishing this widget.

### 9. `CyclingValueButton<T>`

- Generic button that owns a `List<T>` and exposes a current value.
- Click cycles to the next entry; `ValueChanged` event with named
  `EventArgs` carrying old + new values.

## Wave 3 — needs new engine plumbing or composes Wave 2

Order matters here — earlier items unblock later ones.

### 10. `Scrollbar`

- Drag-thumb scrollbar reusing `DragController`.
- `ScrollbarTheme` (track / thumb states).
- `AmountFilled` in [0, 1]; `ScrollbarDirection` (horizontal / vertical).
- Useful standalone; prerequisite for `ScrollingListPanel`.

### 11. `DropdownList<T>`

- Closed: shows current selection. Open: shows a list of `Button`-derived
  options.
- **Depends on** render-on-top so the open list paints over neighbouring
  entities.
- `SelectionChanged` event with named `EventArgs`.

### 12. `GridLayout`

- Layout container; sizes children by row/column rules
  (`CellSizing` / `CellSizingMode` / `GridAlignment` / `GridOrientation`).
- Build before `TabbedPanel` and `ScrollingListPanel` so their internals
  aren't full of manual rect maths.

### 13. `ScrollingListPanel`

- Vertical list of items + a `Scrollbar`.
- **Depends on** render clipping / scissor support.

### 14. `TextInputBox` / `NumberInputBox`

- Single-line editor with cursor, selection, char input.
- `InputBoxTheme` (background, cursor, selection, text state colours).
- **Depends on** window `CharEntered` / `KeyDown` events. Confirm and (if
  needed) extend the input layer before starting.
- Optional follow-up: clipboard support (`Ctrl+C` / `Ctrl+V` / `Ctrl+X`).

### 15. `TabbedPanel` / `Tab` / `PagedPanel`

- Composite containers built on `Panel`, `Button`, and `GridLayout`.
- Tackle last; they're the heaviest and benefit most from the earlier
  primitives being stable.

### 16. `ValueDisplay` / `ValueDisplayWithButtons`

- Label + `+` / `−` buttons that bump a value.
- Trivial once the rest is in place; ship when something needs it.

## Out of scope (for now)

- `DragableUI` from BearEngine — already covered by BBE's `DraggablePanel`
  + `DragController`.
- `UIFactory` — defer until we have enough widgets that the constructor
  ergonomics start to hurt. With theme-driven construction the call
  sites are already short, so this may never be needed.
- `CollapseButton` — small, build on demand.

## Working agreements

- Each widget / theme is a separate PR / commit; keep them small.
- Each widget gets at least a smoke test in the demo sandbox before
  being declared done.
- Follow the global C# rules (class-member layout, target-typed `new()`,
  explicit primitive defaults, named `EventArgs` for events with
  payloads, block braces).
- Themes are `sealed record`, with mandatory fields via `required init`.
- Widget ctors take `(geometry, theme)` for multi-state widgets, or
  `(geometry, primitive)` for single-state widgets. No optional
  `IGraphic` / `ITexture` parameters — custom graphics go through
  `Theme.FromGraphic` / `Theme.FromTexture`.
- Don't pre-build theme entries for widgets that aren't being
  implemented yet — grow the theme tree as we go.
