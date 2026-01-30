using BabyBearsEngine.Input;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Platform;

namespace BabyBearsEngine.Runtime;

//Platform Abstraction Layer
public interface IPlatformContext
{
    IWindow Window { get; }
    IKeyboard Keyboard { get; }
    IMouse Mouse { get; }
    IRenderHost RenderHost { get; }
    IGPUResourceDeletionService GPUResourceDeletionService { get; }
}
