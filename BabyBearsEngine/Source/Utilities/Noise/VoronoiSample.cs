namespace BabyBearsEngine.Utilities.Noise;

/// <summary>
/// One sample from a <see cref="VoronoiNoise"/> lookup. <see cref="F1"/> is the distance to the
/// nearest feature point; <see cref="F2"/> is the distance to the second-nearest. Both are in the
/// same units as the input coordinates.
/// </summary>
/// <param name="F1">Distance to the nearest feature point.</param>
/// <param name="F2">Distance to the second-nearest feature point.</param>
public readonly record struct VoronoiSample(float F1, float F2)
{
    /// <summary>
    /// The Voronoi cell-edge metric: F2 - F1. Zero exactly on a cell boundary and grows toward
    /// the cell interior — useful for drawing crisp cell outlines.
    /// </summary>
    public float EdgeDistance => F2 - F1;
}
