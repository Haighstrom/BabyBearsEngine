using BabyBearsEngine.Worlds;
using OpenTK.Windowing.Common;

namespace BabyBearsEngine;

public interface IWindowService
{
    event Action<ResizeEventArgs>? Resize;

    int Width { get; }
    int Height { get; }

    void ChangeWorld(IWorld world);
}

