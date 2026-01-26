using BabyBearsEngine.Input;
using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.Runtime;
using BabyBearsEngine.Source.OpenTK;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Runtime.Boot;

public sealed record class GameLauncherConfiguration
{
    public IGameLauncherBackend Backend { get; init; } = new OpenTKGameLauncherBackend();
    public Func<IGameWindowContext, IRuntimeServices> CreateRuntimeServices { get; init; } = (context) => new OpenTKGameServicesAdapter((GameWindow)context.NativeWindow);
}
