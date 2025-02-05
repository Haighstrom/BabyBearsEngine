namespace BabyBearsEngine.Source.Graphics.Components;

/// <summary>
/// Wraps a VAO and VBO, and sets up the vertex attributes for a given vertex type.
/// </summary>
/// <typeparam name="TVertex"></typeparam>
internal class VertexDataBuffer<TVertex> : IDisposable where TVertex : struct, IVertex
{
    private bool _disposed;
    private readonly BufferUsageHint _bufferUsageHint;

    public VertexDataBuffer(BufferUsageHint bufferUsageHint = BufferUsageHint.DynamicDraw)
    {
        _bufferUsageHint = bufferUsageHint;

        OpenGLHelper.BindVAO(VAO);
        OpenGLHelper.BindVBO(VBO);
        TVertex.SetVertexAttributes();
    }

    public VAO VAO { get; } = new();

    public VBO VBO { get; } = new();

    public void Bind() => VAO.Bind();

    public void SetNewVertices(TVertex[] vertices)
    {
        VBO.Bind();
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * TVertex.Stride, vertices, _bufferUsageHint);
    }

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                VAO.Dispose();
                VBO.Dispose();
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
