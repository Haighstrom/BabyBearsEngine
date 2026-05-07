using System;
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

    private sealed class TryRecursiveRunWorld : World
    {
        public Exception? CapturedException { get; private set; }

        public override void Update(double elapsed)
        {
            try
            {
                GameLauncher.Run(this);
            }
            catch (InvalidOperationException ex)
            {
                CapturedException = ex;
            }

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

    [TestMethod]
    public void Run_CalledTwice_CompletesWithoutThrowing()
    {
        GameLauncher.Run(TestSettings, () => new CloseImmediatelyWorld());
        GameLauncher.Run(TestSettings, () => new CloseImmediatelyWorld());
    }

    [TestMethod]
    public void Run_CalledWhileAlreadyRunning_Throws()
    {
        var world = new TryRecursiveRunWorld();

        GameLauncher.Run(TestSettings, () => world);

        Assert.IsNotNull(world.CapturedException);
        Assert.Contains("already running", world.CapturedException.Message);
    }
}
