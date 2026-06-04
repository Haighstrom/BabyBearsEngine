using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.OpenGL;

/// <summary>
/// Wraps a VAO and VBO, and sets up the vertex attributes for a given vertex type.
/// </summary>
/// <typeparam name="TVertex"></typeparam>
internal sealed class VertexDataBuffer<TVertex> : IDisposable where TVertex : struct, IVertex
{
    private bool _disposed = false;
    private readonly BufferUsageHint _bufferUsageHint;

    public VertexDataBuffer(BufferUsageHint bufferUsageHint = BufferUsageHint.DynamicDraw)
    {
        GLThread.Ensure();
        _bufferUsageHint = bufferUsageHint;

        VAO.Bind();
        VBO.Bind();
        TVertex.SetVertexAttributes();
    }

    public VAO VAO { get; } = new();

    public VBO VBO { get; } = new();

    public void Bind() => VAO.Bind();

    public void SetNewVertices(TVertex[] vertices)
    {
        SetNewVertices(vertices, vertices.Length);
    }

    /// <summary>
    /// Uploads the first <paramref name="count"/> elements of <paramref name="vertices"/>.
    /// Lets callers use a cached high-water-marked array and pass the active element count
    /// separately, avoiding the per-frame allocation that .Length-based sizing would force.
    /// </summary>
    public void SetNewVertices(TVertex[] vertices, int count)
    {
        VBO.Bind();
        GL.BufferData(BufferTarget.ArrayBuffer, count * TVertex.Stride, vertices, _bufferUsageHint);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        VAO.Dispose();
        VBO.Dispose();
        _disposed = true;
    }
}
