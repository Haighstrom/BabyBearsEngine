using BabyBearsEngine.Input;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.Runtime;
using BabyBearsEngine.Source.Platform;
using BabyBearsEngine.Source.Platform.OpenGL;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.OpenTK;

internal class OpenTKContext(GameWindow gameWindow) : IPlatformContext
{
    public IKeyboard Keyboard { get; } = new OpenTKKeyboardAdapter(gameWindow.KeyboardState);

    public IMouse Mouse { get; } = new OpenTKMouseAdapter(gameWindow.MouseState);

    public IWindow Window { get; } = new OpenTKWindowAdapter(gameWindow);

    public IGPUResourceDeletionService GPUResourceDeletionService { get; } = new DefaultGPUResourceDeletionService();

    public IRenderHost RenderHost { get; } = new OpenTKRenderHost(gameWindow);
}
