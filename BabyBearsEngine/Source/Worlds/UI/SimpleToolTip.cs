using BabyBearsEngine.Geometry;
using BabyBearsEngine.Graphics;
using BabyBearsEngine.Rendering.Graphics.Text;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A small floating label that's normally hidden, shown by calling <see cref="Show"/> after a
/// hover delay and hidden by <see cref="Hide"/>. Trigger wiring is the caller's job — typically
/// subscribe to a target's <c>MouseHovered</c> / <c>MouseHoverStopped</c> / <c>MouseExited</c>
/// events.
/// </summary>
/// <remarks>
/// <para>The tooltip is a standalone <see cref="Entity"/>: it does not own a "target" and does
/// not position itself automatically. Add it wherever you want it rendered — adding it to the
/// <see cref="World"/> directly is the easiest way to make it paint over other widgets, since
/// it'll be drawn after them. Engine-wide topmost-layer support is a known gap; until that
/// lands, tooltip layering relies on tree position.</para>
/// </remarks>
public sealed class SimpleToolTip : Entity
{
    private readonly IGraphic _background;
    private readonly TextImage _text;

    /// <param name="x">Initial X position relative to the parent container.</param>
    /// <param name="y">Initial Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="text">The tooltip's label text.</param>
    public SimpleToolTip(int x, int y, int width, int height, TooltipTheme theme, string text)
        : base(x, y, width, height)
    {
        Visible = false;

        _background = theme.BackgroundFactory(new Rect(0, 0, width, height));
        Add(_background);

        _text = new TextImage(theme.Text.Font, text, theme.Text.Colour, 0, 0, width, height)
        {
            HAlignment = theme.Text.HAlignment,
            VAlignment = theme.Text.VAlignment,
        };
        Add(_text);
    }

    /// <summary>The tooltip's label text. Mutating this updates the rendered glyphs on the next frame.</summary>
    public string Text
    {
        get => _text.Text;
        set => _text.Text = value;
    }

    /// <summary>Make the tooltip visible.</summary>
    public void Show()
    {
        Visible = true;
    }

    /// <summary>Hide the tooltip.</summary>
    public void Hide()
    {
        Visible = false;
    }
}
