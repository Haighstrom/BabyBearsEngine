using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.Runtime;
using BabyBearsEngine.Source.GameEngine;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine;

public static class GameLauncher
{
    private enum LauncherStatus { NotStarted, Initialised, Running }

    private static IGamePlatformFactory s_platform = new OpenTKPlatformFactory();
    private static IGameEngine? s_loadedEngine;

    private static LauncherStatus s_status = LauncherStatus.NotStarted;

    public static void SetPlatform(IGamePlatformFactory platform)
    {
        if (s_status != LauncherStatus.NotStarted)
        {
            throw new InvalidOperationException("Game already initialised");
        }

        s_platform = platform;
    }

    public static void SetDefaultPlatform() => SetPlatform(new OpenTKPlatformFactory());

    public static void Initialise(ApplicationSettings appSettings)
    {
        if (s_status != LauncherStatus.NotStarted)
        {
            throw new InvalidOperationException("Game already initialised");
        }

        s_loadedEngine = s_platform.CreateGameEngine(appSettings);

        var context = s_platform.CreatePlatformContext(s_loadedEngine);

        RuntimeServices.Initialise(context);
        
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
