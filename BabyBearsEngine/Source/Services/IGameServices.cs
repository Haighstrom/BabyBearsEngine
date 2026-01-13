using BabyBearsEngine.Source.Graphics.MemoryManagement;

namespace BabyBearsEngine.Source.Services;

internal interface IGameServices
{
    IWindowService WindowService { get; }
    IKeyboardService KeyboardService { get; }
    IMouseService MouseService { get; }
    IGPUResourceDeletionService GPUResourceDeletionService { get; }
}
