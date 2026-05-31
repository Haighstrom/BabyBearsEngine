namespace BabyBearsEngine;

/// <summary>
/// Production <see cref="IRandom"/> implementation backed by <see cref="System.Random"/>.
/// Construct without arguments for non-deterministic randomness, or pass a <paramref name="seed"/>
/// for a reproducible sequence (useful for replays and seed-based world generation).
/// </summary>
public sealed class SystemRandom : IRandom
{
    private readonly Random _random;

    public SystemRandom()
    {
        _random = new Random();
    }

    public SystemRandom(int seed)
    {
        _random = new Random(seed);
    }

    /// <inheritdoc/>
    public int Int(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);

    /// <inheritdoc/>
    public double Double() => _random.NextDouble();
}
