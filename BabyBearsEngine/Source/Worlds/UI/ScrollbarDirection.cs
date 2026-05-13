namespace BabyBearsEngine.Worlds.UI;

/// <summary>Orientation of a <see cref="Scrollbar"/>.</summary>
public enum ScrollbarDirection
{
    /// <summary>Thumb travels left-to-right; <see cref="Scrollbar.AmountFilled"/> = 0 places it at the left, 1 at the right.</summary>
    Horizontal,

    /// <summary>Thumb travels top-to-bottom; <see cref="Scrollbar.AmountFilled"/> = 0 places it at the top, 1 at the bottom.</summary>
    Vertical,
}
