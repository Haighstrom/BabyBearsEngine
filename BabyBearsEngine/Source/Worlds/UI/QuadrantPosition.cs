namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// Names the nine canonical positions within a rectangle — three columns (left, middle, right)
/// by three rows (top, middle, bottom). Typically used to anchor a UI element within the
/// screen or a parent container without stretching it.
/// </summary>
public enum QuadrantPosition
{
    /// <summary>Top-left corner.</summary>
    TopLeft,

    /// <summary>Horizontally centred along the top edge.</summary>
    TopMiddle,

    /// <summary>Top-right corner.</summary>
    TopRight,

    /// <summary>Vertically centred along the left edge.</summary>
    MiddleLeft,

    /// <summary>Centred in both axes.</summary>
    Centre,

    /// <summary>Vertically centred along the right edge.</summary>
    MiddleRight,

    /// <summary>Bottom-left corner.</summary>
    BottomLeft,

    /// <summary>Horizontally centred along the bottom edge.</summary>
    BottomMiddle,

    /// <summary>Bottom-right corner.</summary>
    BottomRight,
}
