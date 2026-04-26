using System;
using BabyBearsEngine.Demos.Source;
using BabyBearsEngine.Demos.Source.Demos.BearSpinner3000;
using BabyBearsEngine.Demos.Source.Demos.ClickDemo;
using BabyBearsEngine.Demos.Source.Demos.TextDemo;
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
services.AddTransient<DemoWorld, BearSpinnerWorld>();
services.AddTransient<DemoWorld, ClickDemoWorld>();
services.AddTransient<DemoWorld, TextDemoWorld>();

var provider = services.BuildServiceProvider();
var demos = provider.GetServices<DemoWorld>();

GameLauncher.Run(new MenuWorld(demos));
