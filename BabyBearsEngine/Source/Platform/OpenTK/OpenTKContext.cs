using BabyBearsEngine.Input;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.PowerUsers;
using BabyBearsEngine.Runtime;

namespace BabyBearsEngine.Platform.OpenTK;

internal class OpenTKContext(OpenTKGameEngine gameEngine) : IPlatformContext
{
    public IKeyboard Keyboard { get; } = new OpenTKKeyboardAdapter(gameEngine.KeyboardState);

    public IMouse Mouse { get; } = new OpenTKMouseAdapter(gameEngine.MouseState);

    public IWindow Window { get; } = new OpenTKWindowAdapter(gameEngine);

    public IGPUResourceDeletionService GPUResourceDeletionService { get; } = new DefaultGPUResourceDeletionService();

    public IWorldSwitcher WorldSwitcher { get; } = gameEngine;
}
