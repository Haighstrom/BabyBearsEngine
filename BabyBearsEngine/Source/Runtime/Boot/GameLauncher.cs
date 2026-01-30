using BabyBearsEngine.PowerUsers;
using BabyBearsEngine.Runtime;
using BabyBearsEngine.Source.Platform.OpenTK;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine;

public static class GameLauncher
{
    private enum LauncherStatus { NotStarted, Initialised, Running }

    private static IGamePlatform s_platform = new OpenTKPlatform();
    private static LauncherStatus s_status = LauncherStatus.NotStarted;

    public static void SetPlatform(IGamePlatform platform)
    {
        if (s_status != LauncherStatus.NotStarted)
        {
            throw new InvalidOperationException("Game already initialised");
        }

        s_platform = platform;
    }

    public static void Initialise(ApplicationSettings appSettings)
    {
        if (s_status != LauncherStatus.NotStarted)
        {
            throw new InvalidOperationException("Game already initialised");
        }

        s_platform.CreateWindow(appSettings);
        s_platform.CreateContext();
        
        RuntimeServices.Initialise(s_platform.Context);
        
        s_status = LauncherStatus.Initialised;
    }

    public static void Run(IWorld world)
    {
        if (s_status == LauncherStatus.NotStarted)
        {
            throw new InvalidOperationException("Game not yet initialised. Call GameLauncher.Initialise first.");
        }
        if (s_status == LauncherStatus.Running)
        {
            throw new InvalidOperationException("Game already running.");
        }

        s_status = LauncherStatus.Running;

        try
        {
            var gameLoop = new WorldGameLoop(world, s_platform.Context.RenderHost);

            s_platform.Run(gameLoop);
        }
        finally
        {
            s_status = LauncherStatus.NotStarted;
        }
    }

    public static void Run(ApplicationSettings appSettings, Func<IWorld> worldFactory)
    {
        Initialise(appSettings);
        Run(worldFactory());
    }
}
