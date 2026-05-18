using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds.Graphics;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A vertically scrollable list panel. Items added via <see cref="AddItem"/> are rendered
/// inside a clipped viewport; a <see cref="Scrollbar"/> on the right-hand side controls the
/// scroll position. Set <see cref="ContentHeight"/> to the total height of all items so the
/// scrollbar thumb is sized correctly.
/// </summary>
/// <remarks>
/// The content area uses GL scissor clipping — items that fall outside the panel's visible
/// bounds are not drawn. Click detection on scrolled items is not adjusted for the scroll
/// offset; items inside the panel are intended to be non-interactive (labels, rows, etc.).
/// </remarks>
public class ScrollingListPanel : Entity
{
    private const float ScrollbarWidth = 20f;

    private readonly ContentPane _contentPane;
    private float _contentHeight = 0f;
    private readonly Scrollbar _scrollbar;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels (includes the scrollbar).</param>
    /// <param name="height">Height in pixels (the visible viewport height).</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public ScrollingListPanel(float x, float y, float width, float height, ScrollingListPanelTheme theme, int layer = 0)
        : base(x, y, width, height, layer: layer)
    {
        float paneWidth = width - ScrollbarWidth;

        if (theme.BackgroundColour.HasValue)
        {
            Add(new ColourGraphic(theme.BackgroundColour.Value, 0f, 0f, width, height));
        }

        _contentPane = new ContentPane(0f, 0f, paneWidth, height);
        Add(_contentPane);

        _scrollbar = new Scrollbar(paneWidth, 0f, ScrollbarWidth, height, ScrollbarDirection.Vertical, theme.Scrollbar);
        _scrollbar.ScrollChanged += OnScrollChanged;
        Add(_scrollbar);
    }

    /// <param name="rect">Position and size relative to the parent container. Width includes the scrollbar; height is the visible viewport height.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public ScrollingListPanel(Rect rect, ScrollingListPanelTheme theme, int layer = 0)
        : this(rect.X, rect.Y, rect.W, rect.H, theme, layer)
    {
    }

    /// <summary>
    /// Total height of all content items in pixels. Setting this resizes the scrollbar thumb
    /// proportionally and adjusts the scroll offset. When <see cref="ContentHeight"/> is less
    /// than or equal to the panel height the thumb fills the entire track and scrolling has no
    /// effect.
    /// </summary>
    public float ContentHeight
    {
        get => _contentHeight;
        set
        {
            _contentHeight = value;
            UpdateScrollbar();
        }
    }

    /// <summary>Adds an item to the scrollable content area.</summary>
    public void AddItem(IAddable item) => _contentPane.Add(item);

    /// <summary>
    /// Returns the thumb proportion for a given panel and content height. Exposed internally
    /// so unit tests can verify the calculation without constructing the full widget.
    /// </summary>
    internal static float CalculateThumbProportion(float panelHeight, float contentHeight)
        => panelHeight / Math.Max(contentHeight, panelHeight);

    /// <summary>
    /// Returns the pixel scroll offset for a given scroll position, panel height, and content
    /// height. Exposed internally so unit tests can verify the calculation.
    /// </summary>
    internal static float CalculateScrollOffset(float amountFilled, float panelHeight, float contentHeight)
        => amountFilled * Math.Max(0f, contentHeight - panelHeight);

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
        => _contentPane.ScrollOffset = CalculateScrollOffset(e.NewValue, Height, _contentHeight);

    private void UpdateScrollbar()
        => _scrollbar.ThumbProportion = CalculateThumbProportion(Height, _contentHeight);

    private sealed class ContentPane : ContainerEntity
    {
        private float _scrollOffset = 0f;

        internal ContentPane(float x, float y, float width, float height)
            : base(x, y, width, height) { }

        internal float ScrollOffset
        {
            get => _scrollOffset;
            set => _scrollOffset = value;
        }

        public override (float x, float y) GetWindowCoordinates(float localX, float localY) =>
            Parent?.GetWindowCoordinates(localX + X, localY + Y) ?? (localX + X, localY + Y);

        public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
        {
            var (wx, wy) = GetWindowCoordinates(0f, 0f);
            var (_, _, _, vpH) = OpenGLHelper.GetViewport();

            int scissorX = (int)wx;
            int scissorY = vpH - (int)(wy + Height);
            int scissorW = (int)Width;
            int scissorH = (int)Height;

            bool wasEnabled = GL.IsEnabled(EnableCap.ScissorTest);
            int[] prevScissor = new int[4];
            GL.GetInteger(GetPName.ScissorBox, prevScissor);

            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(scissorX, scissorY, scissorW, scissorH);

            var mv = Matrix3.Translate(ref modelView, X, Y - _scrollOffset);
            base.Render(ref projection, ref mv);

            if (wasEnabled)
            {
                GL.Scissor(prevScissor[0], prevScissor[1], prevScissor[2], prevScissor[3]);
            }
            else
            {
                GL.Disable(EnableCap.ScissorTest);
            }
        }
    }
}
