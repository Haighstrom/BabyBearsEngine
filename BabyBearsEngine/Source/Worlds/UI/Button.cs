using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A clickable, labelled rectangle. All visual styling — background, state tints, and text
/// style — comes from a <see cref="ButtonTheme"/>; the only thing the button itself owns is
/// the geometry, the current interaction state, and the label text.
/// </summary>
public class Button : Entity
{
    private readonly IGraphic? _background;
    private bool _disabled = false;
    private bool _hovered = false;
    private bool _pressed = false;
    private readonly TextGraphic? _textImage;
    private readonly ButtonTheme? _theme;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling for the button. Use <see cref="ButtonTheme.Default"/> for prototype work.</param>
    /// <param name="text">Optional label text. Defaults to empty; can also be changed at runtime via <see cref="Text"/>.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    /// <param name="multilineText">When true, the label wraps to the button width and honours newline characters. Required for any label containing '\n'.</param>
    public Button(float x, float y, float width, float height, ButtonTheme theme, string text = "", int layer = 0, bool multilineText = false)
        : base(x, y, width, height, clickable: true, layer: layer)
    {
        _theme = theme;

        _background = theme.BackgroundFactory(new Rect(0, 0, width, height));
        _background.Colour = theme.Idle;
        Add(_background);

        _textImage = new TextGraphic(theme.Text, text, 0, 0, width, height) { Multiline = multilineText };
        Add(_textImage);
    }

    /// <param name="rect">Position and size relative to the parent container.</param>
    /// <param name="theme">Visual styling for the button. Use <see cref="ButtonTheme.Default"/> for prototype work.</param>
    /// <param name="text">Optional label text. Defaults to empty; can also be changed at runtime via <see cref="Text"/>.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    /// <param name="multilineText">When true, the label wraps to the button width and honours newline characters. Required for any label containing '\n'.</param>
    public Button(Rect rect, ButtonTheme theme, string text = "", int layer = 0, bool multilineText = false)
        : this(rect.X, rect.Y, rect.W, rect.H, theme, text, layer, multilineText)
    {
    }

    internal Button(float x, float y, float width, float height)
        : base(x, y, width, height, clickable: true) { }

    /// <summary>
    /// When true, the button ignores all mouse interaction and renders with the theme's
    /// <see cref="ButtonTheme.Disabled"/> tint. Defaults to false.
    /// </summary>
    public bool Disabled
    {
        get => _disabled;
        set
        {
            if (_disabled != value)
            {
                _disabled = value;
                _hovered = false;
                _pressed = false;
                ApplyState();
            }
        }
    }

    /// <summary>The button's label text. Mutating this updates the rendered glyphs on the next frame.</summary>
    public string Text
    {
        get => _textImage?.Text ?? string.Empty;
        set { _textImage?.Text = value; }
    }

    protected override void OnSizeChanged()
    {
        base.OnSizeChanged();
        if (_background is not null)
        {
            _background.Width = Width;
            _background.Height = Height;
            if (_textImage is not null)
            {
                _textImage.Width = Width;
                _textImage.Height = Height;
            }
        }
    }

    private void ApplyState()
    {
        if (_background is null || _theme is null)
        {
            return;
        }

        _background.Colour = _disabled ? _theme.Disabled
                           : _pressed ? _theme.Pressed
                           : _hovered ? _theme.Hover
                           : _theme.Idle;
    }

    protected override void OnLeftPressed()
    {
        if (_disabled)
        {
            return;
        }

        base.OnLeftPressed();
        _pressed = true;
        ApplyState();
    }

    protected override void OnLeftClicked()
    {
        if (_disabled)
        {
            return;
        }

        base.OnLeftClicked();
        _pressed = false;
        ApplyState();
    }

    protected override void OnMouseEntered()
    {
        if (_disabled)
        {
            return;
        }

        base.OnMouseEntered();
        _hovered = true;
        ApplyState();
    }

    protected override void OnMouseExited()
    {
        if (_disabled)
        {
            return;
        }

        base.OnMouseExited();
        _hovered = false;
        _pressed = false;
        ApplyState();
    }
}
