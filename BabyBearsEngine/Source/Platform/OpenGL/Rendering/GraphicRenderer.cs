using BabyBearsEngine.OpenGL;
using Matrix3 = BabyBearsEngine.Geometry.Matrix3;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Platform.OpenGL.Rendering;

internal class GraphicRenderer(ITexture texture) : IDisposable
{
    private static Vertex[] GetVertices(float w, float h, Color4 colour, float u1, float v1, float u2, float v2) =>
    [
        new (0, 0, colour, u1, v1),
        new (w, 0, colour, u2, v1),
        new (0, h, colour, u1, v2),
        new (w, h, colour, u2, v2),
    ];

    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private bool _disposedValue;

    public StandardMatrixShaderProgram Shader { get; set; } = new();

    public void UpdateVertices(float w, float h, Colour colour, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1)
    {
        _vertexDataBuffer.SetNewVertices(GetVertices(w, h, colour.ToOpenTK(), u1, v1, u2, v2));
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
