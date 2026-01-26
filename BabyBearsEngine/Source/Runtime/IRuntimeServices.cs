using BabyBearsEngine.Input;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Runtime;

public interface IRuntimeServices
{
    IWindowService WindowService { get; }
    IKeyboardService KeyboardService { get; }
    IMouseService MouseService { get; }
    IGPUResourceDeletionService GPUResourceDeletionService { get; }
}
