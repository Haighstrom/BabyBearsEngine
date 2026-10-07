using System.Text;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A <see cref="TextInputBox"/> that lays typed characters into a fixed pattern, inserting the
/// pattern's literal separators automatically (pattern <c>###-###</c> turns typed <c>K7Q2X9</c>
/// into <c>K7Q-2X9</c>).
/// </summary>
/// <remarks>
/// <para>In the pattern, <c>#</c> is a slot for one typed character and any other character is a
/// literal separator. Separators cannot be typed or pasted; they are stripped from input and
/// re-inserted wherever the pattern puts them, so pasting either <c>K7Q2X9</c> or <c>K7Q-2X9</c>
/// gives the same result. Input beyond the pattern's slot count is dropped.</para>
/// <para><see cref="TextInputBox.Text"/> is the formatted text including separators;
/// <see cref="RawText"/> is the same text with the separators removed.</para>
/// </remarks>
public class PatternInputBox : TextInputBox
{
    private const char SlotCharacter = '#';

    private readonly bool _forceUpperCase;
    private readonly Func<char, bool>? _isCharAllowed;
    private readonly HashSet<char> _literals;
    private readonly string _pattern;
    private readonly int _slotCount;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="pattern">Layout of the text: <c>#</c> for a typed character, anything else for a literal separator. Must contain at least one <c>#</c>.</param>
    /// <param name="isCharAllowed">Optional filter on typed characters; null accepts any character that is not a separator.</param>
    /// <param name="forceUpperCase">When true, typed letters are converted to upper case.</param>
    /// <param name="initialText">Initial content, with or without separators.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public PatternInputBox(float x, float y, float width, float height,
                           InputBoxTheme theme, string pattern,
                           Func<char, bool>? isCharAllowed = null, bool forceUpperCase = false,
                           string initialText = "", int layer = 0)
        : base(x, y, width, height, theme, layer: layer)
    {
        (_pattern, _slotCount, _literals) = ParsePattern(pattern);
        _isCharAllowed = isCharAllowed;
        _forceUpperCase = forceUpperCase;
        MaxLength = _pattern.Length;
        Text = initialText;
    }

    /// <param name="rect">Position and size relative to the parent container.</param>
    /// <param name="theme">Visual styling.</param>
    /// <param name="pattern">Layout of the text: <c>#</c> for a typed character, anything else for a literal separator. Must contain at least one <c>#</c>.</param>
    /// <param name="isCharAllowed">Optional filter on typed characters; null accepts any character that is not a separator.</param>
    /// <param name="forceUpperCase">When true, typed letters are converted to upper case.</param>
    /// <param name="initialText">Initial content, with or without separators.</param>
    /// <param name="layer">Initial render layer. Higher = further behind, lower = on top, 0 = default top. Must be ≥ 0.</param>
    public PatternInputBox(Rect rect, InputBoxTheme theme, string pattern,
                           Func<char, bool>? isCharAllowed = null, bool forceUpperCase = false,
                           string initialText = "", int layer = 0)
        : this(rect.X, rect.Y, rect.W, rect.H, theme, pattern, isCharAllowed, forceUpperCase, initialText, layer)
    {
    }

    internal PatternInputBox(float x, float y, float width, float height, string pattern,
                             Func<char, bool>? isCharAllowed = null, bool forceUpperCase = false)
        : base(x, y, width, height)
    {
        (_pattern, _slotCount, _literals) = ParsePattern(pattern);
        _isCharAllowed = isCharAllowed;
        _forceUpperCase = forceUpperCase;
        MaxLength = _pattern.Length;
    }

    /// <summary>The text with the pattern's separators removed.</summary>
    public string RawText => StripLiterals(Text);

    /// <inheritdoc/>
    /// <remarks>
    /// The setter accepts text with or without separators, drops characters the box would not accept
    /// from the keyboard, and lays the rest into the pattern.
    /// </remarks>
    public override string Text
    {
        get => base.Text;
        set => base.Text = Format(Filter(value));
    }

    /// <inheritdoc/>
    protected override string FormatEditedText(string text, ref int cursorIndex)
    {
        int clampedCursor = Math.Clamp(cursorIndex, 0, text.Length);
        int rawCharsBeforeCursor = StripLiterals(text[..clampedCursor]).Length;

        string formatted = Format(StripLiterals(text));
        cursorIndex = IndexAfterRawChars(formatted, rawCharsBeforeCursor);
        return formatted;
    }

    /// <inheritdoc/>
    protected override bool IsCharAllowed(char c)
    {
        return !_literals.Contains(c) && (_isCharAllowed?.Invoke(c) ?? true);
    }

    private string Filter(string input)
    {
        StringBuilder accepted = new(input.Length);
        foreach (char c in input)
        {
            if (IsCharAllowed(c))
            {
                accepted.Append(c);
            }
        }

        return accepted.ToString();
    }

    private string Format(string rawText)
    {
        string raw = rawText.Length > _slotCount ? rawText[.._slotCount] : rawText;
        if (_forceUpperCase)
        {
            raw = raw.ToUpperInvariant();
        }

        StringBuilder formatted = new(_pattern.Length);
        int rawIndex = 0;
        foreach (char patternChar in _pattern)
        {
            if (rawIndex >= raw.Length)
            {
                break;
            }

            if (patternChar == SlotCharacter)
            {
                formatted.Append(raw[rawIndex]);
                rawIndex++;
            }
            else
            {
                formatted.Append(patternChar);
            }
        }

        return formatted.ToString();
    }

    private int IndexAfterRawChars(string formatted, int rawCharCount)
    {
        if (rawCharCount == 0)
        {
            return 0;
        }

        int seen = 0;
        for (int index = 0; index < formatted.Length; index++)
        {
            if (!_literals.Contains(formatted[index]))
            {
                seen++;
                if (seen == rawCharCount)
                {
                    return index + 1;
                }
            }
        }

        return formatted.Length;
    }

    private static (string Pattern, int SlotCount, HashSet<char> Literals) ParsePattern(string pattern)
    {
        Ensure.ArgumentNotNullOrEmpty(pattern, nameof(pattern));

        int slotCount = pattern.Count(c => c == SlotCharacter);
        if (slotCount == 0)
        {
            throw new ArgumentException($"Pattern must contain at least one '{SlotCharacter}' slot.", nameof(pattern));
        }

        return (pattern, slotCount, pattern.Where(c => c != SlotCharacter).ToHashSet());
    }

    private string StripLiterals(string text)
    {
        StringBuilder stripped = new(text.Length);
        foreach (char c in text)
        {
            if (!_literals.Contains(c))
            {
                stripped.Append(c);
            }
        }

        return stripped.ToString();
    }
}
