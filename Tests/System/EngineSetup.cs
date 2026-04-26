using BabyBearsEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BabyBearsEngine.Tests.System;

[TestClass]
public static class EngineSetup
{
    [AssemblyInitialize]
    public static void Initialise(TestContext context)
    {
        var appSettings = new ApplicationSettings()
        {
            WindowSettings = new WindowSettings()
            {
                Width = 800,
                Height = 600,
                Title = "System Tests",
            }
        };

        GameLauncher.Initialise(appSettings);
    }
}
