using BabyBearsEngine.Geometry;
using BabyBearsEngine.Graphics;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A horizontal bar that fills from left to right as <see cref="AmountFilled"/> moves from
/// 0 to 1. The background graphic stays fixed; the fill graphic's width is mutated to track
/// the current value.
/// </summary>
public class ProgressBar : Entity
{
    private readonly IGraphic _background;
    private readonly IGraphic _fill;
    private readonly int _fullWidth;
    private float _amountFilled = 0f;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels at <see cref="AmountFilled"/> = 1.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling for the bar.</param>
    /// <param name="amountFilled">Initial fill amount in [0, 1]. Defaults to 0.</param>
    public ProgressBar(int x, int y, int width, int height, ProgressBarTheme theme, float amountFilled = 0f)
        : base(x, y, width, height)
    {
        _fullWidth = width;

        _background = theme.BackgroundFactory(new Rect(0, 0, width, height));
        Add(_background);

        _fill = theme.FillFactory(new Rect(0, 0, width, height));
        Add(_fill);

        AmountFilled = amountFilled;
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
            _fill.Width = _fullWidth * clamped;

            if (wasUnfilled && clamped >= 1f)
            {
                BarFilled?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>Raised when <see cref="AmountFilled"/> reaches 1 (from a smaller value).</summary>
    public event EventHandler? BarFilled;
}
