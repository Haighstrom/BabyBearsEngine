using BabyBearsEngine;
using BabyBearsEngine.Runtime;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Tests.System;

[TestClass]
public class GameLauncherTests
{
    private sealed class CloseImmediatelyWorld : World
    {
        public override void Update(double elapsed)
        {
            EngineConfiguration.WindowService.Close();
        }
    }

    private static ApplicationSettings TestSettings => new()
    {
        WindowSettings = new WindowSettings { CheckForMainThread = false }
    };

    [TestMethod]
    public void Run_WithDefaultSettings_CompletesWithoutThrowing()
    {
        GameLauncher.Run(TestSettings, () => new CloseImmediatelyWorld());
    }
}
