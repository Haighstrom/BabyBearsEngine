using System.Threading;
using BabyBearsEngine.Runtime;
using BabyBearsEngine.Source.Runtime.Boot;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine;

public static class GameLauncher
{
    private static bool s_running;

    private static GameLauncherConfiguration s_configuration = new();

    public static void SetConfiguration(GameLauncherConfiguration configuration)
    {
        if (s_running)
        {
            throw new InvalidOperationException("Cannot change configuration while the game is running.");
        }

        s_configuration = configuration;
    }

    public static void Run(
        ApplicationSettings applicationSettings, 
        Func<IWorld> createWorld)
    {
        ArgumentNullException.ThrowIfNull(applicationSettings);
        ArgumentNullException.ThrowIfNull(createWorld);

        if (Interlocked.Exchange(ref s_running, true))
        {
            throw new InvalidOperationException("Game is already running.");
        }

        try
        {
            var backend = s_configuration.Backend;

            using var gameWindowContext = backend.CreateWindowContext(applicationSettings, createWorld);

            var runtimeServices = s_configuration.CreateRuntimeServices(gameWindowContext);

            RuntimeServices.Initialise(runtimeServices);

            backend.RunGameLoop(gameWindowContext);
        }
        finally
        {
            Interlocked.Exchange(ref s_running, false);
        }
    }
}
