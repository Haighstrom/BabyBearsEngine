using System;
using BabyBearsEngine.Demos.Source.Demos.AnimationDemo;
using BabyBearsEngine.Demos.Source.Demos.BorderDemo;
using BabyBearsEngine.Demos.Source.Demos.BearSpinner3000;
using BabyBearsEngine.Demos.Source.Demos.CameraDemo;
using BabyBearsEngine.Demos.Source.Demos.ClickDemo;
using BabyBearsEngine.Demos.Source.Demos.ClickTheBearDemo;
using BabyBearsEngine.Demos.Source.Demos.IODemo;
using BabyBearsEngine.Demos.Source.Demos.GraphicDemo;
using BabyBearsEngine.Demos.Source.Demos.KeyboardDemo;
using BabyBearsEngine.Demos.Source.Demos.MouseDemo;
using BabyBearsEngine.Demos.Source.Demos.ShaderDemo;
using BabyBearsEngine.Demos.Source.Demos.TextDemo;
using BabyBearsEngine.Demos.Source.Demos.StencilDemo;
using BabyBearsEngine.Demos.Source.Demos.TweenDemo;
using BabyBearsEngine.Demos.Source.Demos.UIDemo;
using BabyBearsEngine.Demos.Source.Menu;

var appSettings = new ApplicationSettings()
{
    WindowSettings = new WindowSettings()
    {
        Width = 800,
        Height = 600,
        Title = "Bears",
    }
};

Func<World> mainMenuFactory = null!;
Func<World> getMainMenu = () => mainMenuFactory();

Func<World> textSubmenuFactory = null!;
Func<World> getTextSubmenu = () => textSubmenuFactory();

Func<World> uiSubmenuFactory = null!;
Func<World> getUISubmenu = () => uiSubmenuFactory();

Func<World> graphicsSubmenuFactory = null!;
Func<World> getGraphicsSubmenu = () => graphicsSubmenuFactory();

textSubmenuFactory = () => new MenuWorld(
[
    new("Text Demo",        () => new TextDemoWorld(getTextSubmenu)),
    new("Text Demo 2",      () => new TextDemoWorld2(getTextSubmenu)),
    new("Typewriter Demo",  () => new TypewriterDemoWorld(getTextSubmenu)),
    new("Inline Tags Demo", () => new InlineTagsDemoWorld(getTextSubmenu)),
], backFactory: getMainMenu);

uiSubmenuFactory = () => new MenuWorld(
[
    new("UI Demo",   () => new UIDemoWorld(getUISubmenu)),
    new("UI Demo 2", () => new UIDemoWorld2(getUISubmenu)),
], backFactory: getMainMenu);

graphicsSubmenuFactory = () => new MenuWorld(
[
    new("Animation Demo",    () => new AnimationDemoWorld(getGraphicsSubmenu)),
    new("Bear Spinner 3000", () => new BearSpinnerWorld(getGraphicsSubmenu)),
    new("Border Demo",       () => new BorderDemoWorld(getGraphicsSubmenu)),
    new("Graphic Demo",      () => new GraphicDemoWorld(getGraphicsSubmenu)),
    new("Shader Demo",       () => new ShaderDemoWorld(getGraphicsSubmenu)),
    new("Stencil Demo",      () => new StencilDemoWorld(getGraphicsSubmenu)),
], backFactory: getMainMenu);

MenuEntry[] mainMenuEntries =
[
    new("Camera Demo",    () => new CameraDemoWorld(getMainMenu)),
    new("Click Demo",     () => new ClickDemoWorld(getMainMenu)),
    new("Click The Bear", () => new ClickTheBearDemoWorld(getMainMenu)),
    new("IO Demo",        () => new IODemoWorld(getMainMenu)),
    new("Keyboard Demo",  () => new KeyboardDemoWorld(getMainMenu)),
    new("Mouse Demo",     () => new MouseDemoWorld(getMainMenu)),
    new("Tween Demo",    () => new TweenDemoWorld(getMainMenu)),
    new("Text",     getTextSubmenu,     MenuEntryStyle.Submenu),
    new("UI",       getUISubmenu,       MenuEntryStyle.Submenu),
    new("Graphics", getGraphicsSubmenu, MenuEntryStyle.Submenu),
];

mainMenuFactory = () => new MenuWorld(mainMenuEntries);

GameLauncher.Run(appSettings, mainMenuFactory);
