using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.Runtime;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine;

public static class GameLauncher
{
    private enum LauncherStatus { NotStarted, Initialised, Running }

    private static OpenTKGameEngine? s_loadedEngine = null;
    private static LauncherStatus s_status = LauncherStatus.NotStarted;

    public static void Initialise(ApplicationSettings appSettings)
    {
        if (s_status != LauncherStatus.NotStarted)
        {
            throw new InvalidOperationException("Game already initialised");
        }

        s_loadedEngine = new OpenTKGameEngine(appSettings);

        EngineConfiguration.Initialise(
            window: new OpenTKWindowAdapter(s_loadedEngine),
            keyboard: new OpenTKKeyboardAdapter(s_loadedEngine.KeyboardState),
            mouse: new OpenTKMouseAdapter(s_loadedEngine.MouseState),
            worldSwitcher: s_loadedEngine);

        s_status = LauncherStatus.Initialised;
    }

    public static void Run(IWorld world)
    {
        if (s_status == LauncherStatus.NotStarted)
        {
            throw new InvalidOperationException("Game not yet initialised. Call GameLauncher.Initialise first.");
        }

        Ensure.NotNull(s_loadedEngine); //this shouldn't be a problem as launcher being started should assign this

        if (s_status == LauncherStatus.Running)
        {
            throw new InvalidOperationException("Game already running.");
        }

        s_status = LauncherStatus.Running;

        try
        {
            s_loadedEngine.Run(world);
        }
        finally
        {
            EngineConfiguration.Reset();
            s_loadedEngine = null;
            s_status = LauncherStatus.NotStarted;
        }
    }

    public static void Run(ApplicationSettings appSettings, Func<IWorld> worldFactory)
    {
        Initialise(appSettings);
        Run(worldFactory());
    }
}
