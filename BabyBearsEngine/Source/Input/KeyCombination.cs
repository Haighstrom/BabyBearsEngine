using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BabyBearsEngine.Input;

/// <summary>
/// A primary <see cref="Keys"/> together with the <see cref="KeyModifiers"/> that must be held
/// alongside it (e.g. Ctrl+S, Ctrl+Shift+Z). Two combinations compare equal iff their
/// <see cref="Key"/> and <see cref="Modifiers"/> match.
/// </summary>
/// <remarks>
/// The <see cref="Key"/> itself should be the non-modifier key in the shortcut. Putting a modifier
/// key (e.g. <see cref="Keys.LeftControl"/>) in <see cref="Key"/> is legal but unusual — the
/// intended pattern is <c>new KeyCombination(Keys.S, KeyModifiers.Ctrl)</c> for "Ctrl+S".
/// </remarks>
public readonly record struct KeyCombination(Keys Key, KeyModifiers Modifiers = KeyModifiers.None)
{
    /// <summary>
    /// Renders the combination as a human-readable shortcut string (e.g. <c>"Ctrl+Shift+S"</c>).
    /// Modifiers appear in the fixed order Ctrl, Shift, Alt, followed by the key name.
    /// Round-trips through <see cref="TryParse"/>.
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new();

        if ((Modifiers & KeyModifiers.Ctrl) != 0)
        {
            sb.Append("Ctrl+");
        }
        if ((Modifiers & KeyModifiers.Shift) != 0)
        {
            sb.Append("Shift+");
        }
        if ((Modifiers & KeyModifiers.Alt) != 0)
        {
            sb.Append("Alt+");
        }

        sb.Append(Key.ToString());
        return sb.ToString();
    }

    /// <summary>
    /// Parses a shortcut string of the form <c>"Ctrl+Shift+S"</c> back into a
    /// <see cref="KeyCombination"/>. Modifier and key names are matched case-insensitively.
    /// "Control" is accepted as a synonym for "Ctrl". Whitespace around the <c>+</c> separators
    /// is allowed.
    /// </summary>
    /// <param name="text">The string to parse.</param>
    /// <param name="combination">The parsed combination, or default on failure.</param>
    /// <returns>True if <paramref name="text"/> parsed successfully; false otherwise.</returns>
    public static bool TryParse(string? text, out KeyCombination combination)
    {
        combination = default;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string[] parts = text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return false;
        }

        KeyModifiers modifiers = KeyModifiers.None;

        for (int partIndex = 0; partIndex < parts.Length - 1; partIndex++)
        {
            string part = parts[partIndex];

            if (part.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) || part.Equals("Control", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= KeyModifiers.Ctrl;
            }
            else if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= KeyModifiers.Shift;
            }
            else if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= KeyModifiers.Alt;
            }
            else
            {
                return false;
            }
        }

        string keyPart = parts[^1];
        if (!Enum.TryParse(keyPart, ignoreCase: true, out Keys key))
        {
            return false;
        }

        if (!Enum.IsDefined(key))
        {
            return false;
        }

        combination = new KeyCombination(key, modifiers);
        return true;
    }

    /// <summary>
    /// Parses a shortcut string in the same format as <see cref="TryParse"/>. Throws
    /// <see cref="FormatException"/> when the input is not a valid combination.
    /// </summary>
    public static KeyCombination Parse([NotNull] string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (!TryParse(text, out KeyCombination combination))
        {
            throw new FormatException($"'{text}' is not a valid key combination.");
        }

        return combination;
    }
}
