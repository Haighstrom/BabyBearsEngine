using BabyBearsEngine.Source.Worlds;
using OpenTK.Windowing.Common;

namespace BabyBearsEngine.Source.Services;

public interface IWindowService
{
    event Action<ResizeEventArgs>? Resize;

    int Width { get; }
    int Height { get; }

    void ChangeWorld(IWorld world);
}

