namespace BabyBearsEngine.Worlds.Graphics;

/// <summary>
/// Controls how a border is positioned relative to the graphic's stated rectangle.
/// </summary>
public enum BorderPosition
{
    /// <summary>
    /// The border draws inside the stated bounds. The fill area shrinks by the border thickness on each side.
    /// </summary>
    Inside,

    /// <summary>
    /// The border draws outside the stated bounds. The fill area occupies the full stated rectangle.
    /// </summary>
    Outside,

    /// <summary>
    /// The border straddles the edge of the stated bounds, half inside and half outside.
    /// The fill area shrinks by half the border thickness on each side.
    /// </summary>
    Centred,
}
