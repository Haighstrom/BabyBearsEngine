using BabyBearsEngine.Input;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.PowerUsers;

namespace BabyBearsEngine.Runtime;

//Platform Abstraction Layer
public interface IPlatformContext
{
    IWindow Window { get; }
    IKeyboard Keyboard { get; }
    IMouse Mouse { get; }
    IGPUResourceDeletionService GPUResourceDeletionService { get; }
    IWorldSwitcher WorldSwitcher { get; }
}
