using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Demos.Source.Demos.AnimationDemo;
using BabyBearsEngine.Demos.Source.Demos.BearSpinner3000;
using BabyBearsEngine.Demos.Source.Demos.ClickDemo;
using BabyBearsEngine.Demos.Source.Demos.ClickTheBearDemo;
using BabyBearsEngine.Demos.Source.Demos.GraphicDemo;
using BabyBearsEngine.Demos.Source.Demos.KeyboardDemo;
using BabyBearsEngine.Demos.Source.Demos.MouseDemo;
using BabyBearsEngine.Demos.Source.Demos.ShaderDemo;
using BabyBearsEngine.Demos.Source.Demos.TextDemo;
using BabyBearsEngine.Demos.Source.Demos.UIDemo;
using BabyBearsEngine.Demos.Source.Menu;
using Microsoft.Extensions.DependencyInjection;

var appSettings = new ApplicationSettings()
{
    WindowSettings = new WindowSettings()
    {
        Width = 800,
        Height = 600,
        Title = "Bears",
    }
};

GameLauncher.Initialise(appSettings);

var services = new ServiceCollection();
services.AddSingleton<Func<World>>(sp => () => new MenuWorld(sp.GetServices<DemoWorld>()));
services.AddTransient<DemoWorld, AnimationDemoWorld>();
services.AddTransient<DemoWorld, BearSpinnerWorld>();
services.AddTransient<DemoWorld, ClickDemoWorld>();
services.AddTransient<DemoWorld, ClickTheBearDemoWorld>();
services.AddTransient<DemoWorld, GraphicDemoWorld>();
services.AddTransient<DemoWorld, KeyboardDemoWorld>();
services.AddTransient<DemoWorld, MouseDemoWorld>();
services.AddTransient<DemoWorld, ShaderDemoWorld>();
services.AddTransient<DemoWorld, TextDemoWorld>();
services.AddTransient<DemoWorld, UIDemoWorld>();

var provider = services.BuildServiceProvider();
var demos = provider.GetServices<DemoWorld>();

GameLauncher.Run(new MenuWorld(demos));
