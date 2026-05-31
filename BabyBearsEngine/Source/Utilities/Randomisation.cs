namespace BabyBearsEngine;

/// <summary>
/// Static facade over the engine-wide <see cref="IRandom"/> source. Every method is a pure
/// delegate to <c>EngineConfiguration.RandomService</c> via <see cref="RandomExtensions"/>, so
/// substituting the underlying source (production <see cref="SystemRandom"/>, a seeded
/// <see cref="SystemRandom"/> for replays, a <see cref="FixedSequenceRandom"/> for deterministic
/// tests) automatically reroutes every caller. Mirrors <see cref="Audio"/> in shape.
/// </summary>
/// <remarks>
/// Method names match those on <see cref="IRandom"/> / <see cref="RandomExtensions"/> exactly,
/// so callers can use either path interchangeably. Inject <see cref="IRandom"/> into your
/// classes when you need a test seam at object level rather than at process level.
/// </remarks>
public static class Randomisation
{
    private static IRandom Source => EngineConfiguration.RandomService;

    // ─── Integer / double / float ranges ───

    /// <summary>Returns a random <c>int</c> in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
    public static int Int(int minInclusive, int maxExclusive) => Source.Int(minInclusive, maxExclusive);

    /// <summary>Returns a random <c>int</c> in [0, <paramref name="maxExclusive"/>).</summary>
    public static int Int(int maxExclusive) => Source.Int(maxExclusive);

    /// <summary>Returns a random <c>double</c> in [0.0, 1.0).</summary>
    public static double Double() => Source.Double();

    /// <summary>Returns a random <c>double</c> in [0.0, <paramref name="maxExclusive"/>).</summary>
    public static double Double(double maxExclusive) => Source.Double(maxExclusive);

    /// <summary>Returns a random <c>double</c> in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
    public static double Double(double minInclusive, double maxExclusive) => Source.Double(minInclusive, maxExclusive);

    /// <summary>Returns a random <c>float</c> in [0.0, 1.0).</summary>
    public static float Float() => Source.Float();

    /// <summary>Returns a random <c>float</c> in [0.0, <paramref name="maxExclusive"/>).</summary>
    public static float Float(float maxExclusive) => Source.Float(maxExclusive);

    /// <summary>Returns a random <c>float</c> in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
    public static float Float(float minInclusive, float maxExclusive) => Source.Float(minInclusive, maxExclusive);

    // ─── Probability ───

    /// <summary>Returns <c>true</c> with probability <paramref name="probability"/> in [0, 1].</summary>
    public static bool Chance(float probability) => Source.Chance(probability);

    /// <summary>Returns <c>true</c> with probability <paramref name="percentage"/>%, where <paramref name="percentage"/> is in [0, 100].</summary>
    public static bool ChancePercent(float percentage) => Source.ChancePercent(percentage);

    // ─── Selection ───

    /// <summary>
    /// Returns a uniformly-selected element from <paramref name="items"/>. Use this overload for
    /// literal argument lists (<c>Choose("a", "b", "c")</c>); for an existing collection variable,
    /// call <see cref="RandomElement{T}"/> on the list to avoid ambiguity with the params form.
    /// </summary>
    public static T Choose<T>(params T[] items) => Source.Choose(items);

    /// <summary>Returns a uniformly-selected value of enum type <typeparamref name="T"/>.</summary>
    public static T Enum<T>()
        where T : struct, Enum
        => Source.Enum<T>();

    // ─── Shuffle (Fisher–Yates, in-place) ───

    /// <summary>Shuffles <paramref name="array"/> in-place using Fisher–Yates and returns it.</summary>
    public static T[] Shuffle<T>(T[] array) => Source.Shuffle(array);

    /// <summary>Shuffles <paramref name="list"/> in-place using Fisher–Yates and returns it.</summary>
    public static IList<T> Shuffle<T>(this IList<T> list) => Source.Shuffle(list);

    /// <summary>Returns a uniformly-selected element from <paramref name="list"/>.</summary>
    public static T RandomElement<T>(this IReadOnlyList<T> list) => Source.Choose(list);

    // ─── Distributions ───

    /// <summary>
    /// Returns a random <c>float</c> in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>)
    /// using an approximate Gaussian distribution (cosine-based bell curve centred on the midpoint).
    /// </summary>
    public static float GaussianApprox(float minInclusive, float maxExclusive) => Source.GaussianApprox(minInclusive, maxExclusive);

    // ─── Strings ───

    /// <summary>Returns a random uppercase ASCII (A–Z) string of length <paramref name="length"/>.</summary>
    public static string UpperCaseString(int length) => Source.UpperCaseString(length);

    // ─── Colour ───

    /// <summary>Returns a random <see cref="BabyBearsEngine.Colour"/> with random R, G, B in [0, 255] and A = 255.</summary>
    public static Colour Colour() => Source.Colour();

    /// <summary>Returns a uniformly-selected named static <see cref="BabyBearsEngine.Colour"/> property (e.g. <see cref="BabyBearsEngine.Colour.Red"/>).</summary>
    public static Colour NamedColour() => Source.NamedColour();
}
