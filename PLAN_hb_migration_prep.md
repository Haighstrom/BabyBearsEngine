# HappyBlacksmith → BBE migration: features to build first

What HappyBlacksmith uses from BearsEngine that BBE doesn't have. Build the
foundational items before starting the migration; polish items can be
filled in as needed.

Bootstrap/wiring (Program.cs, DI, world switching) is excluded — that's
straightforward porting work, not feature gaps.

## Confirmed: not needed

- **Audio.** No `.Audio.*` references in HB. BE has an audio module; HB doesn't use it. Skip.
- **Collision (`ICollideable`, `.Collides`).** Only one `MouseIntersecting` override in `Person.cs`. Trivial to hand-port; no engine-level need.

## Tier 1 — foundational (must land before migration starts)

### 1. Task / coroutine system

HB has **40+** Task subclasses (GoToTask, CarryItemTask, CraftAtAnvilTask, ChatTask, LookTask, …). This is the spine of all character behaviour and AI; nothing else in HB works without it.

BearsEngine shape (small — straight port):
- `ITask` — Active, IsComplete, NextTask, Start/Update/Complete/Reset, TaskStarted/TaskCompleted events.
- `Task` — concrete base; `ActionsOnStart`, `CompletionConditions`, `ActionsOnComplete` lists.
- `TaskGroup` — composes multiple tasks.
- `ITaskController` / `TaskController` — runs the current task on an entity each frame, advances `NextTask` chain when complete.

Cheap to port wholesale from `BearsEngine/Source/Tasks/`. Independent of any other engine subsystem.

### 2. Pathfinding

Grid-based with directional walls (HB has `HWNodeGrid` — horizontal/vertical wall edges between cells). Used for every character's movement.

BearsEngine shape:
- `IPathfindNode` / `PathfindNodeBase` / `PathfindNodeT<T>` — graph nodes.
- `IPathSolver` / `AStarSolver` / `RandPathSolver`.
- `GridPathfinder<T>` — grid-based wrapper.
- `IPathBuilder` / `PathBuilder` — runs a solver, produces a path.

Also a port from `BearsEngine/Source/Pathfinding/`. Standalone — depends on nothing from the rest of BE.

### 3. Sprite animation

Frame-stepped sprite-sheet animation. HB has PersonAnimation, CustomerAnimation, MeleeAttackAnimation, ArcheryShotAnimation, etc. — character visibility depends on this.

BearsEngine shape:
- `ISpriteTexture` / `SpriteTexture` — wraps a multi-cell texture with frame indexing.
- `ISprite` / `Sprite` — renders a single frame from a sprite texture.
- `IAnimation` / `Animation` — IUpdateable sprite that advances frames over time. `Play(loop)`, `AnimationComplete` event.
- `MultiLayerAnimation` — composite (e.g. body + clothes).
- `NumTween` — value tweening (for fades, moves).

Port from `BearsEngine/Source/Graphics/Animations/`. Depends on `Image` and `ITexture` (already in BBE).

### 4. Controller framework

HB leans on several controllers BE provides:
- `WaypointController` — walks an entity along a path from the pathfinder.
- `MoveFadeRemoveController` — translates an entity, fades alpha, removes when done (used for `Arrow`, `PopupText`).
- `YIndexedLayerController` — sets a sprite's layer from its Y coord so things further down render in front (depth sorting in a top-down view).

Small classes; port individually as needed. BBE already has `ClickController` / `DragController`, so the pattern is established.

## Tier 2 — game-layer features (needed for almost all HB code)

### 5. Tilemap / grid rendering

HB is grid-based (`FixedTileSizeCameraView` from BE). The world is laid out in tiles; entity positions are in tile-space, the camera scales to pixel-space.

BBE has `Camera.WithTileSize(...)` — looks like this **already works**. Verify by smoke-testing a tile-coord scene; if so, no work needed. If gaps emerge (e.g. tile-flood rendering helpers, grid lookup utilities), build them as discovered.

### 6. Save / load (JSON-based)

HB serialises:
- `PlayerSaveData` (levels unlocked, progress).
- Map data (rooms, doors, tile layouts).
- Hotkey settings.

Custom `JsonConverter`s (e.g. `ItemKeyJsonConverter`). Looks like plain `System.Text.Json` with project-specific converters — **no engine feature required**.

