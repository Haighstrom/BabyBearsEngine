using BabyBearsEngine.Source.Config;
using BabyBearsEngine.Source.Platform.OpenTK;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Source.Boot;

public static class GameLauncher
{
    private static bool s_running = false;

    public static void Run(ApplicationSettings applicationSettings, Func<IWorld> createWorld, IGameLauncherBackend? backendOverride = null)
    {
        ArgumentNullException.ThrowIfNull(applicationSettings);
        ArgumentNullException.ThrowIfNull(createWorld);

        if (s_running)
        {
            throw new InvalidOperationException("Game is already running.");
        }

        s_running = true;

        var backend = backendOverride ?? new OpenTKGameLauncherBackend();

        backend.Run(applicationSettings, createWorld);
    }
}
