# BabyBearsEngine

A 2D game engine for .NET, built on [OpenTK](https://github.com/opentk/opentk) and OpenGL 4.5.

BabyBearsEngine gives you a `World` to fill with entities, a render loop driven by the GPU, a built-in UI toolkit (buttons, scrollbars, dropdowns, tabbed panels, input boxes…), text rendering with inline-tag styling, sprites and animations, tweening, pathfinding, mouse/keyboard input, logging, and JSON/XML serialisation — all from a single package, with no extra plumbing.

> **Status:** Pre-release (`0.1.0`). The public API is stable enough to build games against but may still shift between minor versions.

---

## Getting started

### Requirements

- .NET 10 SDK
- A GPU capable of OpenGL 4.5 (the requested context version is configurable via [`WindowSettings.OpenGLVersion`](BabyBearsEngine/Source/Runtime/Settings/WindowSettings.cs))

### Hello, Bear

```csharp
using BabyBearsEngine;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics.Text;

var settings = new ApplicationSettings
{
    WindowSettings = new WindowSettings
    {
        Width  = 800,
        Height = 600,
        Title  = "Hello, Bear",
    },
};

GameLauncher.Run(settings, () => new HelloWorld());

internal sealed class HelloWorld : World
{
    public HelloWorld()
    {
        BackgroundColour = new Colour(30, 30, 40);

        Add(new TextGraphic(
            new FontDefinition("Times New Roman", 32),
            "Hello, Bear!",
            Colour.White,
            x: 0, y: 250, w: 800, h: 100)
        {
            HAlignment = HAlignment.Centred,
            VAlignment = VAlignment.Centred,
        });
    }
}
```

That's the whole program. `GameLauncher.Run` opens the window, initialises OpenGL, and drives the main loop until the window closes.

### Running the demos

The repository ships with a [Demos project](Demos/) that shows every feature listed below. From the solution root:

```
dotnet run --project Demos
```

You'll get a menu with sub-menus for Text, UI, Graphics, Input, Camera, IO, Tweens, and a couple of small showcases (Bear Spinner 3000, Click The Bear).

---

## Core concepts

### `ApplicationSettings` — your one configuration object

Everything you configure at launch lives in [`ApplicationSettings`](BabyBearsEngine/Source/Runtime/Settings/ApplicationSettings.cs): window, game loop, logging, IO, diagnostics, and the optional debug console window. Every nested record has a `.Default` preset, so you only set what you want to change.

### `World` — your scene

A [`World`](BabyBearsEngine/Source/Worlds/World.cs) is the top-level container. You subclass it, `Add()` entities/graphics/UI to it, and optionally override `Update(double elapsed)` for per-frame logic. The engine handles the draw loop for you.

Switch between worlds at any time with `Engine.ChangeWorld(() => new NextWorld())`.

### `IUpdateable` / `IRenderable`

Anything you `Add()` to the world is automatically wired into the update and render loops if it implements [`IUpdateable`](BabyBearsEngine/Source/Worlds/IUpdateable.cs) and/or [`IRenderable`](BabyBearsEngine/Source/Worlds/IRenderable.cs). Most built-in graphics and UI controls do both.

---

## Key features

### Graphics

Sprites, animations, coloured rectangles, bordered rectangles, points, stencil-masked graphics, and text — all rendered via OpenGL 4.5 with optional MSAA per camera.

```csharp
// A static texture
Add(new TextureGraphic("bear.png", x: 100, y: 100, w: 64, h: 64));

// An animated sprite from a sprite map
var map = new SpriteMap("walk.png", frameWidth: 32, frameHeight: 32);
Add(new Animation(map, frameRate: 12, x: 200, y: 100));

// A solid colour quad
Add(new ColourGraphic(new Colour(60, 130, 220), x: 0, y: 0, w: 400, h: 4));
```

See [`Source/Worlds/Graphics/`](BabyBearsEngine/Source/Worlds/Graphics/) for the full surface.

### Text

Text rendering supports left/centre/right and top/centre/bottom alignment, wrapping, font swapping, decorations, a typewriter effect, and **inline tags** for changing colour, font, or size mid-string.

```csharp
Add(new TextGraphic(
    new FontDefinition("Times New Roman", 18),
    "Plain text, [c=red]red text[/c], and [size=28]bigger[/size] inline.",
    Colour.Black,
    x: 20, y: 20, w: 760, h: 60));
```

### UI

A full set of controls is included out of the box — see [`Source/Worlds/UI/`](BabyBearsEngine/Source/Worlds/UI/):

- `Button`, `Checkbox`, `CyclingValueButton`
- `ProgressBar`, `TimedProgressBar`
- `TextLabel`, `SimpleToolTip`
- `Scrollbar`, `ScrollingListPanel`
- `TabbedPanel`, `DropdownList`
- `GridLayout`, `Panel`, `DraggablePanel`
- `TextInputBox`, `NumberInputBox`

```csharp
Button play = new(
    x: 350, y: 250, w: 100, h: 40,
    ButtonTheme.FromColour(new Colour(80, 160, 80)),
    "Play");

play.LeftClicked += (_, _) => Engine.ChangeWorld(() => new GameWorld());
play.MouseEntered += (_, _) => play.Text = "Let's go!";

Add(play);
```

Mouse events distinguish enter/exit, press, click, double-click, and a 0.5s hover dwell. By default only the top-most control under the cursor receives a click; set `ClickThrough = true` to forward to lower controls.

### Input

Direct keyboard and mouse polling via [`IKeyboard`](BabyBearsEngine/Source/Input/IKeyboard.cs) and [`IMouse`](BabyBearsEngine/Source/Input/IMouse.cs), available from anywhere via `EngineConfiguration`. For UI you usually don't need this — the controls handle it themselves.

### Tweens & easings

```csharp
NumTween fade = new(from: 0, to: 255, duration: 2.0, easing: Easings.EaseInOutSine);
Add(fade);

// later, in Update:
sprite.Alpha = (byte)fade.Value;
```

Built-in easings include linear, quad, sine, bounce, and back variants (`EaseIn`/`EaseOut`/`EaseInOut`). `ColourTween` interpolates colour values directly. See [`Source/Worlds/Tweens/`](BabyBearsEngine/Source/Worlds/Tweens/).

### Cameras

Each world can host one or more cameras with independent viewports, zoom, and MSAA sample counts. Default MSAA is set globally on `ApplicationSettings.DefaultCameraMsaa` and clamped to whatever the GPU actually supports.

### Pathfinding

A grid-aware A\* implementation lives in [`Source/Pathfinding/`](BabyBearsEngine/Source/Pathfinding/), with pluggable `IPathSolver` (e.g. `AStarSolver`, `RandomPathSolver`) so you can swap strategies per use case.

### Tasks (timed actions)

Schedule one-shot or chained actions on the world via [`Source/Tasks/`](BabyBearsEngine/Source/Tasks/) — useful for cutscenes, AI behaviour sequences, or timed UI reveals.

### IO

[`Files`](BabyBearsEngine/Source/IO/Files.cs), [`Json`](BabyBearsEngine/Source/IO/Json.cs), and [`Xml`](BabyBearsEngine/Source/IO/Xml.cs) helpers cover read/write/copy with configurable retry behaviour for transient file-lock failures — handy for save games on Windows where AV scanners and cloud sync can briefly hold files open.

### Logging

A simple, sink-based logger with console and file sinks, configurable severity, and optional metadata (timestamp, thread, source). Configured through `ApplicationSettings.LogSettings`. See [`Source/Diagnostics/`](BabyBearsEngine/Source/Diagnostics/).

### Diagnostics

Frame capture (write the back buffer to disk each frame) is available via `DiagnosticsSettings.CaptureFrames` for debugging visual regressions. There's also an optional in-process debug console window on Windows.

---

## Project layout

```
BabyBearsEngine/         the engine library (this is what you reference)
Demos/                   runnable demo project showcasing every feature
Benchmarks/              BenchmarkDotNet performance suites
Sandbox/                 ad-hoc experiments
Tests/                   unit and system tests (MSTest)
```

---

## Building from source

```
git clone <repo-url>
cd BabyBearsEngine
dotnet build
dotnet test
dotnet run --project Demos
```

---

## License

Copyright © Simon Haigh 2026.
