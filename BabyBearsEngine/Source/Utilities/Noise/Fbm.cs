namespace BabyBearsEngine.Utilities.Noise;

/// <summary>
/// Fractional Brownian motion — sums multiple octaves of an <see cref="INoise2D"/> source at
/// increasing frequency and decreasing amplitude. Builds detail on top of a base shape without
/// changing the source noise's overall range.
/// </summary>
public static class Fbm
{
    /// <summary>
    /// Returns an octave-summed sample at (<paramref name="x"/>, <paramref name="y"/>), normalised
    /// so the output keeps the same range as a single <paramref name="source"/> sample.
    /// </summary>
    /// <param name="source">Underlying noise to sample at each octave.</param>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <param name="octaves">Number of octaves to sum. Must be ≥ 1. More octaves = more detail and more cost.</param>
    /// <param name="frequency">Initial frequency multiplier applied to the input coordinates.</param>
    /// <param name="lacunarity">Frequency multiplier between consecutive octaves. 2 is the classic doubling-detail choice.</param>
    /// <param name="persistence">Amplitude multiplier between consecutive octaves. Lower = smoother (top octave dominates); higher = noisier.</param>
    public static float Sample(INoise2D source, float x, float y, int octaves, float frequency = 1f, float lacunarity = 2f, float persistence = 0.5f)
    {
        Ensure.ArgumentPositive(octaves, nameof(octaves));

        float currentFrequency = frequency;
        float currentAmplitude = 1f;
        float weightedSum = 0f;
        float amplitudeSum = 0f;

        for (int octave = 0; octave < octaves; octave++)
        {
            weightedSum += currentAmplitude * source.Sample(x * currentFrequency, y * currentFrequency);
            amplitudeSum += currentAmplitude;
            currentFrequency *= lacunarity;
            currentAmplitude *= persistence;
        }

        return weightedSum / amplitudeSum;
    }
}
