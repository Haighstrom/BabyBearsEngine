using BabyBearsEngine.Geometry;
using BabyBearsEngine.Graphics;
using BabyBearsEngine.Rendering.Graphics.Text;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A clickable, labelled rectangle. All visual styling — background, state tints, and text
/// style — comes from a <see cref="ButtonTheme"/>; the only thing the button itself owns is
/// the geometry, the current interaction state, and the label text.
/// </summary>
public class Button : Entity
{
    private readonly IGraphic _background;
    private bool _hovered = false;
    private bool _pressed = false;
    private readonly TextImage _textImage;
    private readonly ButtonTheme _theme;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling for the button. Use <see cref="ButtonTheme.Default"/> for prototype work.</param>
    /// <param name="text">Optional label text. Defaults to empty; can also be changed at runtime via <see cref="Text"/>.</param>
    public Button(int x, int y, int width, int height, ButtonTheme theme, string text = "")
        : base(x, y, width, height, clickable: true)
    {
        _theme = theme;

        _background = theme.BackgroundFactory(new Rect(0, 0, width, height));
        _background.Colour = theme.Idle;
        Add(_background);

        _textImage = new TextImage(theme.Text.Font, text, theme.Text.Colour, 0, 0, width, height)
        {
            HAlignment = theme.Text.HAlignment,
            VAlignment = theme.Text.VAlignment,
        };
        Add(_textImage);
    }

    /// <summary>The button's label text. Mutating this updates the rendered glyphs on the next frame.</summary>
    public string Text
    {
        get => _textImage.Text;
        set => _textImage.Text = value;
    }

    private void ApplyState()
    {
        _background.Colour = _pressed ? _theme.Pressed
                           : _hovered ? _theme.Hover
                           : _theme.Idle;
    }

    protected override void OnLeftPressed()
    {
        base.OnLeftPressed();
        _pressed = true;
        ApplyState();
    }

    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();
        _pressed = false;
        ApplyState();
    }

    protected override void OnMouseEntered()
    {
        base.OnMouseEntered();
        _hovered = true;
        ApplyState();
    }

    protected override void OnMouseExited()
    {
        base.OnMouseExited();
        _hovered = false;
        _pressed = false;
        ApplyState();
    }
}
