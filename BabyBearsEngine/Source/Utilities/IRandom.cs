namespace BabyBearsEngine;

/// <summary>
/// Abstraction over a random number source. Inject this anywhere randomness drives behaviour
/// so tests can substitute a deterministic implementation. Use <see cref="SystemRandom"/> in
/// production, <see cref="FixedSequenceRandom"/> in unit tests, or a custom fake when bespoke
/// behaviour is needed.
/// </summary>
/// <remarks>
/// <para>The interface intentionally exposes only two primitives — an integer range and a
/// uniform-double in [0, 1). Everything higher-level (float ranges, weighted choices,
/// Fisher–Yates shuffle, Gaussian distribution, random colour / enum / string helpers) lives
/// in <see cref="RandomExtensions"/>, so implementers only need to satisfy a tiny contract
/// and the convenience surface is automatically available on every <see cref="IRandom"/>.</para>
///
/// <para>Implementations are not required to be thread-safe; callers serialise themselves.</para>
/// </remarks>
public interface IRandom
{
    /// <summary>
    /// Returns a random <c>int</c> in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minInclusive"/> is greater than <paramref name="maxExclusive"/>.</exception>
    int Int(int minInclusive, int maxExclusive);

    /// <summary>Returns a random <c>double</c> in [0.0, 1.0).</summary>
    double Double();
}
