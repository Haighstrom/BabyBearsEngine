namespace BabyBearsEngine;

/// <summary>
/// Abstraction over a random number source. Use <see cref="SystemRandom"/> for production
/// and a test stub for deterministic unit tests.
/// </summary>
public interface IRandom
{
    /// <summary>Returns a random double in [0.0, 1.0).</summary>
    double NextDouble();
}
