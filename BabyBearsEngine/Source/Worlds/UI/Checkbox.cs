using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;
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
    // Gap in pixels between the right edge of the box and the start of the label text.
    private const float LabelGap = 5f;

    private readonly IGraphic? _tick;
    // Null for the internal no-theme constructor — Label cannot be set in that case.
    private readonly TextTheme? _labelTextTheme;
    private TextGraphic? _label = null;
    private bool _isChecked = false;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling for the checkbox.</param>
    /// <param name="isChecked">Initial check state. Defaults to <c>false</c>.</param>
    /// <param name="label">Optional text shown to the right of the box; can also be changed at runtime via <see cref="Label"/>. Defaults to empty.</param>
    public Checkbox(float x, float y, float width, float height, CheckboxTheme theme, bool isChecked = false, string label = "")
        : base(x, y, width, height, theme.Box)
    {
        _isChecked = isChecked;
        _labelTextTheme = theme.Box.Text;

        _tick = theme.TickFactory(new Rect(0, 0, width, height));
        _tick.Visible = isChecked;
        Add(_tick);

        Label = label;
    }

    /// <param name="rect">Position and size relative to the parent container.</param>
    /// <param name="theme">Visual styling for the checkbox.</param>
    /// <param name="isChecked">Initial check state. Defaults to <c>false</c>.</param>
    /// <param name="label">Optional text shown to the right of the box; can also be changed at runtime via <see cref="Label"/>. Defaults to empty.</param>
    public Checkbox(Rect rect, CheckboxTheme theme, bool isChecked = false, string label = "")
        : this(rect.X, rect.Y, rect.W, rect.H, theme, isChecked, label)
    {
    }

    internal Checkbox(float x, float y, float width, float height, bool isChecked = false)
        : base(x, y, width, height)
    {
        _isChecked = isChecked;
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
            _tick?.Visible = value;

            if (value)
            {
                OnChecked();
                Checked?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                OnUnchecked();
                Unchecked?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Optional text shown immediately to the right of the box, vertically centred against it and
    /// styled from the checkbox theme's box text. Setting an empty string hides the label.
    /// Throws if the checkbox was created via the internal no-theme constructor.
    /// </summary>
    public string Label
    {
        get => _label?.Text ?? string.Empty;
        set
        {
            if (_labelTextTheme is null)
            {
                throw new InvalidOperationException("This Checkbox has no text theme (it was created via the internal no-theme constructor); Label cannot be set.");
            }

            if (_label is null)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                _label = new TextGraphic(_labelTextTheme, value, Width + LabelGap, 0, 0, Height)
                {
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Centred,
                };
                Add(_label);
            }
            else
            {
                _label.Text = value;
            }

            _label.Width = _label.MeasureString().X;
        }
    }

    /// <summary>Raised when the checkbox is ticked, by click or by setting <see cref="IsChecked"/>.</summary>
    public event EventHandler? Checked;

    /// <summary>Raised when the checkbox is unticked, by click or by setting <see cref="IsChecked"/>.</summary>
    public event EventHandler? Unchecked;

    /// <summary>Called when the checkbox transitions to checked. Override in subclasses instead of subscribing to <see cref="Checked"/>.</summary>
    protected virtual void OnChecked() { }

    /// <summary>Called when the checkbox transitions to unchecked. Override in subclasses instead of subscribing to <see cref="Unchecked"/>.</summary>
    protected virtual void OnUnchecked() { }

    protected override void OnLeftClicked()
    {
        base.OnLeftClicked();
        IsChecked = !_isChecked;
    }
}
