using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Platform.OpenTK;

public sealed class OpenTKWindowAdapter(GameWindow window) : IWindowService
{
    public int Width => window.ClientSize.X;

    public int Height => window.ClientSize.Y;

    public event Action<ResizeEventArgs>? Resize
    {
        add { window.Resize += value; }
        remove { window.Resize -= value; }
    }
}
