using System.Reflection;
using System.Text;

namespace BabyBearsEngine;

/// <summary>
/// Convenience helpers built on top of <see cref="IRandom"/>'s two primitives. Mirrors the
/// surface previously offered by the static <c>Randomisation</c> class, but bound to an
/// injected source so every helper is testable. Add new helpers here rather than as static
/// methods on a parallel class so all randomness flows through one swappable abstraction.
/// </summary>
public static class RandomExtensions
{
    private const string UpperCaseAsciiAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // Cached on first access. The reflection scan over Colour's static properties is cheap but
    // there's no reason to repeat it every time NamedColour is called.
    private static Colour[]? s_namedColours = null;

    // ─── Integer / double / float ranges ───

    /// <summary>Returns a random <c>int</c> in [0, <paramref name="maxExclusive"/>).</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxExclusive"/> is negative.</exception>
    public static int Int(this IRandom random, int maxExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxExclusive);
        if (maxExclusive == 0)
        {
            return 0;
        }
        return random.Int(0, maxExclusive);
    }

    /// <summary>Returns a random <c>double</c> in [0.0, <paramref name="maxExclusive"/>).</summary>
    public static double Double(this IRandom random, double maxExclusive)
    {
        return random.Double() * maxExclusive;
    }

    /// <summary>Returns a random <c>double</c> in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
    public static double Double(this IRandom random, double minInclusive, double maxExclusive)
    {
        return minInclusive + random.Double() * (maxExclusive - minInclusive);
    }

    /// <summary>Returns a random <c>float</c> in [0.0, 1.0).</summary>
    public static float Float(this IRandom random) => (float)random.Double();

    /// <summary>Returns a random <c>float</c> in [0.0, <paramref name="maxExclusive"/>).</summary>
    public static float Float(this IRandom random, float maxExclusive)
    {
        return (float)(random.Double() * maxExclusive);
    }

    /// <summary>Returns a random <c>float</c> in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
    public static float Float(this IRandom random, float minInclusive, float maxExclusive)
    {
        return minInclusive + (float)random.Double() * (maxExclusive - minInclusive);
    }

    // ─── Probability ───

    /// <summary>
    /// Returns <c>true</c> with probability <paramref name="probability"/> in [0, 1].
    /// Values ≤ 0 always return <c>false</c>; values ≥ 1 always return <c>true</c>. Use
    /// <see cref="ChancePercent"/> for the 0–100 form.
    /// </summary>
    public static bool Chance(this IRandom random, float probability)
    {
        if (probability <= 0f)
        {
            return false;
        }
        if (probability >= 1f)
        {
            return true;
        }
        return random.Double() < probability;
    }

    /// <summary>
    /// Returns <c>true</c> with probability <paramref name="percentage"/>%, where
    /// <paramref name="percentage"/> is in [0, 100]. Convenience over <see cref="Chance"/>
    /// for the percentage-friendly form common in game design.
    /// </summary>
    public static bool ChancePercent(this IRandom random, float percentage) => random.Chance(percentage / 100f);

    // ─── Selection ───

    /// <summary>Returns a uniformly-selected element from <paramref name="items"/>.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty.</exception>
    public static T Choose<T>(this IRandom random, params T[] items)
    {
        if (items.Length == 0)
        {
            throw new ArgumentException("Cannot choose from an empty collection.", nameof(items));
        }
        return items[random.Int(0, items.Length)];
    }

    /// <summary>Returns a uniformly-selected element from <paramref name="items"/>.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="items"/> is empty.</exception>
    public static T Choose<T>(this IRandom random, IList<T> items)
    {
        if (items.Count == 0)
        {
            throw new ArgumentException("Cannot choose from an empty collection.", nameof(items));
        }
        return items[random.Int(0, items.Count)];
    }

    /// <summary>Returns a uniformly-selected value of enum type <typeparamref name="T"/>.</summary>
    public static T Enum<T>(this IRandom random)
        where T : struct, Enum
    {
        T[] values = System.Enum.GetValues<T>();
        return values[random.Int(0, values.Length)];
    }

    // ─── Shuffle (Fisher–Yates, in-place) ───

    /// <summary>Shuffles <paramref name="array"/> in-place using Fisher–Yates and returns it.</summary>
    public static T[] Shuffle<T>(this IRandom random, T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int swapIndex = random.Int(0, i + 1);
            (array[i], array[swapIndex]) = (array[swapIndex], array[i]);
        }
        return array;
    }

    /// <summary>Shuffles <paramref name="list"/> in-place using Fisher–Yates.</summary>
    public static void Shuffle<T>(this IRandom random, IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Int(0, i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }

    // ─── Distributions ───

    /// <summary>
    /// Returns a random <c>float</c> in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>)
    /// using an approximate Gaussian distribution (cosine-based bell curve centred on the
    /// midpoint of the range). Returns <paramref name="maxExclusive"/> when the range is empty
    /// or inverted.
    /// </summary>
    public static float GaussianApprox(this IRandom random, float minInclusive, float maxExclusive)
    {
        if (maxExclusive <= minInclusive)
        {
            return maxExclusive;
        }

        // gaussian ≈ (1 + cos x) / 2π over [0, 2π]; the integral (x + sin x) / 2π maps a
        // uniform sample in [0, 1] to a bell-shaped distribution in [0, 1].
        double zeroTo2PI = 2 * Math.PI * random.Double();
        double zeroToOne = (zeroTo2PI + Math.Sin(zeroTo2PI)) / (2 * Math.PI);
        return minInclusive + (float)(zeroToOne * (maxExclusive - minInclusive));
    }

    // ─── Strings ───

    /// <summary>Returns a random uppercase ASCII (A–Z) string of length <paramref name="length"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is negative.</exception>
    public static string UpperCaseString(this IRandom random, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length == 0)
        {
            return string.Empty;
        }
        StringBuilder builder = new(length);
        for (int i = 0; i < length; i++)
        {
            builder.Append(UpperCaseAsciiAlphabet[random.Int(0, UpperCaseAsciiAlphabet.Length)]);
        }
        return builder.ToString();
    }

    // ─── Colour ───

    /// <summary>Returns a random <see cref="BabyBearsEngine.Colour"/> with random R, G, B in [0, 255] and A = 255.</summary>
    public static Colour Colour(this IRandom random)
    {
        return new Colour(
            (byte)random.Int(0, 256),
            (byte)random.Int(0, 256),
            (byte)random.Int(0, 256),
            byte.MaxValue);
    }

    /// <summary>
    /// Returns a uniformly-selected named static <see cref="BabyBearsEngine.Colour"/> property
    /// (e.g. <see cref="BabyBearsEngine.Colour.Red"/>, <see cref="BabyBearsEngine.Colour.Blue"/>).
    /// The set is discovered once via reflection on first call and cached for subsequent calls.
    /// </summary>
    public static Colour NamedColour(this IRandom random)
    {
        s_namedColours ??= BuildNamedColours();
        return random.Choose(s_namedColours);
    }

    private static Colour[] BuildNamedColours()
    {
        List<Colour> colours = [];
        foreach (PropertyInfo propertyInfo in typeof(Colour).GetProperties(BindingFlags.Static | BindingFlags.Public))
        {
            if (propertyInfo.GetValue(null) is Colour colour)
            {
                colours.Add(colour);
            }
        }
        return [.. colours];
    }
}
