namespace BabyBearsEngine.Source.Graphics.Shaders;

public abstract class ShaderProgramBase : IShaderProgram
{
    private bool _disposed;

    public abstract int Handle { get; }

    public void Bind() => OpenGLHelper.BindShader(Handle);

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
            OpenGLHelper.UnbindShader();
            GL.DeleteProgram(Handle);

            _disposed = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~ShaderProgramBase()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
        //todo: logging of the bad dispose
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
