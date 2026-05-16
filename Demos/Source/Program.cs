using System;
using BabyBearsEngine.Demos.Source.Demos.AnimationDemo;
using BabyBearsEngine.Demos.Source.Demos.BearSpinner3000;
using BabyBearsEngine.Demos.Source.Demos.CameraDemo;
using BabyBearsEngine.Demos.Source.Demos.ClickDemo;
using BabyBearsEngine.Demos.Source.Demos.ClickTheBearDemo;
using BabyBearsEngine.Demos.Source.Demos.GraphicDemo;
using BabyBearsEngine.Demos.Source.Demos.KeyboardDemo;
using BabyBearsEngine.Demos.Source.Demos.MouseDemo;
using BabyBearsEngine.Demos.Source.Demos.ShaderDemo;
using BabyBearsEngine.Demos.Source.Demos.TextDemo;
using BabyBearsEngine.Demos.Source.Demos.StencilDemo;
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

Func<World> menuFactory = null!;
Func<World> getMenu = () => menuFactory();

Func<World> textSubmenuFactory = null!;
Func<World> getTextSubmenu = () => textSubmenuFactory();

Func<World> uiSubmenuFactory = null!;
Func<World> getUISubmenu = () => uiSubmenuFactory();

Func<World> graphicsSubmenuFactory = null!;
Func<World> getGraphicsSubmenu = () => graphicsSubmenuFactory();

textSubmenuFactory = () => new MenuWorld(
[
    new("Text Demo",   () => new TextDemoWorld(getTextSubmenu)),
    new("Text Demo 2", () => new TextDemoWorld2(getTextSubmenu)),
], backFactory: getMenu);

uiSubmenuFactory = () => new MenuWorld(
[
    new("UI Demo",   () => new UIDemoWorld(getUISubmenu)),
    new("UI Demo 2", () => new UIDemoWorld2(getUISubmenu)),
], backFactory: getMenu);

graphicsSubmenuFactory = () => new MenuWorld(
[
    new("Animation Demo",    () => new AnimationDemoWorld(getGraphicsSubmenu)),
    new("Bear Spinner 3000", () => new BearSpinnerWorld(getGraphicsSubmenu)),
    new("Graphic Demo",      () => new GraphicDemoWorld(getGraphicsSubmenu)),
    new("Shader Demo",       () => new ShaderDemoWorld(getGraphicsSubmenu)),
    new("Stencil Demo",      () => new StencilDemoWorld(getGraphicsSubmenu)),
], backFactory: getMenu);

MenuEntry[] mainMenuEntries =
[
    new("Camera Demo",    () => new CameraDemoWorld(getMenu)),
    new("Click Demo",     () => new ClickDemoWorld(getMenu)),
    new("Click The Bear", () => new ClickTheBearDemoWorld(getMenu)),
    new("Keyboard Demo",  () => new KeyboardDemoWorld(getMenu)),
    new("Mouse Demo",     () => new MouseDemoWorld(getMenu)),
    new("Text",     getTextSubmenu,     MenuEntryStyle.Submenu),
    new("UI",       getUISubmenu,       MenuEntryStyle.Submenu),
    new("Graphics", getGraphicsSubmenu, MenuEntryStyle.Submenu),
];

menuFactory = () => new MenuWorld(mainMenuEntries);

GameLauncher.Run(appSettings, menuFactory);
