using System.IO;
using BabyBearsEngine.Source.Graphics.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Graphics.Shaders;

public class SolidColourShaderProgram : IShaderProgram
{
    public SolidColourShaderProgram(GameWindow window)
    {
        string vsSource = File.ReadAllText("Assets/Shaders/vs_nomatrixsolidcolour.vert");
        int vertexShader = OpenGLHelper.CreateShader(vsSource, ShaderType.VertexShader);

        string fsSource = File.ReadAllText("Assets/Shaders/fs_solidcolour.frag");
        int fragmentShader = OpenGLHelper.CreateShader(fsSource, ShaderType.FragmentShader);

        Handle = OpenGLHelper.CreateShaderProgram(vertexShader, fragmentShader);

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
    private bool _disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            GL.DeleteProgram(Handle);

            _disposedValue = true;
        }
    }

    ~SolidColourShaderProgram()
    {
        if (_disposedValue == false)
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
