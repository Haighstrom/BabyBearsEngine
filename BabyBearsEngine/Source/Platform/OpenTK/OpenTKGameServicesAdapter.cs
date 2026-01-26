using BabyBearsEngine.Input;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.Runtime;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.OpenTK;

internal class OpenTKGameServicesAdapter(GameWindow gameWindow) : IRuntimeServices
{
    public IKeyboardService KeyboardService { get; } = new OpenTKKeyboardAdapter(gameWindow.KeyboardState);

    public IMouseService MouseService { get; } = new OpenTKMouseAdapter(gameWindow.MouseState);

    public IWindowService WindowService { get; } = new OpenTKWindowAdapter(gameWindow);

    public IGPUResourceDeletionService GPUResourceDeletionService { get; } = new DefaultGPUResourceDeletionService();
}
