using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A small floating label that's normally hidden, shown by calling <see cref="Show"/> after a
/// hover delay and hidden by <see cref="Hide"/>. Use <see cref="AttachTo"/> to wire it to a
/// target's hover events with automatic cleanup, or subscribe to the events manually for full
/// control.
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
    private readonly IGraphic? _background;
    private readonly TextGraphic? _text;

    /// <param name="x">Initial X position relative to the parent container.</param>
    /// <param name="y">Initial Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="text">The tooltip's label text.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public SimpleToolTip(float x, float y, float width, float height, TooltipTheme theme, string text, int layer = 0)
        : base(x, y, width, height, layer: layer)
    {
        Visible = false;

        _background = theme.BackgroundFactory(new Rect(0, 0, width, height));
        Add(_background);

        _text = new TextGraphic(theme.Text.Font, text, theme.Text.Colour, 0, 0, width, height)
        {
            HAlignment = theme.Text.HAlignment,
            VAlignment = theme.Text.VAlignment,
            Multiline = true,
        };
        Add(_text);
    }

    /// <param name="rect">Initial position and size relative to the parent container.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="text">The tooltip's label text.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public SimpleToolTip(Rect rect, TooltipTheme theme, string text, int layer = 0)
        : this(rect.X, rect.Y, rect.W, rect.H, theme, text, layer)
    {
    }

    internal SimpleToolTip(float x, float y, float width, float height)
        : base(x, y, width, height)
    {
        Visible = false;
    }

    /// <summary>The tooltip's label text. Mutating this updates the rendered glyphs on the next frame.</summary>
    public string Text
    {
        get => _text?.Text ?? string.Empty;
        set
        {
            if (_text is null)
            {
                throw new InvalidOperationException("This SimpleToolTip has no text graphic (it was created via the internal test constructor); Text cannot be set.");
            }

            _text.Text = value;
        }
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

    /// <summary>
    /// Wires this tooltip to <paramref name="target"/>: <see cref="Show"/> on
    /// <see cref="Entity.MouseHovered"/>, <see cref="Hide"/> on
    /// <see cref="Entity.MouseHoverStopped"/> or <see cref="Entity.MouseExited"/>. The
    /// subscription is torn down (and the tooltip hidden) when either the target or this
    /// tooltip is removed from the entity tree, so a caller doesn't have to remember to
    /// dismiss the tooltip when the trigger goes away.
    /// </summary>
    public void AttachTo(Entity target)
    {
        ArgumentNullException.ThrowIfNull(target);

        EventHandler showHandler = (_, _) => Show();
        EventHandler hideHandler = (_, _) => Hide();
        EventHandler? detachHandler = null;
        detachHandler = (_, _) =>
        {
            target.MouseHovered -= showHandler;
            target.MouseHoverStopped -= hideHandler;
            target.MouseExited -= hideHandler;
            target.Removed -= detachHandler;
            Removed -= detachHandler;
            Hide();
        };

        target.MouseHovered += showHandler;
        target.MouseHoverStopped += hideHandler;
        target.MouseExited += hideHandler;
        target.Removed += detachHandler;
        Removed += detachHandler;
    }
}
