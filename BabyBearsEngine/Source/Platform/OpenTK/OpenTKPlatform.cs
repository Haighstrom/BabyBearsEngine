using BabyBearsEngine.Platform.OpenTK;
using BabyBearsEngine.PowerUsers;
using BabyBearsEngine.Runtime;
using BabyBearsEngine.Source.OpenTK;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Platform.OpenTK;

internal sealed class OpenTKPlatform : IGamePlatform
{
    private GameWindow? _gameWindow;
    private IPlatformContext? _context;

    public IPlatformContext Context => _context ?? throw new InvalidOperationException("Platform context has not been created. Call CreatePlatformContext first.");

    public void CreateWindow(ApplicationSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(appSettings);

        _gameWindow = new GameWindow(appSettings.GameLoopSettings.ToOpenTK(), appSettings.WindowSettings.ToOpenTK());
    }

    public IPlatformContext CreateContext()
    {
        if (_gameWindow == null)
        {
            throw new InvalidOperationException("Game window has not been created. Call CreateWindow first.");
        }

        return _context = new OpenTKContext(_gameWindow);
    }

    public void Run(IGameLoop gameLoop)
    {
        if (_gameWindow == null)
        {
            throw new InvalidOperationException("Game window has not been created. Call CreateWindow first.");
        }

        // Wire up OpenTK events to your GameLoop
        _gameWindow.Load += gameLoop.Load;
        _gameWindow.Unload += gameLoop.Unload;
        _gameWindow.UpdateFrame += (args) => gameLoop.Update(args.Time);
        _gameWindow.RenderFrame += (args) => gameLoop.Render(args.Time);
        _gameWindow.Resize += (args) => gameLoop.HandleScreenResize(args.Width, args.Height);

        _gameWindow.Run();

        _gameWindow.Dispose();

        _gameWindow = null;
    }
}
