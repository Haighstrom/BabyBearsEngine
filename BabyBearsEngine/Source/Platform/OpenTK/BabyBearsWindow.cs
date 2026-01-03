using BabyBearsEngine.Source.Graphics;
using BabyBearsEngine.Source.Worlds;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Platform.OpenTK;

public class BabyBearsWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, Func<IWorld>? createWorld = null)
    : GameWindow(gameWindowSettings, nativeWindowSettings)
{
    public IWorld World { get; set; } = null!;

    private Image? _image;

    protected override void OnLoad()
    {
        base.OnLoad();

        World = createWorld?.Invoke() ?? new World();

        //_image = new Image("Assets/bear.png", 50, 50, 200, 200);

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        //World.Unload();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);      
        
        if (_image is null)
        {
            _image = new Image("Assets/bear.png", 50, 50, 200, 200);
        }

        //World.UpdateThings();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        _image?.Render();

        //World.DrawGraphics();

        SwapBuffers();
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
    }
}
