using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.Source.Runtime.Boot;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Platform.OpenTK;

internal sealed class OpenTKGameWindowContext(
    GameWindow gameWindow,
    OpenTKGameHost gameHost)
    : IGameWindowContext
{
    public object NativeWindow { get; } = gameWindow;

    public OpenTKGameHost GameHost { get; } = gameHost;

    public void Dispose() => gameWindow.Dispose();
}
