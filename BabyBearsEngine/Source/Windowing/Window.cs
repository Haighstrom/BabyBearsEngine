using BabyBearsEngine.Runtime;
using OpenTK.Windowing.Common;

namespace BabyBearsEngine;

public static class Window
{
    public static event Action<ResizeEventArgs> Resize
    {
        add => RuntimeServices.WindowService.Resize += value;
        remove => RuntimeServices.WindowService.Resize -= value;
    }

    public static int Width => RuntimeServices.WindowService.Width;
    public static int Height => RuntimeServices.WindowService.Height;
}
