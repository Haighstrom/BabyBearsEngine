namespace BabyBearsEngine.OpenGL;

public sealed class EBO(BufferUsageHint bufferUsageHint = BufferUsageHint.DynamicDraw) : IDisposable
{
    private bool _disposed;

    public int Handle { get; } = GL.GenBuffer();

    public void Bind() => OpenGLHelper.BindEBO(Handle);

    public void BufferData(uint[] indices)
    {
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, bufferUsageHint);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null

            GPUResourceDeletion.TryRequestDeleteEBO(Handle);

            _disposed = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~EBO()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
