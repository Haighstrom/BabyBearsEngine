using System.IO;
using BabyBearsEngine.Source.Graphics.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace BabyBearsEngine.Source.Graphics.Shaders;

internal class StandardMatrixShaderProgram : IShaderProgram
{
    private bool _disposed;

    private readonly int _mvMatrixLocation;
    private readonly int _pMatrixLocation;

    public StandardMatrixShaderProgram(GameWindow window)
    {
        string vsSource = File.ReadAllText("Assets/Shaders/vs_default.vert");
        int vertexShader = OpenGLHelper.CreateShader(vsSource, ShaderType.VertexShader);

        string fsSource = File.ReadAllText("Assets/Shaders/fs_default.frag");
        int fragmentShader = OpenGLHelper.CreateShader(fsSource, ShaderType.FragmentShader);

        Handle = OpenGLHelper.CreateShaderProgram(vertexShader, fragmentShader);

        _mvMatrixLocation = GL.GetUniformLocation(Handle, "MVMatrix");
        _pMatrixLocation = GL.GetUniformLocation(Handle, "PMatrix");

        var matrix = Matrix3.Identity;
        SetModelViewMatrix(ref matrix);

        window.Resize += Window_Resize;
    }

    private void Window_Resize(ResizeEventArgs args)
    {
        var halfWidth = args.Width / 2f;
        var halfHeight = args.Height / 2f;

        Matrix3 scale = new(1 / halfWidth, 0, 0, 0, 1 / halfHeight, 0, 0, 0, 1);
        Matrix3 flipY = new(1, 0, 0, 0, -1, 0, 0, 0, 1);
        Matrix3 translate = new(1, 0, -halfWidth, 0, 1, -halfHeight, 0, 0, 1);

        var ortho = scale * flipY * translate;

        SetProjectionMatrix(ref ortho);
    }

    public int Handle { get; }

    public void Bind()
    {
        GL.UseProgram(Handle);
    }

    public void Unbind()
    {
        GL.UseProgram(0);
    }

    public void SetModelViewMatrix(ref Matrix3 modelViewMatrix)
    {
        Bind();
        GL.UniformMatrix3(_mvMatrixLocation, true, ref modelViewMatrix);
    }

    public void SetProjectionMatrix(ref Matrix3 projectionMatrix)
    {
        Bind();
        GL.UniformMatrix3(_pMatrixLocation, true, ref projectionMatrix);
    }

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~StandardMatrixShaderProgram()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
