using BabyBearsEngine.Source.Services;
using BabyBearsEngine.Source.Worlds;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Platform.OpenTK;

public sealed class OpenTKWindowAdapter(BabyBearsWindow window) : IWindowService
{
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

