using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Platform.OpenGL.Rendering;

namespace BabyBearsEngine.Graphics;

public class Image(ITexture texture, float x, float y, float width, float height) : IRenderable, IDisposable
{
    private readonly GraphicRenderer _graphicRenderer = new(texture);
    private float _angle = 0;
    private Colour _colour = Colour.White;
    private bool _verticesChanged = true;
    private bool _modelViewChanged = true;

    /// <summary>
    /// advanced users only
    /// </summary>
    /// <returns></returns>
    public IShaderProgram GetShaderProgram() => _graphicRenderer.Shader;

    //public void SetShadaer(ShaderReference shader) => Shader = ShaderMapping.GetShader(shader);

    public float X
    {
        get => x;
        set
        {
            x = value;
            _verticesChanged = true;
        }
    }

    public float Y
    {
        get => y;
        set
        {
            y = value;
            _verticesChanged = true;
        }
    }

    public float Width
    {
        get => width;
        set
        {
            width = value;
            _verticesChanged = true;
        }
    }

    public float Height
    {
        get => height;
        set
        {
            height = value;
            _verticesChanged = true;
        }
    }

    public Colour Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    public virtual float Alpha
    {
        get => Colour.A;
        set => Colour = new(Colour.R, Colour.G, Colour.B, value);
    }

    public float Angle
    {
        get => _angle;
        set
        {
            _angle = value;
            _modelViewChanged = true;
        }
    }

    public void Render(Matrix3 projection)
    {
        if (_verticesChanged)
        {
            _graphicRenderer.UpdateVertices(x, y, width, height, _colour);
            _verticesChanged = false;
        }

        if (_modelViewChanged)
        {
            _graphicRenderer.UpdateAngle(_angle, x, y, width, height);
            _modelViewChanged = false;
        }

        _graphicRenderer.Render(projection);
    }

    #region Dispose
    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                //_texture.Dispose();
                _graphicRenderer.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Image()
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
