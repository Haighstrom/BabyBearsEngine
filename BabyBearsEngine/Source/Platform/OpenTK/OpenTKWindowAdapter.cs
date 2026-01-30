using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Platform.OpenTK;

internal sealed class OpenTKWindowAdapter(GameWindow window) : IWindow
{
    public int Width => window.ClientSize.X;

    public int Height => window.ClientSize.Y;

    public event Action<ResizeEventArgs>? Resize
    {
        add { window.Resize += value; }
        remove { window.Resize -= value; }
    }
}
