using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Graphics;

internal class SimpleGraphic : AddableBase, IRenderable, IDisposable
{
    private readonly object _syncRoot = new();
    private bool _disposed = false;

    public DefaultShaderProgram Shader { get; } = new();
    private readonly VertexDataBuffer<Vertex> _vertexDataBuffer = new();
    public ITexture Texture { get; }
    public Vertex[] Vertices { get; }

    //private ITexture temp;
    public SimpleGraphic(ITexture texture, Vertex[] vertices)
    {
        //temp = texture;
        if (vertices.Length < 3)
            throw new ArgumentException("Cannot make a SimpleGraphic with fewer than 3 vertices", nameof(vertices));

        Texture = texture;// new TextureFactory().CreateTextureFromImageFile("Assets/bear.png");//texture;
        Vertices = vertices;

        Shader.Bind();
        _vertexDataBuffer.Bind();
        Texture.Bind();
        _vertexDataBuffer.SetNewVertices(Vertices);
    }

    // Properties
    public bool Visible { get; set; } = true;

    public void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        Shader.Bind();
        _vertexDataBuffer.Bind();
        Texture.Bind();

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SimpleGraphic() => Dispose(false);

    private void Dispose(bool manual)
    {
        if (!_disposed)
        {
            if (manual)
            {
                lock (_syncRoot)
                {
                    _vertexDataBuffer.Dispose();
                }
            }
            else
            {
                //Log.Warning($"{nameof(SimpleGraphic)} did not dispose correctly, did you forget to call Dispose()?");
            }
            _disposed = true;
        }
    }

}
