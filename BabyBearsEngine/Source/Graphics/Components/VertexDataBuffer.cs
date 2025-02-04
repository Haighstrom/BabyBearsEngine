namespace BabyBearsEngine.Source.Graphics.Components;

internal class VertexDataBuffer<TVertex> : IDisposable where TVertex : struct, IVertex
{
    private bool _disposed;
    private readonly VAO _vAO = new();
    private readonly VBO _vBO = new();
    private readonly BufferUsageHint _bufferUsageHint;

    public VertexDataBuffer(BufferUsageHint bufferUsageHint = BufferUsageHint.DynamicDraw)
    {
        _bufferUsageHint = bufferUsageHint;

        _vAO.Bind();
        _vBO.Bind();
        TVertex.SetVertexAttributes();
    }

    public void SetNewVertices(TVertex[] vertices)
    {
        _vBO.Bind();
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * TVertex.Stride, vertices, _bufferUsageHint);
    }

    public void Bind()
    {
        _vAO.Bind();
    }

    public void Unbind()
    {
        _vAO.Unbind();
    }

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _vAO.Dispose();
                _vBO.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ShaderConnector()
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
