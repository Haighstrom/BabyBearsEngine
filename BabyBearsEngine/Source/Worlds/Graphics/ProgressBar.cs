using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// A horizontal bar that fills from left to right as <see cref="AmountFilled"/> moves from
/// 0 to 1. The background graphic stays fixed. The fill graphic tracks the current value: a
/// <see cref="TextureGraphic"/> fill is clipped via its <see cref="TextureGraphic.SourceArea"/>
/// (revealed left-to-right without distorting the texture); any other fill graphic has its
/// width mutated.
/// <para>Composed of two child graphics (background then fill) but exposes itself as a single
/// <see cref="IGraphic"/> — they are rendered manually rather than held in a container.</para>
/// </summary>
public class ProgressBar : GraphicBase
{
    private readonly IGraphic _background;
    private readonly IGraphic _fill;
    private readonly TextureGraphic? _textureFill;
    private readonly float _fullWidth;
    private float _amountFilled = 0f;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels at <see cref="AmountFilled"/> = 1.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling for the bar.</param>
    /// <param name="amountFilled">Initial fill amount in [0, 1]. Defaults to 0.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public ProgressBar(float x, float y, float width, float height, ProgressBarTheme theme, float amountFilled = 0f, int layer = int.MaxValue)
        : base(x, y, width, height, layer)
    {
        _fullWidth = width;

        _background = theme.BackgroundFactory(new Rect(0, 0, width, height));
        _fill = theme.FillFactory(new Rect(0, 0, width, height));
        _textureFill = _fill as TextureGraphic;

        AmountFilled = amountFilled;
    }

    /// <param name="rect">Position and size relative to the parent container. The rect's width is the bar width at <see cref="AmountFilled"/> = 1.</param>
    /// <param name="theme">Visual styling for the bar.</param>
    /// <param name="amountFilled">Initial fill amount in [0, 1]. Defaults to 0.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top. Default is <see cref="int.MaxValue"/> (drawn at the back). Must be ≥ 0.</param>
    public ProgressBar(Rect rect, ProgressBarTheme theme, float amountFilled = 0f, int layer = int.MaxValue)
        : this(rect.X, rect.Y, rect.W, rect.H, theme, amountFilled, layer)
    {
    }

    /// <summary>
    /// How full the bar is, in [0, 1]. Values outside the range are clamped. Setting this
    /// to 1 (from a smaller value) raises <see cref="BarFilled"/>.
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

            if (_textureFill is not null)
            {
                _textureFill.SourceArea = new Rect(0, 0, clamped, 1);
            }
            else
            {
                _fill.Width = _fullWidth * clamped;
            }

            if (wasUnfilled && clamped >= 1f)
            {
                BarFilled?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>Raised when <see cref="AmountFilled"/> reaches 1 (from a smaller value).</summary>
    public event EventHandler? BarFilled;

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        var mv = Matrix3.Translate(ref modelView, X, Y);
        _background.Render(ref projection, ref mv);
        _fill.Render(ref projection, ref mv);
    }
}
