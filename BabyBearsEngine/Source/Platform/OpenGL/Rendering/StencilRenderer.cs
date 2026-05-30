using BabyBearsEngine.OpenGL;
using Matrix3 = BabyBearsEngine.Geometry.Matrix3;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Platform.OpenGL.Rendering;

internal sealed class StencilRenderer(ITexture imageTexture, ITexture stencilTexture) : IDisposable
{
    private static Vertex[] GetVertices(float w, float h, Color4 colour) =>
    [
        new(0, 0, colour, 0, 0),
        new(w, 0, colour, 1, 0),
        new(0, h, colour, 0, 1),
        new(w, h, colour, 1, 1),
    ];

    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private bool _disposed = false;

    public void UpdateVertices(float w, float h, Colour colour)
    {
        _vertexDataBuffer.SetNewVertices(GetVertices(w, h, colour.ToOpenTK()));
    }

    public float Threshold { get; set; } = 0f;

    public void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        StencilShaderProgram shader = StencilShaderProgram.Instance;
        shader.Bind();
        _vertexDataBuffer.Bind();
        imageTexture.Bind(TextureTarget.Texture2D, TextureUnit.Texture0);
        stencilTexture.Bind(TextureTarget.Texture2D, TextureUnit.Texture1);
        shader.SetThreshold(Threshold);
        shader.SetProjectionMatrix(ref projection);
        shader.SetModelViewMatrix(ref modelView);
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _vertexDataBuffer.Dispose();
        _disposed = true;
    }
}
