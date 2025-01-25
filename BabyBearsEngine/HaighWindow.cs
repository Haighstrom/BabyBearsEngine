using BabyBearsEngine.Source.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine;

public class HaighWindow(int width, int height, string title) 
    : GameWindow(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = (width, height), Title = title })
{
    private readonly float[] _vertices = {
    -0.5f, -0.5f, 0.0f, //Bottom-left vertex
     0.5f, -0.5f, 0.0f, //Bottom-right vertex
     0.0f,  0.5f, 0.0f  //Top vertex
    };
    int _vertexBufferObject;
    Shader _shader = null!;
    int _vertexArrayObject;

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        _vertexBufferObject = GL.GenBuffer();
        _shader = new Shader("shader.vert", "shader.frag");
        _vertexArrayObject = GL.GenVertexArray();

        GL.BindVertexArray(_vertexArrayObject);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(_vertexArrayObject);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        _shader.Dispose();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        _shader.Use();
        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        SwapBuffers();
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
    }
}
