namespace BabyBearsEngine;

/// <summary>
/// Production <see cref="IRandom"/> implementation backed by <see cref="System.Random"/>.
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
    public double NextDouble() => _random.NextDouble();
}
