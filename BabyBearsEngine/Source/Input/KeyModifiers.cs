namespace BabyBearsEngine.Input;

/// <summary>
/// Modifier keys held alongside a primary key in a <see cref="KeyCombination"/>. Left and right
/// variants of the same modifier (e.g. <see cref="Keys.LeftControl"/> and <see cref="Keys.RightControl"/>)
/// are treated as equivalent — only one bit covers both sides.
/// </summary>
[Flags]
public enum KeyModifiers
{
    /// <summary>No modifiers held.</summary>
    None = 0,

    /// <summary>Either Control key.</summary>
    Ctrl = 1 << 0,

    /// <summary>Either Shift key.</summary>
    Shift = 1 << 1,

    /// <summary>Either Alt key.</summary>
    Alt = 1 << 2,
}
