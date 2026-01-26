using BabyBearsEngine.Runtime;
using BabyBearsEngine.Source.GameEngine;
using BabyBearsEngine.Source.OpenTK;
using BabyBearsEngine.Source.Platform.OpenGL;
using BabyBearsEngine.Source.Platform.OpenTK;
using BabyBearsEngine.Source.Runtime.Boot;
using BabyBearsEngine.Worlds;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Platform.OpenTK;

internal sealed class OpenTKGameLauncherBackend() : IGameLauncherBackend
{
    public IGameWindowContext CreateWindowContext(ApplicationSettings applicationSettings, Func<IWorld> createWorld)
    {
        var gameWindow = new GameWindow(
            applicationSettings.GameLoopSettings.ToOpenTK(),
            applicationSettings.WindowSettings.ToOpenTK());

        var runtimeServices = new OpenTKGameServicesAdapter(gameWindow);

        var renderHost = new OpenTKRenderHost(gameWindow);

        var gameLoop = new WorldGameLoop(
            createInitialWorld: createWorld,
            renderHost: renderHost);

        var gameHost = new OpenTKGameHost(gameWindow, gameLoop);

        return new OpenTKGameWindowContext(gameWindow, gameHost);
    }

    public void RunGameLoop(IGameWindowContext gameWindowContext)
    {
        var window = (OpenTKGameWindowContext)gameWindowContext;
        window.GameHost.Run();
    }
}
