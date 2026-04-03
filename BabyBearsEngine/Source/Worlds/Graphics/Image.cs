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
        set => Colour = new(Colour.R, Colour.G, Colour.B, (byte)Math.Round(value * 255f));
    }

    public float Angle
    {
        get => _angle;
        set
        {
            _angle = value;
        }
    }

    public void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        if (_verticesChanged)
        {
            _graphicRenderer.UpdateVertices(x, y, width, height, _colour);
            _verticesChanged = false;
        }

        var mv = modelView;

        if (_angle != 0)
        {
            mv = Matrix3.RotateAroundPoint(ref mv, _angle, x + width / 2, y + height / 2);
        }

        _graphicRenderer.Render(ref projection, ref mv);
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
