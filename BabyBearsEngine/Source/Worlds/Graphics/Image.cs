using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Platform.OpenGL.Rendering;

namespace BabyBearsEngine.Graphics;

/// <summary>
/// A textured rectangle drawn at a position and size, optionally tinted, alpha-blended, and rotated.
/// Construction allocates GL resources (vertex buffer, etc.) — must be created on the engine thread
/// after the GL context exists. Implements <see cref="IDisposable"/> to release those resources.
/// </summary>
/// <param name="texture">The texture to sample. Not owned by this image; not disposed when the image is disposed.</param>
/// <param name="x">X position in the parent's local space.</param>
/// <param name="y">Y position in the parent's local space.</param>
/// <param name="width">Width in pixels.</param>
/// <param name="height">Height in pixels.</param>
public sealed class Image(ITexture texture, float x, float y, float width, float height) : GraphicBase, IGraphic, IDisposable
{
    private readonly GraphicRenderer _graphicRenderer = new(texture);
    private float _angle = 0;
    private Colour _colour = Colour.White;
    private bool _verticesChanged = true;

    /// <summary>X position in the parent's local space.</summary>
    public float X { get; set; } = x;

    /// <summary>Y position in the parent's local space.</summary>
    public float Y { get; set; } = y;

    /// <summary>Width in pixels.</summary>
    public float Width
    {
        get => width;
        set
        {
            width = value;
            _verticesChanged = true;
        }
    }

    /// <summary>Height in pixels.</summary>
    public float Height
    {
        get => height;
        set
        {
            height = value;
            _verticesChanged = true;
        }
    }

    /// <summary>Tint colour multiplied with the texture sample. Defaults to <see cref="Colour.White"/> (no tint).</summary>
    public Colour Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    /// <summary>Convenience accessor for the alpha component of <see cref="Colour"/>, expressed as a normalised float (0–1) on set; raw byte (0–255) on get.</summary>
    public float Alpha
    {
        get => Colour.A;
        set => Colour = new(Colour.R, Colour.G, Colour.B, (byte)Math.Round(value * 255f));
    }

    /// <summary>Rotation angle in degrees, applied around the image's centre.</summary>
    public float Angle
    {
        get => _angle;
        set
        {
            _angle = value;
        }
    }

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        if (_verticesChanged)
        {
            _graphicRenderer.UpdateVertices(width, height, _colour);
            _verticesChanged = false;
        }

        var mv = Matrix3.Translate(ref modelView, X, Y);

        if (_angle != 0)
        {
            mv = Matrix3.RotateAroundPoint(ref mv, _angle, width / 2, height / 2);
        }

        _graphicRenderer.Render(ref projection, ref mv);
    }

    #region Dispose
    private bool _disposedValue;

    private void Dispose(bool disposing)
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
