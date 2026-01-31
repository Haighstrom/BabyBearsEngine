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

//option 1
//GameLauncher.Run(appSettings, () => new MenuWorld());

//option 2
GameLauncher.Initialise(appSettings);
GameLauncher.Run(new MenuWorld());