But: BBE should probably ship a minimal `IO/Json/` namespace with helpers (file-read+deserialise, file-write+serialise, common converters like `RectJsonConverter`/`ColourJsonConverter`/`PointJsonConverter`). Lightweight; build alongside the migration.

### 7. Text rendering enhancements

BBE has `TextImage` with HAlignment/VAlignment already. BE's `TextGraphic` adds:
- Multiline support (BBE's TextImage looks single-line only — verify).
- "Command tags" (inline colour/style markers) — used widely in HB? Worth checking before building.
- Font themes (font + colour + style bundle) — BBE has `TextTheme` already.

Probably just need to add **multiline support** to `TextImage` and confirm alignment edges work. Command tags can be deferred unless HB really depends on them.

## Tier 3 — UI gaps

### 8. `FillableBar`

Used for patience/health/progress displays. BBE has `ProgressBar` already, but BE's `FillableBar` is the raw graphic (just a fillable rectangle, no widget wrapping). Trivial — could even drop the distinction and use `ProgressBar` directly.

**Status: probably already covered by `ProgressBar`. Verify and tick off.**

### 9. Menu / popup framework

HB has `Menu`/`MenuOpener`, `CentrePopupWindow`, `SidePopupWindow`, an item-ordering menu, hotkey-driven overlays. Not in BBE.

A minimal version is small: a base `Popup` widget (probably extending `Panel`) with show/hide, positioning (centred, side-attached), and a close-on-click-outside or close-on-key behaviour. Build when migrating the first menu screen.

### 10. UI camera / overlay rendering

HB uses a `UICamera` separate from the game-world camera. BBE has the new `World.Overlay` we just built which serves the same purpose, *if* the overlay can host a Camera-like thing for HUD scaling. May want a `UICamera` convenience that's essentially a Camera in `world.Overlay`.

Try the existing `Overlay.Add(camera)` pattern first; only build a bespoke `UICamera` if it's awkward.

## Tier 4 — polish (defer until first migration milestone hits)

- **Effect entities** — `SpeechBalloon`, `PopupText`, `Arrow`, `Poof`. Each is a small `Entity` + `Animation` / `MoveFadeRemoveController`. Build on demand.
- **MSAA support on Camera** — BBE's Camera already takes `MsaaSamples`; verify the X2/X4 modes work. Visual-only.
- **Logging API parity** — HB uses `Log.Error(...)` etc. (BE static). BBE has `Logger`. Either add a `Log` static facade over `Logger`, or accept a `Log` → `Logger` rename during migration. Probably the latter — small mechanical change at HB call sites.
- **Hotkey/menu input refinements** — BBE's `IKeyboard` is polled. HB wires hotkeys via a `HotkeySettings` object. Probably just build a `Hotkey` helper in HB, no engine change.

## Roughly the right order

1. **Tasks** — port `BearsEngine.Tasks` directly. Adds new namespace `BabyBearsEngine.Tasks`.
2. **Pathfinding** — port `BearsEngine.Pathfinding`. New namespace `BabyBearsEngine.Pathfinding`.
3. **Sprite animation** — port `BearsEngine.Graphics.Animations` into BBE's existing `Worlds.Graphics`. Reuses BBE's `Image` / `ITexture`.
4. **Controllers** — port the three controllers HB uses (`WaypointController`, `MoveFadeRemoveController`, `YIndexedLayerController`) into `Worlds`.
5. **Tilemap smoke test** — verify `Camera.WithTileSize` is enough; build helpers only if needed.
6. **Save/load helpers** — add `IO/Json/` with a couple of common converters. Only when HB needs them.
7. **TextImage multiline** — small feature add when first multiline label appears.
8. **Menu/popup base** — build the first one when porting the first HB menu.

After step 4 you have everything load-bearing for HB. Steps 5–8 land naturally during the migration.

## Open questions / things to confirm before starting

- **Tile rendering**: smoke-test `Camera.WithTileSize` end-to-end and confirm HB's grid coordinate use translates cleanly.
- **Logging**: do you want a `Log` static facade for compatibility, or rename call sites?
- **Text command tags**: how widely does HB use them? May be a "skip entirely" item.
- **License / copy-paste**: BearsEngine and BabyBearsEngine are both yours, so direct ports are fine — confirming this so I don't tip-toe around copying code.
