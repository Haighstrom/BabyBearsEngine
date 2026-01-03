using BabyBearsEngine.Source.Services;

namespace BabyBearsEngine.Source.Platform.OpenTK;

internal class OpenTKGameServicesAdapter(BabyBearsWindow gameWindow) : IGameServices
{
    public IKeyboardService KeyboardService { get; } = new OpenTKKeyboardAdapter(gameWindow.KeyboardState);

    public IMouseService MouseService { get; } = new OpenTKMouseAdapter(gameWindow.MouseState);

    public IWindowService WindowService { get; } = new OpenTKWindowAdapter(gameWindow);
}
