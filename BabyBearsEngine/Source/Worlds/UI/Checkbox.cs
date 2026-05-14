using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A <see cref="Button"/> that owns a tick overlay and toggles its visibility on left-click.
/// The box's idle / hover / pressed appearance is inherited from <see cref="Button"/>;
/// <see cref="Checked"/> / <see cref="Unchecked"/> events fire when <see cref="IsChecked"/>
/// changes (whether by click or by direct assignment).
/// </summary>
public class Checkbox : Button
{
    private readonly IGraphic _tick;
    private bool _isChecked = false;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling for the checkbox.</param>
    /// <param name="isChecked">Initial check state. Defaults to <c>false</c>.</param>
    public Checkbox(float x, float y, float width, float height, CheckboxTheme theme, bool isChecked = false)
        : base(x, y, width, height, theme.Box)
    {
        _isChecked = isChecked;

        _tick = theme.TickFactory(new Rect(0, 0, width, height));
        _tick.Visible = isChecked;
        Add(_tick);
    }

    /// <summary>True when the tick is shown. Setting this raises <see cref="Checked"/> or <see cref="Unchecked"/> if the value changes.</summary>
    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value)
            {
                return;
            }

            _isChecked = value;
            _tick.Visible = value;

            if (value)
            {
                Checked?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Unchecked?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>Raised when the checkbox is ticked, by click or by setting <see cref="IsChecked"/>.</summary>
    public event EventHandler? Checked;

    /// <summary>Raised when the checkbox is unticked, by click or by setting <see cref="IsChecked"/>.</summary>
    public event EventHandler? Unchecked;

    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();
        IsChecked = !_isChecked;
    }
}
