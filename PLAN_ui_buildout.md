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
- `TextImage`, `ColouredRectangle`, `Image`
- `IKeyboard` / `IMouse`

## Engine gaps to confirm or fill

These will block specific later widgets. Flag and resolve as we go — do not
pre-build infrastructure for them.

- **Theme bundle** — `Button` currently hardcodes hover/pressed deltas; a
  shared `UITheme` is needed before more widgets accumulate the same pattern.
- **Per-keypress / character input events** — required for `TextInputBox`.
  Need to confirm whether `IKeyboard` / the windowing layer surfaces
  `CharEntered` and `KeyDown` events or only polled state.
- **Render clipping / scissor** — required for any scrolling list. Confirm
  what the renderer exposes before starting `ScrollingListPanel`.
- **Render-on-top / topmost layer** — required for tooltips and open
  dropdowns so they paint over neighbouring entities.

## Wave 1 — foundations

Goal: extract a small amount of shared infrastructure before more widgets
accrete duplicated styling code.

### 1. `UITheme` (lite)

- New `BabyBearsEngine.Worlds.UI.Themes` namespace.
- Start with `ButtonTheme` (default / hover / pressed / disabled colours)
  and `TextTheme` (font definition, colour, alignment).
- Wrap both in a top-level `UITheme` so widgets take a single `theme`
  parameter. Grow the theme tree only as new widgets demand it — do not
  port the whole BearEngine theme structure up front.
- Refactor `Button` to consume `ButtonTheme` instead of computing hover /
  pressed colours from a base colour. Keep an overload that accepts a bare
  `Colour` and synthesises a theme for it, to avoid breaking existing
  callers.

### 2. `TextLabel`

- `Panel` + a centred `TextImage`.
- Takes a `TextTheme` and a string; exposes `Text`, `TextColour`.
- No new engine features required.

## Wave 2 — high-value widgets, no new engine features

Order within the wave is approximate; each is independent.

### 3. `Checkbox`

- Extends `Button`.
- Holds a tick graphic, toggles its visibility on click.
- `IsChecked`, `IsDisabled` properties; `Checked` / `Unchecked` events using
  named `EventArgs`.

### 4. `ProgressBar`

- Two stacked rectangles (background + fill) or a single rectangle scaled
  by `AmountFilled`.
- `AmountFilled` in [0, 1]; `BarFilled` event when it reaches 1.
- Optional follow-up: `ProgressBarTimer` that advances `AmountFilled` over a
  configured duration.

### 5. `SimpleToolTip`

- Hover-delay timer; on expiry, displays text near the cursor.
- **Depends on** the render-on-top mechanism — flag if missing and resolve
  in the engine before finishing this widget.

### 6. `CyclingValueButton<T>`

- Generic button that owns a `List<T>` and exposes a current value.
- Click cycles to the next entry; `ValueChanged` event with named
  `EventArgs` carrying old + new values.

## Wave 3 — needs new engine plumbing or composes Wave 2

Order matters here — earlier items unblock later ones.

### 7. `Scrollbar`

- Drag-thumb scrollbar reusing `DragController`.
- `AmountFilled` in [0, 1]; `ScrollbarDirection` (horizontal / vertical).
- Useful standalone; prerequisite for `ScrollingListPanel`.

### 8. `DropdownList<T>`

- Closed: shows current selection. Open: shows a list of `Button`-derived
  options.
- **Depends on** render-on-top so the open list paints over neighbouring
  entities.
- `SelectionChanged` event with named `EventArgs`.

### 9. `GridLayout`

- Layout container; sizes children by row/column rules
  (`CellSizing` / `CellSizingMode` / `GridAlignment` / `GridOrientation`).
- Build before `TabbedPanel` and `ScrollingListPanel` so their internals
  aren't full of manual rect maths.

### 10. `ScrollingListPanel`

- Vertical list of items + a `Scrollbar`.
- **Depends on** render clipping / scissor support.

### 11. `TextInputBox` / `NumberInputBox`

- Single-line editor with cursor, selection, char input.
- **Depends on** window `CharEntered` / `KeyDown` events. Confirm and (if
  needed) extend the input layer before starting.
- Optional follow-up: clipboard support (`Ctrl+C` / `Ctrl+V` / `Ctrl+X`).

### 12. `TabbedPanel` / `Tab` / `PagedPanel`

- Composite containers built on `Panel`, `Button`, and `GridLayout`.
- Tackle last; they're the heaviest and benefit most from the earlier
  primitives being stable.

### 13. `ValueDisplay` / `ValueDisplayWithButtons`

- Label + `+` / `−` buttons that bump a value.
- Trivial once the rest is in place; ship when something needs it.

## Out of scope (for now)

- `DragableUI` from BearEngine — already covered by BBE's `DraggablePanel`
  + `DragController`.
- `UIFactory` — defer until we have enough widgets that the constructor
  ergonomics start to hurt.
- `CollapseButton` — small, build on demand.

## Working agreements

- Each widget is a separate PR / commit; keep them small.
- Each widget gets at least a smoke test in the demo sandbox before being
  declared done.
- Follow the global C# rules (class-member layout, target-typed `new()`,
  explicit primitive defaults, named `EventArgs` for events with payloads,
  block braces).
- Don't pre-build theme entries for widgets that aren't being implemented
  yet — grow the `UITheme` tree as we go.
