using BabyBearsEngine.Worlds;
using OpenTK.Windowing.Common;

namespace BabyBearsEngine.Platform.OpenTK;

public sealed class OpenTKWindowAdapter(BabyBearsWindow window) : IWindowService
{
    public int Width => window.ClientSize.X;

    public int Height => window.ClientSize.Y;

    public event Action<ResizeEventArgs>? Resize
    {
        add { window.Resize += value; }
        remove { window.Resize -= value; }
    }

    public void ChangeWorld(IWorld world)
    {
        window.World = world;
    }
}

