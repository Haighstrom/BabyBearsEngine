using System;
using BabyBearsEngine.Demos.Source.Demos.AnimationDemo;
using BabyBearsEngine.Demos.Source.Demos.BorderDemo;
using BabyBearsEngine.Demos.Source.Demos.BearSpinner3000;
using BabyBearsEngine.Demos.Source.Demos.CameraDemo;
using BabyBearsEngine.Demos.Source.Demos.ClickDemo;
using BabyBearsEngine.Demos.Source.Demos.ClickTheBearDemo;
using BabyBearsEngine.Demos.Source.Demos.IODemo;
using BabyBearsEngine.Demos.Source.Demos.KeyboardDemo;
using BabyBearsEngine.Demos.Source.Demos.MouseDemo;
using BabyBearsEngine.Demos.Source.Demos.ShaderDemo;
using BabyBearsEngine.Demos.Source.Demos.TextDemo;
using BabyBearsEngine.Demos.Source.Demos.StencilDemo;
using BabyBearsEngine.Demos.Source.Demos.TweenDemo;
using BabyBearsEngine.Demos.Source.Demos.ScrollListDemo;
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

Func<World> inputSubmenuFactory = null!;
Func<World> getInputSubmenu = () => inputSubmenuFactory();

Func<World> showcasesSubmenuFactory = null!;
Func<World> getShowcasesSubmenu = () => showcasesSubmenuFactory();

textSubmenuFactory = () => new MenuWorld(
[
    new("Text Wrapping",    () => new TextWrappingDemoWorld(getTextSubmenu)),
    new("Text Decorations", () => new TextDecorationsDemoWorld(getTextSubmenu)),
    new("Font Swapping",    () => new FontSwappingDemoWorld(getTextSubmenu)),
    new("Typewriter Demo",  () => new TypewriterDemoWorld(getTextSubmenu)),
    new("Inline Tags Demo", () => new InlineTagsDemoWorld(getTextSubmenu)),
    new("Rendering Quality", () => new TextRenderingQualityDemoWorld(getTextSubmenu)),
], backFactory: getMainMenu);

uiSubmenuFactory = () => new MenuWorld(
[
    new("Buttons",           () => new ClickDemoWorld(getUISubmenu)),
    new("Checkbox",          () => new CheckboxDemoWorld(getUISubmenu)),
    new("Cycling Button",    () => new CyclingButtonDemoWorld(getUISubmenu)),
    new("Progress Bars",     () => new ProgressBarDemoWorld(getUISubmenu)),
    new("TextLabel & Tooltip", () => new TextLabelDemoWorld(getUISubmenu)),
    new("Scrollbars",        () => new ScrollbarDemoWorld(getUISubmenu)),
    new("Tabbed Panel",      () => new TabbedPanelDemoWorld(getUISubmenu)),
    new("Dropdown",          () => new DropdownDemoWorld(getUISubmenu)),
    new("Scroll List",       () => new ScrollListDemoWorld(getUISubmenu)),
    new("Grid Layout",       () => new GridLayoutDemoWorld(getUISubmenu)),
    new("Input Boxes",       () => new InputBoxDemoWorld(getUISubmenu)),
], backFactory: getMainMenu);

graphicsSubmenuFactory = () => new MenuWorld(
[
    new("Animation", () => new AnimationDemoWorld(getGraphicsSubmenu)),
    new("Border",    () => new BorderDemoWorld(getGraphicsSubmenu)),
    new("Shader",    () => new ShaderDemoWorld(getGraphicsSubmenu)),
    new("Stencil",   () => new StencilDemoWorld(getGraphicsSubmenu)),
], backFactory: getMainMenu);

inputSubmenuFactory = () => new MenuWorld(
[
    new("Keyboard Demo", () => new KeyboardDemoWorld(getInputSubmenu)),
    new("Mouse Demo",    () => new MouseDemoWorld(getInputSubmenu)),
], backFactory: getMainMenu);

showcasesSubmenuFactory = () => new MenuWorld(
[
    new("Bear Spinner 3000", () => new BearSpinnerWorld(getShowcasesSubmenu)),
    new("Click The Bear",    () => new ClickTheBearDemoWorld(getShowcasesSubmenu)),
], backFactory: getMainMenu);

MenuEntry[] mainMenuEntries =
[
    new("Text",      getTextSubmenu,      MenuEntryStyle.Submenu),
    new("UI",        getUISubmenu,        MenuEntryStyle.Submenu),
    new("Graphics",  getGraphicsSubmenu,  MenuEntryStyle.Submenu),
    new("Input",     getInputSubmenu,     MenuEntryStyle.Submenu),
    new("Showcases", getShowcasesSubmenu, MenuEntryStyle.Submenu),
    new("Camera Demo", () => new CameraDemoWorld(getMainMenu)),
    new("IO Demo",     () => new IODemoWorld(getMainMenu)),
    new("Tween Demo",  () => new TweenDemoWorld(getMainMenu)),
];

mainMenuFactory = () => new MenuWorld(mainMenuEntries);

GameLauncher.Run(appSettings, mainMenuFactory);
