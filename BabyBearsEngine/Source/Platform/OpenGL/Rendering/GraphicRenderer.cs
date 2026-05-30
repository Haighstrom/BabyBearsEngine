using BabyBearsEngine.OpenGL;
using Matrix3 = BabyBearsEngine.Geometry.Matrix3;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Platform.OpenGL.Rendering;

internal sealed class GraphicRenderer(ITexture texture) : IDisposable
{
    private static Vertex[] GetVertices(float w, float h, Color4 colour, float u1, float v1, float u2, float v2) =>
    [
        new (0, 0, colour, u1, v1),
        new (w, 0, colour, u2, v1),
        new (0, h, colour, u1, v2),
        new (w, h, colour, u2, v2),
    ];

    private IMatrixShaderProgram _shader = new StandardMatrixShaderProgram();
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private bool _disposedValue = false;

    /// <summary>
    /// The matrix-aware shader used to draw this graphic. Defaults to a
    /// <see cref="StandardMatrixShaderProgram"/>; assign a different
    /// <see cref="IMatrixShaderProgram"/> (e.g. <see cref="GreyscaleShaderProgram"/>,
    /// <see cref="BlurShaderProgram"/>) to apply a fragment effect. Never null.
    /// </summary>
    public IMatrixShaderProgram Shader
    {
        get => _shader;
        set => _shader = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// The texture bound on every <see cref="Render"/>. Never null; assigning null throws
    /// <see cref="ArgumentNullException"/>. Not owned by the renderer — disposing the renderer
    /// does not dispose the texture.
    /// </summary>
    public ITexture Texture
    {
        get => texture;
        set => texture = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void UpdateVertices(float w, float h, Colour colour, float u1 = 0, float v1 = 0, float u2 = 1, float v2 = 1)
    {
        _vertexDataBuffer.SetNewVertices(GetVertices(w, h, colour.ToOpenTK(), u1, v1, u2, v2));
    }

    public void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        Shader.Bind();
        _vertexDataBuffer.Bind();
        texture.Bind();

        Shader.SetProjectionMatrix(ref projection);
        Shader.SetModelViewMatrix(ref modelView);

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
    }

    public void Dispose()
    {
        if (_disposedValue)
        {
            return;
        }

        _shader.Dispose();
        _vertexDataBuffer.Dispose();
        _disposedValue = true;
    }
}
