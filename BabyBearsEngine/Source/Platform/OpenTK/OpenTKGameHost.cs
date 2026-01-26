using BabyBearsEngine.Source.GameEngine;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Platform.OpenTK;

internal sealed class OpenTKGameHost(GameWindow gameWindow, IGameLoop gameLoop) : IDisposable
{
    public void Run()
    {
        gameWindow.Load += OnLoad;
        gameWindow.Unload += OnUnload;
        gameWindow.UpdateFrame += OnUpdateFrame;
        gameWindow.RenderFrame += OnRenderFrame;
        gameWindow.FramebufferResize += OnFramebufferResize;

        try
        {
            gameWindow.Run();
        }
        finally
        {
            gameWindow.Load -= OnLoad;
            gameWindow.Unload -= OnUnload;
            gameWindow.UpdateFrame -= OnUpdateFrame;
            gameWindow.RenderFrame -= OnRenderFrame;
            gameWindow.FramebufferResize -= OnFramebufferResize;
        }
    }

    private void OnLoad() => gameLoop.Load();

    private void OnUnload() => gameLoop.Unload();

    private void OnUpdateFrame(FrameEventArgs args) => gameLoop.Update(args.Time);

    private void OnRenderFrame(FrameEventArgs args) => gameLoop.Render(args.Time);

    private void OnFramebufferResize(FramebufferResizeEventArgs args) => gameLoop.HandleScreenResize(args.Width, args.Height);

    public void Dispose() => gameWindow.Dispose();
}

