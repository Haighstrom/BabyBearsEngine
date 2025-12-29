using BabyBearsEngine.Source.Worlds;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Platform.OpenTK;

public class BabyBearsWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, Func<IWorld>? createWorld = null)
    : GameWindow(gameWindowSettings, nativeWindowSettings)
{
    public IWorld World { get; set; } = null!;

    private bool _first = true;

    protected override void OnLoad()
    {
        base.OnLoad();

        World = createWorld?.Invoke() ?? new World();

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        World.Unload();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);           

        World.UpdateThings();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        World.DrawGraphics();

        SwapBuffers();
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
    }
}
