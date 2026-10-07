using System.Text;

namespace BabyBearsEngine;

/// <summary>
/// Encodes whole numbers as short text in Crockford's base32 alphabet, five bits per character.
/// The alphabet leaves out I, L, O and U, and decoding treats O as 0 and I/L as 1, so common
/// typing and reading mistakes are forgiven. Suited to codes a player copies or types by hand.
/// </summary>
public static class CrockfordBase32
{
    /// <summary>The 32 symbols, in value order: <c>0-9</c> then <c>A-Z</c> without I, L, O and U.</summary>
    public const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

    /// <summary>Bits of data carried by each character.</summary>
    public const int BitsPerCharacter = 5;

    /// <summary>The most characters <see cref="Encode"/> and <see cref="TryDecode"/> handle (12 characters = 60 bits).</summary>
    public const int MaxLength = 12;

    /// <summary>
    /// Writes <paramref name="value"/> as exactly <paramref name="length"/> characters, padding with leading
    /// zeros. Throws if <paramref name="length"/> is outside 1 to <see cref="MaxLength"/> or the value
    /// needs more than <c>length * 5</c> bits.
    /// </summary>
    public static string Encode(ulong value, int length)
    {
        Ensure.IsInRange(length, 1, MaxLength);

        if (value >> (length * BitsPerCharacter) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Value does not fit in {length} characters.");
        }

        char[] characters = new char[length];
        for (int index = length - 1; index >= 0; index--)
        {
            characters[index] = Alphabet[(int)(value & 0x1F)];
            value >>= BitsPerCharacter;
        }

        return new string(characters);
    }

    /// <summary>
    /// Cleans up typed or pasted code text: upper-cases it, drops spaces and dashes, and maps O to 0
    /// and I/L to 1. Other characters are left as they are, so <see cref="TryDecode"/> can reject them.
    /// </summary>
    public static string Normalise(string text)
    {
        StringBuilder normalised = new(text.Length);
        foreach (char c in text)
        {
            char upper = char.ToUpperInvariant(c);
            switch (upper)
            {
                case ' ':
                case '-':
                    break;
                case 'O':
                    normalised.Append('0');
                    break;
                case 'I':
                case 'L':
                    normalised.Append('1');
                    break;
                default:
                    normalised.Append(upper);
                    break;
            }
        }

        return normalised.ToString();
    }

    /// <summary>
    /// Reads <paramref name="text"/> (after <see cref="Normalise"/>) back into a number. Returns false for
    /// empty text, text longer than <see cref="MaxLength"/> characters, or any character outside the alphabet.
    /// </summary>
    public static bool TryDecode(string text, out ulong value)
    {
        value = 0;

        string normalised = Normalise(text);
        if (normalised.Length == 0 || normalised.Length > MaxLength)
        {
            return false;
        }

        ulong result = 0;
        foreach (char c in normalised)
        {
            int digit = Alphabet.IndexOf(c);
            if (digit < 0)
            {
                return false;
            }

            result = (result << BitsPerCharacter) | (uint)digit;
        }

        value = result;
        return true;
    }
}
