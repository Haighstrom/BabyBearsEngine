namespace BabyBearsEngine;

/// <summary>
/// Static facade over the engine-wide <see cref="IRandom"/> source. Every method delegates to
/// <c>EngineConfiguration.RandomService</c>, so substituting the underlying source (production
/// <see cref="SystemRandom"/>, a seeded <see cref="SystemRandom"/> for replays, a
/// <see cref="FixedSequenceRandom"/> for deterministic tests) automatically reroutes every
/// caller. Mirrors <see cref="Audio"/> in shape.
/// </summary>
/// <remarks>
/// The convenience surface here matches the pre-facade <c>Randomisation</c> class for
/// migration ease, but the full set of helpers (including new names like <c>Int</c>,
/// <c>Double</c>, <c>Float</c>, <c>Choose</c>) is also available directly on any
/// <see cref="IRandom"/> via <see cref="RandomExtensions"/>. Inject <see cref="IRandom"/>
/// into your classes when you need a test seam at object level rather than at process level.
/// </remarks>
public static class Randomisation
{
    private static IRandom Source => EngineConfiguration.RandomService;

    /// <summary>Returns <c>true</c> with probability <paramref name="chanceOutOfAHundred"/>%.</summary>
    public static bool Chance(float chanceOutOfAHundred) => Source.ChancePercent(chanceOutOfAHundred);

    /// <summary>Returns a randomly selected element from <paramref name="things"/>.</summary>
    public static T Choose<T>(params T[] things) => Source.Choose(things);

    /// <summary>Returns a randomly selected element from <paramref name="things"/>.</summary>
    public static T Choose<T>(List<T> things) => Source.Choose((IList<T>)things);

    /// <summary>Returns a random <c>double</c> in [0, <paramref name="max"/>).</summary>
    public static double RandD(double max) => Source.Double(max);

    /// <summary>Returns a random <c>int</c> in [<paramref name="min"/>, <paramref name="max"/>).</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="max"/> is less than <paramref name="min"/>.</exception>
    public static int Rand(int min, int max) => Source.Int(min, max);

    /// <summary>Returns a random <c>int</c> in [0, <paramref name="max"/>). Returns 0 when <paramref name="max"/> is 0 or less.</summary>
    public static int Rand(int max) => max <= 0 ? 0 : Source.Int(max);

    /// <summary>Returns a randomly selected enum value of type <typeparamref name="T"/>.</summary>
    public static T RandEnum<T>()
        where T : struct, Enum
        => Source.Enum<T>();

    /// <summary>Returns a random <see cref="Colour"/> with random R, G, B components and full opacity.</summary>
    public static Colour RandColour() => Source.Colour();

    /// <summary>Returns a uniformly-selected named static <see cref="Colour"/> property (e.g. <see cref="Colour.Red"/>).</summary>
    public static Colour RandNamedColour() => Source.NamedColour();

    /// <summary>Returns a random <c>float</c> in [0, <paramref name="max"/>).</summary>
    public static float RandF(int max) => Source.Float(max);

    /// <summary>Returns a random <c>float</c> in [0, <paramref name="max"/>).</summary>
    public static float RandF(float max) => Source.Float(max);

    /// <summary>
    /// Returns a random <c>float</c> in [<paramref name="min"/>, <paramref name="max"/>].
    /// Returns <paramref name="max"/> when <paramref name="max"/> ≤ <paramref name="min"/>.
    /// </summary>
    public static float RandF(int min, int max) => max <= min ? max : Source.Float(min, max);

    /// <summary>
    /// Returns a random <c>float</c> in [<paramref name="min"/>, <paramref name="max"/>].
    /// Returns <paramref name="max"/> when <paramref name="max"/> ≤ <paramref name="min"/>.
    /// </summary>
    public static float RandF(float min, float max) => max <= min ? max : Source.Float(min, max);

    /// <summary>
    /// Returns a random <c>float</c> in [<paramref name="min"/>, <paramref name="max"/>] using an
    /// approximate Gaussian distribution (cosine-based bell curve centred on the midpoint).
    /// Returns <paramref name="max"/> when <paramref name="max"/> ≤ <paramref name="min"/>.
    /// </summary>
    public static float RandGaussianApprox(float min, float max) => Source.GaussianApprox(min, max);

    /// <summary>Returns a random uppercase ASCII string of <paramref name="chars"/> characters.</summary>
    public static string RandUpperCaseString(int chars) => Source.UpperCaseString(chars);

    /// <summary>Shuffles <paramref name="array"/> in-place using Fisher–Yates and returns it.</summary>
    public static T[] Shuffle<T>(T[] array) => Source.Shuffle(array);

    /// <summary>Shuffles <paramref name="list"/> in-place using Fisher–Yates and returns it.</summary>
    public static List<T> Shuffle<T>(List<T> list)
    {
        Source.Shuffle(list);
        return list;
    }

    /// <summary>Returns a randomly selected element from <paramref name="list"/>.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="list"/> is empty.</exception>
    public static T RandomElement<T>(this IList<T> list) => Source.Choose(list);

    /// <summary>Shuffles <paramref name="list"/> in-place using Fisher–Yates.</summary>
    public static void Shuffle<T>(this IList<T> list) => Source.Shuffle(list);
}
