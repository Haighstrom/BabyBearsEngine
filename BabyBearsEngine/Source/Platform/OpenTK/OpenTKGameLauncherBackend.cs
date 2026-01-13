using BabyBearsEngine.Runtime;
using BabyBearsEngine.Source.OpenTK;
using BabyBearsEngine.Source.Runtime.Boot;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Platform.OpenTK;

internal sealed class OpenTKGameLauncherBackend() : IGameLauncherBackend
{
    public void Run(ApplicationSettings applicationSettings, Func<IWorld> createWorld)
    {
        using BabyBearsWindow window = new(applicationSettings.GameLoopSettings.ToOpenTK(), applicationSettings.WindowSettings.ToOpenTK(), createWorld);

        RuntimeServices.Initialise(new OpenTKGameServicesAdapter(window));

        window.Run();
    }
}
