namespace BabyBearsEngine.Utilities.Noise;

/// <summary>
/// Per-instance seeded 2D cellular (Voronoi) noise — places one feature point per integer cell at
/// a deterministic offset and returns the distance(s) to the closest features for any sample. Two
/// instances with the same seed produce identical output; sampling is thread-safe.
/// </summary>
/// <remarks>
/// Each integer cell contributes exactly one feature point at a hashed offset inside the cell.
/// Sampling considers the 3×3 neighbourhood of the sample's containing cell, which is sufficient
/// for points placed inside the unit cell. Distances are Euclidean; for a square-grid look pass
/// a low cell scale, for an irregular look pass a high one.
/// </remarks>
public sealed class VoronoiNoise
{
    private readonly int _seed;

    public VoronoiNoise() 
        : this(Randomisation.Rand(int.MaxValue))
    {
    }

    public VoronoiNoise(int seed)
    {
        _seed = seed;
    }

    public int Seed => _seed;

    /// <summary>
    /// Returns the Voronoi sample at (<paramref name="x"/>, <paramref name="y"/>). Both
    /// <see cref="VoronoiSample.F1"/> and <see cref="VoronoiSample.F2"/> are typically in [0, ~2]
    /// — F1 ≤ √2 within a 3×3 search, F2 a little larger.
    /// </summary>
    public VoronoiSample Sample(float x, float y)
    {
        int cellX = FastFloor(x);
        int cellY = FastFloor(y);

        float nearestSquared = float.MaxValue;
        float secondNearestSquared = float.MaxValue;

        for (int offsetY = -1; offsetY <= 1; offsetY++)
        {
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                int neighbourCellX = cellX + offsetX;
                int neighbourCellY = cellY + offsetY;

                float featurePointX = neighbourCellX + HashToUnit(neighbourCellX, neighbourCellY, dimension: 0);
                float featurePointY = neighbourCellY + HashToUnit(neighbourCellX, neighbourCellY, dimension: 1);

                float deltaX = featurePointX - x;
                float deltaY = featurePointY - y;
                float distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);

                if (distanceSquared < nearestSquared)
                {
                    secondNearestSquared = nearestSquared;
                    nearestSquared = distanceSquared;
                }
                else if (distanceSquared < secondNearestSquared)
                {
                    secondNearestSquared = distanceSquared;
                }
            }
        }

        return new VoronoiSample((float)Math.Sqrt(nearestSquared), (float)Math.Sqrt(secondNearestSquared));
    }

    private static int FastFloor(float value)
    {
        int truncated = (int)value;
        return value < truncated ? truncated - 1 : truncated;
    }

    // Mixes seed + cell coords + a dimension tag into a value in [0, 1). The dimension tag lets
    // a single call produce two independent coordinates (one for x, one for y) for the same cell.
    private float HashToUnit(int cellX, int cellY, int dimension)
    {
        unchecked
        {
            uint hash = (uint)_seed;
            hash ^= (uint)cellX * 374761393u;
            hash ^= (uint)cellY * 668265263u;
            hash ^= (uint)dimension * 1274126177u;
            hash = (hash ^ (hash >> 13)) * 1274126177u;
            hash ^= hash >> 16;
            // Mask to 24 bits and divide so the result fits cleanly into float precision in [0, 1).
            return (hash & 0xFFFFFFu) / (float)0x1000000;
        }
    }
}
