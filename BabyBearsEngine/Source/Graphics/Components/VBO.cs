using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Source.Graphics.Components;

internal class VBO(BufferUsageHint bufferUsageHint = BufferUsageHint.DynamicDraw) : IVBO
{
    private static int s_lastBoundHandle = 0;

    private bool _disposed;

    public int Handle { get; } = GL.GenBuffer();

    public void BufferData<TVertex>(TVertex[] vertices) where TVertex : struct, IVertex
    {
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * TVertex.Stride, vertices, bufferUsageHint);
    }

    public void Bind()
    {
        if (s_lastBoundHandle != Handle)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
            s_lastBoundHandle = Handle;
        }
    }

    public void Unbind()
    {
        if (s_lastBoundHandle != 0)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            s_lastBoundHandle = 0;
        }
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
    // ~VBO()
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
