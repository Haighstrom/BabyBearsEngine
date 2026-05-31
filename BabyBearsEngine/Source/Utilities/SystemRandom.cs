using System.Threading;

namespace BabyBearsEngine;

/// <summary>
/// Production <see cref="IRandom"/> implementation backed by <see cref="System.Random"/>.
/// Construct without arguments for non-deterministic randomness, or pass a <paramref name="seed"/>
/// for a reproducible sequence (useful for replays and seed-based world generation).
/// </summary>
public sealed class SystemRandom : IRandom
{
    // System.Random instances are not thread-safe; concurrent access can corrupt internal state
    // and start returning zeros. The engine update loop is single-threaded but loaders/audio
    // threads may still hit this, so all access goes through _lock.
    private readonly Lock _lock = new();
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
    public int Int(int minInclusive, int maxExclusive)
    {
        lock (_lock)
        {
            return _random.Next(minInclusive, maxExclusive);
        }
    }

    /// <inheritdoc/>
    public double Double()
    {
        lock (_lock)
        {
            return _random.NextDouble();
        }
    }
}
