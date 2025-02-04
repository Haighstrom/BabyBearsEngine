using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Source.Graphics.Components;

internal class EBO(BufferUsageHint bufferUsageHint = BufferUsageHint.DynamicDraw)
{
    private static int s_lastBoundHandle = 0;

    private bool _disposed;

    public int Handle { get; } = GL.GenBuffer();

    public void Bind()
    {
        if (s_lastBoundHandle != Handle)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
            s_lastBoundHandle = Handle;
        }
    }

    public void Unbind()
    {
        if (s_lastBoundHandle != 0)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            s_lastBoundHandle = 0;
        }
    }

    public void BufferData(uint[] indices)
    {
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, bufferUsageHint);
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
    // ~EBO()
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
