var appSettings = new ApplicationSettings()
{
    WindowSettings = new WindowSettings()
    {
        Width = 800,
        Height = 600,
        Title = "Sandbox",
    }
};

GameLauncher.Initialise(appSettings);
GameLauncher.Run(new BabyBearsEngine.Sandbox.ScratchWorld());
