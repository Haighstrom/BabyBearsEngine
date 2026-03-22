using BabyBearsEngine.Source.Runtime;
using OpenTK.Windowing.Common;

namespace BabyBearsEngine;

public static class Window
{
    private static IWindow Implementation => EngineConfiguration.WindowService;

    public static event Action<ResizeEventArgs> Resize
    {
        add => Implementation.Resize += value;
        remove => Implementation.Resize -= value;
    }

    public static int Width => Implementation.Width;
    public static int Height => Implementation.Height;
}
