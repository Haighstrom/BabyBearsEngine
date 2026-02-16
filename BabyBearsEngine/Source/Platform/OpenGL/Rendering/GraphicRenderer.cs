using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Platform.OpenGL.Shaders.ShaderPrograms;
using OpenTK.Mathematics;
using Matrix3 = OpenTK.Mathematics.Matrix3;

namespace BabyBearsEngine.Source.Platform.OpenGL.Rendering;

internal class GraphicRenderer(ITexture texture) : IDisposable
{
    private static Vertex[] GetVertices(float x, float y, float w, float h, Color4 colour) =>
    [
        new (x, y, colour, 0, 0), // bottom left
        new (x + w, y, colour, 1, 0), // bottom right
        new (x, y + h, colour, 0, 1), // top left
        new (x + w, y + h, colour, 1, 1), // top right
    ];

    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    private bool _disposedValue;

    public StandardMatrixShaderProgram Shader { get; set; } = new();

    public void UpdateVertices(float x, float y, float w, float h, Colour colour)
    {
        _vertexDataBuffer.SetNewVertices(GetVertices(x, y, w, h, colour.ToOpenTK()));
    }

    public void UpdateAngle(float angle, float x, float y, float width, float height)
    {
        var mv = Matrix3.Identity;

        var translateBack = new Matrix3(
            1, 0, x + width / 2,
            0, 1, y + height / 2,
            0, 0, 1);

        var rotationAroundOrigin = Matrix3.CreateRotationZ(MathHelper.DegreesToRadians(-angle));

        var translateToOrigin = new Matrix3(
            1, 0, -(x + width / 2),
            0, 1, -(y + height / 2),
            0, 0, 1);

        mv = translateBack * rotationAroundOrigin * translateToOrigin * mv;

        Shader.SetModelViewMatrix(ref mv);
    }

    public void Render(Geometry.Matrix3 projection)
    {
        Shader.Bind();
        _vertexDataBuffer.Bind();
        texture.Bind();

        if (Shader is IWorldShader worldShader)
        {
            worldShader.SetProjectionMatrix(projection);
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
