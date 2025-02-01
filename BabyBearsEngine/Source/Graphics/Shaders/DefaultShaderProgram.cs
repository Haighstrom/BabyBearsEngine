using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Graphics.Shaders;

public class DefaultShaderProgram : IShaderProgram
{
    private bool _disposed = false;

    public DefaultShaderProgram(GameWindow window)
    {
        int vertexShader;
        int fragmentShader;
        string vertexSource = File.ReadAllText("Assets/Shaders/shader.vert");
        string fragmentSource = File.ReadAllText("Assets/Shaders/shader.frag");

        vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexSource);

        fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentSource);

        GL.CompileShader(vertexShader);

        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out var success);
        if (success == 0)
        {
            var infoLog = GL.GetShaderInfoLog(vertexShader);
            Console.WriteLine(infoLog);
        }

        GL.CompileShader(fragmentShader);

        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out var success2);
        if (success2 == 0)
        {
            var infoLog = GL.GetShaderInfoLog(fragmentShader);
            Console.WriteLine(infoLog);
        }

        Handle = GL.CreateProgram();

        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);

        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out var success3);
        if (success3 == 0)
        {
            var infoLog = GL.GetProgramInfoLog(Handle);
            Console.WriteLine(infoLog);
        }
        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);

        window.Resize += Window_Resize;
    }

    public int Handle { get; }

    private void Window_Resize(ResizeEventArgs args)
    {
        Bind();
        var windowSizeLocation = GL.GetUniformLocation(Handle, "WindowSize");
        GL.Uniform2(windowSizeLocation, new Vector2(args.Width, args.Height));
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
