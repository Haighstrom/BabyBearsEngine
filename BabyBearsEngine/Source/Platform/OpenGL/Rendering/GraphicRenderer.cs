using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Platform.OpenGL.Shaders.ShaderPrograms;

namespace BabyBearsEngine.Source.Platform.OpenGL.Rendering;

internal class GraphicRenderer(ITexture texture) : IDisposable
{
    private static Vertex[] GetVertices(float w, float h, OpenTK.Mathematics.Color4 colour) =>
    [
        new (0, 0, colour, 0, 0),
        new (w, 0, colour, 1, 0),
        new (0, h, colour, 0, 1),
        new (w, h, colour, 1, 1),
    ];

    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private bool _disposedValue;

    public StandardMatrixShaderProgram Shader { get; set; } = new();

    public void UpdateVertices(float w, float h, Colour colour)
    {
        _vertexDataBuffer.SetNewVertices(GetVertices(w, h, colour.ToOpenTK()));
    }

    public void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        Shader.Bind();
        _vertexDataBuffer.Bind();
        texture.Bind();

        if (Shader is IMVPShader mvpShader)
        {
            mvpShader.SetProjectionMatrix(ref projection);
            mvpShader.SetModelViewMatrix(ref modelView);
        }

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~GraphicRenderer()
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
}
