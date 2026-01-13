using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Platform.OpenTK;

public class BabyBearsWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, Func<IWorld>? createWorld = null)
    : GameWindow(gameWindowSettings, nativeWindowSettings)
{
    public IWorld World { get; set; } = null!;

    protected override void OnLoad()
    {
        base.OnLoad();

        World = createWorld?.Invoke() ?? new World();

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Enable(EnableCap.Blend);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        World.Unload();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        World.UpdateThings(args.Time);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        World.DrawGraphics();

        GPUResourceDeletion.ProcessDeletes();

        SwapBuffers();
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
    }
}
