using System.IO;
using BabyBearsEngine.Source.Graphics.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Graphics.Shaders;

public class DefaultShaderProgram : IShaderProgram
{
    private bool _disposed = false;
    private readonly int _windowSizeLocation;

    public DefaultShaderProgram(GameWindow window)
    {
        string vsSource = File.ReadAllText("Assets/Shaders/shader.vert");
        int vertexShader = OpenGLHelper.CreateShader(vsSource, ShaderType.VertexShader);

        string fsSource = File.ReadAllText("Assets/Shaders/shader.frag");
        int fragmentShader = OpenGLHelper.CreateShader(fsSource, ShaderType.FragmentShader);

        Handle = OpenGLHelper.CreateShaderProgram(vertexShader, fragmentShader);

        _windowSizeLocation = GL.GetUniformLocation(Handle, "WindowSize");

        window.Resize += Window_Resize;
    }

    public int Handle { get; }

    private void Window_Resize(ResizeEventArgs args)
    {
        Bind();
        GL.Uniform2(_windowSizeLocation, new Vector2(args.Width, args.Height));
    }

    public void Bind()
    {
        GL.UseProgram(Handle);
    }

    public void Unbind()
    {
        GL.UseProgram(0);
    }

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            GL.DeleteProgram(Handle);

            _disposed = true;
        }
    }

    ~DefaultShaderProgram()
    {
        if (_disposed == false)
        {
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
