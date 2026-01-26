using BabyBearsEngine.OpenGL;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Platform.OpenGL;

internal class OpenTKRenderHost(GameWindow window) : IRenderHost
{
    public void Initialise()
    {
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
    }

    public void BeginFrame()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
    }

    public void EndFrame()
    {
        window.SwapBuffers();
    }

    public void HandleScreenResize(int width, int height)
    {
        GL.Viewport(0, 0, width, height);
    }
}
