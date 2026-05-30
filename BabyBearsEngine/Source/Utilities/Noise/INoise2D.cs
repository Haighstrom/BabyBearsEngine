namespace BabyBearsEngine.Utilities.Noise;

/// <summary>
/// A continuous 2D noise function — given any real-valued coordinate, returns a deterministic
/// noise sample. Implementations should be smooth (samples close together produce similar values).
/// </summary>
public interface INoise2D
{
    /// <summary>
    /// Returns the noise value at <paramref name="x"/>, <paramref name="y"/>. The returned range
    /// is implementation-defined but is typically approximately [-1, 1] for gradient noises and
    /// [0, +∞) for cellular noises.
    /// </summary>
    float Sample(float x, float y);
}
