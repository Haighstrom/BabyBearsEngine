using BabyBearsEngine.Source.Services;
using BabyBearsEngine.Source.Worlds;
using OpenTK.Windowing.Common;

namespace BabyBearsEngine.Source.Core;

public static class Window
{
    public static event Action<ResizeEventArgs> Resize
    {
        add => GameServices.WindowService.Resize += value;
        remove => GameServices.WindowService.Resize -= value;
    }

    public static void ChangeWorld(IWorld world) => GameServices.WindowService.ChangeWorld(world);
}
