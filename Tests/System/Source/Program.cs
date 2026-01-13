using BabyBearsEngine;
using BabyBearsEngine.Source.Runtime.Boot;
using BabyBearsEngine.Tests.System.Source.ShadingLanguageCookbook.Menu;

var appSettings = new ApplicationSettings()
{
    WindowSettings = new WindowSettings()
    {
        Width = 800,
        Height = 600,
        Title = "Bears"
    }
};

GameLauncher.Run(appSettings, () => new MenuWorld());
