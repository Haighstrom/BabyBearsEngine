namespace BabyBearsEngine.Geometry;

public static class DirectionExtensions
{
    /// <summary>
    /// Returns the cardinal <see cref="Direction"/> that best matches the vector represented by
    /// this point. When the X and Y magnitudes are equal, the Y axis takes precedence.
    /// </summary>
    public static Direction ToDirection(this Point p)
    {
        if (Math.Abs(p.X) > Math.Abs(p.Y))
        {
            return p.X > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return p.Y > 0 ? Direction.Down : Direction.Up;
        }
    }
}
