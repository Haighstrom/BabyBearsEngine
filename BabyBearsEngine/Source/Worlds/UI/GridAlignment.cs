namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// Controls how a child entity is positioned and sized within its <see cref="GridLayout"/> cell.
/// </summary>
public enum GridAlignment
{
    /// <summary>Child is stretched to fill the entire cell in both dimensions.</summary>
    Fill,

    /// <summary>Child is centered in the cell and keeps its own size.</summary>
    Center,

    /// <summary>Child is placed at the top-left corner of the cell and keeps its own size.</summary>
    TopLeft,

    /// <summary>Child is centered horizontally at the top of the cell and keeps its own size.</summary>
    TopCenter,

    /// <summary>Child is placed at the top-right corner of the cell and keeps its own size.</summary>
    TopRight,

    /// <summary>Child is centered vertically on the left edge of the cell and keeps its own size.</summary>
    MiddleLeft,

    /// <summary>Child is centered vertically on the right edge of the cell and keeps its own size.</summary>
    MiddleRight,

    /// <summary>Child is placed at the bottom-left corner of the cell and keeps its own size.</summary>
    BottomLeft,

    /// <summary>Child is centered horizontally at the bottom of the cell and keeps its own size.</summary>
    BottomCenter,

    /// <summary>Child is placed at the bottom-right corner of the cell and keeps its own size.</summary>
    BottomRight,

    /// <summary>Child is stretched to fill the full cell width and centered vertically; its own height is preserved.</summary>
    StretchHorizontally,

    /// <summary>Child is stretched to fill the full cell height and centered horizontally; its own width is preserved.</summary>
    StretchVertically,
}
