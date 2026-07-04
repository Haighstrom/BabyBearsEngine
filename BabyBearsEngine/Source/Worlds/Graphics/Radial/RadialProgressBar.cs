using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A radial ("clock") progress indicator that fills as <see cref="AmountFilled"/> moves from
/// 0 to 1: either a pie sector growing from the centre, or a ring/annulus band, sweeping from a
/// configurable start angle — see <see cref="RadialProgressBarTheme"/> and
/// <see cref="RadialFillMeshBuilder"/>. The background graphic stays fixed; the fill graphic
/// (a <see cref="RadialFillGraphic"/> or <see cref="RadialTextureFillGraphic"/>) regenerates its
/// mesh as the sweep grows.
/// <para>Composed of two child graphics (background then fill) but exposes itself as a single
/// <see cref="IGraphic"/> — they are rendered manually rather than held in a container, mirroring <see cref="ProgressBar"/>.</para>
/// </summary>
public class RadialProgressBar : GraphicBase
{
    private readonly IGraphic _background;
    private readonly IGraphic _fill;
    private readonly RadialFillGraphic? _radialColourFill;
    private readonly RadialTextureFillGraphic? _radialTextureFill;
    private float _amountFilled = 0f;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Bounding box width in pixels.</param>
    /// <param name="height">Bounding box height in pixels.</param>
    /// <param name="theme">Visual styling for the bar.</param>
    /// <param name="amountFilled">Initial fill amount in [0, 1]. Defaults to 0.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public RadialProgressBar(float x, float y, float width, float height, RadialProgressBarTheme theme, float amountFilled = 0f, int layer = int.MaxValue)
        : base(x, y, width, height, layer)
    {
        _background = theme.BackgroundFactory(new Rect(0, 0, width, height));
        _fill = theme.FillFactory(new Rect(0, 0, width, height));
        _radialColourFill = _fill as RadialFillGraphic;
        _radialTextureFill = _fill as RadialTextureFillGraphic;

        _amountFilled = Math.Clamp(amountFilled, 0f, 1f);
        ApplyFillAmount();
    }

    /// <param name="rect">Position and size relative to the parent container.</param>
    /// <param name="theme">Visual styling for the bar.</param>
    /// <param name="amountFilled">Initial fill amount in [0, 1]. Defaults to 0.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public RadialProgressBar(Rect rect, RadialProgressBarTheme theme, float amountFilled = 0f, int layer = int.MaxValue)
        : this(rect.X, rect.Y, rect.W, rect.H, theme, amountFilled, layer)
    {
    }

    /// <summary>
    /// How full the bar is, in [0, 1]. Values outside the range are clamped. Setting this
    /// to 1 (from a smaller value) raises <see cref="Filled"/>.
    /// </summary>
    public float AmountFilled
    {
        get => _amountFilled;
        set
        {
            float clamped = Math.Clamp(value, 0f, 1f);

            if (_amountFilled == clamped)
            {
                return;
            }

            bool wasUnfilled = _amountFilled < 1f;
            _amountFilled = clamped;
            ApplyFillAmount();

            if (wasUnfilled && clamped >= 1f)
            {
                Filled?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void ApplyFillAmount()
    {
        if (_radialColourFill is not null)
        {
            _radialColourFill.AmountFilled = _amountFilled;
        }
        else if (_radialTextureFill is not null)
        {
            _radialTextureFill.AmountFilled = _amountFilled;
        }
    }

    /// <summary>Raised when <see cref="AmountFilled"/> reaches 1 (from a smaller value).</summary>
    public event EventHandler? Filled;

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        var mv = Matrix3.Translate(ref modelView, X, Y);
        _background.Render(ref projection, ref mv);
        _fill.Render(ref projection, ref mv);
    }
}
