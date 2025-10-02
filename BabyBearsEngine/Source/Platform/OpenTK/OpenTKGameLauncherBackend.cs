using BabyBearsEngine.Source.Boot;
using BabyBearsEngine.Source.Config;
using BabyBearsEngine.Source.Services;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Source.Platform.OpenTK;

internal sealed class OpenTKGameLauncherBackend() : IGameLauncherBackend
{
    public void Run(ApplicationSettings applicationSettings, Func<IWorld> createWorld)
    {
        using BabyBearsWindow window = new(applicationSettings.GameLoopSettings.ToOpenTK(), applicationSettings.WindowSettings.ToOpenTK(), createWorld);

        GameServices.Initialise(new OpenTKGameServicesAdapter(window));

        window.Run();
    }
}
