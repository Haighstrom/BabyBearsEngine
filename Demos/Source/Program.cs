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
using BabyBearsEngine.Demos.Source.Demos.UIDemo;
using BabyBearsEngine.Demos.Source.Menu;

var appSettings = new ApplicationSettings()
{
    WindowSettings = new WindowSettings()
    {
        Width = 800,
        Height = 800,
        Title = "Bears",
    }
};

Func<World> menuFactory = null!;
Func<World> getMenu = () => menuFactory();

(string Name, Func<World> Factory)[] demos =
[
    ("Animation Demo",    () => new AnimationDemoWorld(getMenu)),
    ("Bear Spinner 3000", () => new BearSpinnerWorld(getMenu)),
    ("Camera Demo",       () => new CameraDemoWorld(getMenu)),
    ("Click Demo",        () => new ClickDemoWorld(getMenu)),
    ("Click The Bear",    () => new ClickTheBearDemoWorld(getMenu)),
    ("Graphic Demo",      () => new GraphicDemoWorld(getMenu)),
    ("Keyboard Demo",     () => new KeyboardDemoWorld(getMenu)),
    ("Mouse Demo",        () => new MouseDemoWorld(getMenu)),
    ("Shader Demo",       () => new ShaderDemoWorld(getMenu)),
    ("Text Demo",         () => new TextDemoWorld(getMenu)),
    ("UI Demo",           () => new UIDemoWorld(getMenu)),
];

menuFactory = () => new MenuWorld(demos);

GameLauncher.Run(appSettings, menuFactory);
