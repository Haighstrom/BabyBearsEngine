using BabyBearsEngine.OpenGL;
using Matrix3 = BabyBearsEngine.Geometry.Matrix3;
using OpenTK.Mathematics;

namespace BabyBearsEngine.Platform.OpenGL.Rendering;

internal class StencilRenderer(ITexture imageTexture, ITexture stencilTexture) : IDisposable
{
    private static Vertex[] GetVertices(float w, float h, Color4 colour) =>
    [
        new(0, 0, colour, 0, 0),
        new(w, 0, colour, 1, 0),
        new(0, h, colour, 0, 1),
        new(w, h, colour, 1, 1),
    ];

    private readonly StencilShaderProgram _shader = new();
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private bool _disposedValue = false;

    public void UpdateVertices(float w, float h, Colour colour)
    {
        _vertexDataBuffer.SetNewVertices(GetVertices(w, h, colour.ToOpenTK()));
    }

    public float Threshold { get; set; } = 0f;

    public void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        _shader.Bind();
        _vertexDataBuffer.Bind();
        imageTexture.Bind(TextureTarget.Texture2D, TextureUnit.Texture0);
        stencilTexture.Bind(TextureTarget.Texture2D, TextureUnit.Texture1);
        _shader.SetThreshold(Threshold);
        _shader.SetProjectionMatrix(ref projection);
        _shader.SetModelViewMatrix(ref modelView);
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _shader.Dispose();
                _vertexDataBuffer.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
