using BabyBearsEngine.Source.Worlds;
using OpenTK.Windowing.Common;

namespace BabyBearsEngine.Source.Services;

public interface IWindowService
{
    event Action<ResizeEventArgs>? Resize;

    void ChangeWorld(IWorld world);
}

