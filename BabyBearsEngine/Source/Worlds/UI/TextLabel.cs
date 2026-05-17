using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A non-interactive text label. Renders a single piece of text inside a rectangular region
/// using a <see cref="TextTheme"/> for font, colour, and alignment. An optional background
/// colour and border can be specified at construction time.
/// </summary>
public class TextLabel : Entity
{
    private readonly IColourGraphic? _background;
    private readonly IBorderGraphic? _border;
    private readonly ITextGraphic _text;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling — font, colour, and alignment.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="backgroundColour">Optional background fill colour. Pass <see langword="null"/> for no background.</param>
    /// <param name="borderColour">Optional border colour. Pass <see langword="null"/> for no border.</param>
    /// <param name="borderThickness">Border width in pixels on each side when <paramref name="borderColour"/> is not <see langword="null"/>.</param>
    /// <param name="borderPosition">Controls where the border draws relative to the stated bounds when <paramref name="borderColour"/> is not <see langword="null"/>.</param>
    public TextLabel(float x, float y, float width, float height, TextTheme theme, string text,
        Colour? backgroundColour = null, Colour? borderColour = null, float borderThickness = 2f,
        BorderPosition borderPosition = BorderPosition.Inside)
        : base(x, y, width, height)
    {
        _background = backgroundColour.HasValue
            ? new ColourGraphic(backgroundColour.Value, 0f, 0f, width, height)
            : null;
        _border = borderColour.HasValue
            ? new BorderedRectangleGraphic(0f, 0f, width, height, borderThickness, borderColour.Value, borderPosition)
            : null;
        _text = new TextGraphic(theme.Font, text, theme.Colour, 0, 0, width, height)
        {
            HAlignment = theme.HAlignment,
            VAlignment = theme.VAlignment,
        };
        if (_background is not null)
        {
            Add(_background);
        }

        if (_border is not null)
        {
            Add(_border);
        }

        Add(_text);
    }

    internal TextLabel(float x, float y, float width, float height, ITextGraphic textGraphic,
        IColourGraphic? background = null, IBorderGraphic? border = null)
        : base(x, y, width, height)
    {
        _background = background;
        _border = border;
        _text = textGraphic;
        if (_background is not null)
        {
            Add(_background);
        }

        if (_border is not null)
        {
            Add(_border);
        }

        Add(_text);
    }

    /// <summary>
    /// Background fill colour, or <see langword="null"/> if no background is shown.
    /// Setting to <see langword="null"/> hides the background; setting to a colour shows it.
    /// Has no effect if no background was provided at construction.
    /// </summary>
    public Colour? BackgroundColour
    {
        get => _background is { Visible: true } ? _background.Colour : null;
        set
        {
            if (_background is null)
            {
                return;
            }

            if (value is null)
            {
                _background.Visible = false;
                return;
            }

            _background.Colour = value.Value;
            _background.Visible = true;
        }
    }

    /// <summary>
    /// Border colour, or <see langword="null"/> if no border is shown.
    /// Setting to <see langword="null"/> hides the border; setting to a colour shows it.
    /// Has no effect if no border was provided at construction.
    /// </summary>
    public Colour? BorderColour
    {
        get => _border is { Visible: true } ? _border.BorderColour : null;
        set
        {
            if (_border is null)
            {
                return;
            }

            if (value is null)
            {
                _border.Visible = false;
                return;
            }

            _border.BorderColour = value.Value;
            _border.Visible = true;
        }
    }

    /// <summary>The colour used to render the glyphs.</summary>
    public Colour Colour
    {
        get => _text.Colour;
        set => _text.Colour = value;
    }

    /// <summary>The text string displayed by this label.</summary>
    public string Text
    {
        get => _text.Text;
        set => _text.Text = value;
    }
}
