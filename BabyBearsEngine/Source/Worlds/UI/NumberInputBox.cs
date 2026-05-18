using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A <see cref="TextInputBox"/> constrained to numeric input. Only digits, an optional
/// decimal point, and an optional leading minus sign are accepted.
/// </summary>
/// <remarks>
/// <see cref="Value"/> returns <c>null</c> when the box is empty or contains a partial entry
/// such as <c>"-"</c>. Clamping to <see cref="MinValue"/> / <see cref="MaxValue"/> is not
/// applied during typing — subscribe to <see cref="TextInputBox.Submitted"/> or
/// <see cref="TextInputBox.FocusLost"/> to enforce bounds after the user finishes editing.
/// </remarks>
public class NumberInputBox : TextInputBox
{
    private readonly bool _allowDecimals;
    private readonly bool _allowNegative;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="allowDecimals">When true, a single <c>.</c> character is permitted.</param>
    /// <param name="allowNegative">When true, a leading <c>-</c> at position 0 is permitted.</param>
    /// <param name="initialText">Initial content. Must be a valid numeric string for the given settings, or empty.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public NumberInputBox(float x, float y, float width, float height,
                          InputBoxTheme theme,
                          bool allowDecimals = true, bool allowNegative = false,
                          string initialText = "", int layer = 0)
        : base(x, y, width, height, theme, initialText, layer)
    {
        _allowDecimals = allowDecimals;
        _allowNegative = allowNegative;
    }

    /// <param name="rect">Position and size relative to the parent container.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="allowDecimals">When true, a single <c>.</c> character is permitted.</param>
    /// <param name="allowNegative">When true, a leading <c>-</c> at position 0 is permitted.</param>
    /// <param name="initialText">Initial content. Must be a valid numeric string for the given settings, or empty.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public NumberInputBox(Rect rect, InputBoxTheme theme,
                          bool allowDecimals = true, bool allowNegative = false,
                          string initialText = "", int layer = 0)
        : this(rect.X, rect.Y, rect.W, rect.H, theme, allowDecimals, allowNegative, initialText, layer)
    {
    }

    internal NumberInputBox(float x, float y, float width, float height,
                            bool allowDecimals = true, bool allowNegative = false)
        : base(x, y, width, height)
    {
        _allowDecimals = allowDecimals;
        _allowNegative = allowNegative;
    }

    // -------------------------------------------------------------------------
    // Properties

    /// <summary>
    /// The current numeric value, or <c>null</c> when the text is empty or not yet a
    /// complete number (e.g. just <c>"-"</c> or <c>"."</c>).
    /// </summary>
    public float? Value => float.TryParse(Text, System.Globalization.NumberStyles.Float,
                                          System.Globalization.CultureInfo.InvariantCulture,
                                          out float v) ? v : null;

    /// <summary>Optional lower bound. Not enforced during typing — apply in a <see cref="TextInputBox.Submitted"/> handler.</summary>
    public float? MinValue { get; set; }

    /// <summary>Optional upper bound. Not enforced during typing — apply in a <see cref="TextInputBox.Submitted"/> handler.</summary>
    public float? MaxValue { get; set; }

    // -------------------------------------------------------------------------
    // Override

    /// <inheritdoc/>
    protected override bool IsCharAllowed(char c)
    {
        if (char.IsDigit(c))
        {
            return true;
        }

        if (c == '.' && _allowDecimals && !Text.Contains('.'))
        {
            return true;
        }

        if (c == '-' && _allowNegative && CursorIndex == 0 && !Text.StartsWith('-'))
        {
            return true;
        }

        return false;
    }
}
