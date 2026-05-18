using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenGL.Rendering;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A textured rectangle that displays one frame of a sprite sheet. Set <see cref="Frame"/> to
/// change which region of the sheet is shown. Construction allocates GL resources — must be
/// created on the engine thread after the GL context exists.
/// </summary>
/// <param name="texture">Sprite sheet texture. Not owned by this sprite; not disposed when the sprite is.</param>
/// <param name="x">X position in the parent's local space.</param>
/// <param name="y">Y position in the parent's local space.</param>
/// <param name="width">Width in pixels.</param>
/// <param name="height">Height in pixels.</param>
/// <param name="initialFrame">Zero-based frame index to display initially.</param>
/// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
public class Sprite(ISpriteTexture texture, float x, float y, float width, float height, int initialFrame = 0, int layer = 0)
    : GraphicBase(x, y, width, height, layer), ISprite, IDisposable
{
    private readonly GraphicRenderer _renderer = new GraphicRenderer(texture);
    private Colour _colour = Colour.White;
    private bool _verticesChanged = true;
    private bool _disposed = false;

    /// <inheritdoc/>
    public Colour Colour
    {
        get => _colour;
        set
        {
            _colour = value;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public int Frame
    {
        get => initialFrame;
        set
        {
            int wrapped = ((value % texture.Frames) + texture.Frames) % texture.Frames;
            if (initialFrame == wrapped)
            {
                return;
            }
            initialFrame = wrapped;
            _verticesChanged = true;
        }
    }

    /// <inheritdoc/>
    public int Frames => texture.Frames;

    /// <summary>Last valid frame index (<see cref="Frames"/> − 1).</summary>
    public int LastFrame => Frames - 1;

    /// <summary>The sprite sheet texture currently used for rendering.</summary>
    public ISpriteTexture Texture
    {
        get => texture;
        set
        {
            texture = value;
            _renderer.Shader = new();
            _verticesChanged = true;
        }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        _verticesChanged = true;
    }

    /// <inheritdoc/>
    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        if (Width == 0 || Height == 0)
        {
            return;
        }

        if (_verticesChanged)
        {
            var (u1, v1, u2, v2) = texture.GetFrameUVs(initialFrame);
            _renderer.UpdateVertices(Width, Height, _colour, u1, v1, u2, v2);
            _verticesChanged = false;
        }

        var mv = Matrix3.Translate(ref modelView, X, Y);
        _renderer.Render(ref projection, ref mv);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _renderer.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
