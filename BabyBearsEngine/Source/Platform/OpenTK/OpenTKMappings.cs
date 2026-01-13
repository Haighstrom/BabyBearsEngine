using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Platform.OpenTK;

internal static class OpenTkMappings
{
    public static GameWindowSettings ToOpenTK(this GameLoopSettings settings) => new()
    {
        UpdateFrequency = settings.TargetFramesPerSecond
    };

    public static NativeWindowSettings ToOpenTK(this WindowSettings settings) => new()
    {
        ClientSize = (settings.Width, settings.Height),
        Title = settings.Title
    };
}
