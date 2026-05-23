namespace BabyBearsEngine.Geometry;

/// <summary>
/// Static geometry helpers that don't naturally belong on <see cref="Point"/> or <see cref="Rect"/>.
/// </summary>
public static class Maths
{
    /// <summary>Clamps <paramref name="value"/> to the inclusive [<paramref name="min"/>, <paramref name="max"/>] range.</summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The inclusive lower bound.</param>
    /// <param name="max">The inclusive upper bound.</param>
    public static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);

    /// <inheritdoc cref="Clamp(float, float, float)"/>
    public static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);

    /// <summary>
    /// Manhattan (grid / taxicab) distance between two points: <c>|a.X - b.X| + |a.Y - b.Y|</c>.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    public static float DistGrid(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    /// <inheritdoc cref="DistGrid(Point, Point)"/>
    public static float DistGrid(IPosition a, IPosition b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    /// <summary>
    /// Returns the cardinal direction of the side of <paramref name="rect"/> whose closest point to <paramref name="point"/>
    /// has the smallest Euclidean distance — i.e. which edge the point is geometrically nearest to.
    /// </summary>
    /// <remarks>
    /// Works for points outside, on, or inside the rectangle. The distance to each edge is computed as the distance from
    /// <paramref name="point"/> to the nearest point on the edge segment (perpendicular drop when the point projects onto
    /// the edge, otherwise the nearer endpoint). On a tie the result is, in order, <see cref="Direction.Left"/>,
    /// <see cref="Direction.Right"/>, <see cref="Direction.Up"/>, then <see cref="Direction.Down"/>.
    /// </remarks>
    public static Direction GetNearestSide(Rect rect, Point point)
    {
        Point closestOnTop = new(Math.Clamp(point.X, rect.Left, rect.Right), rect.Top);
        Point closestOnBottom = new(Math.Clamp(point.X, rect.Left, rect.Right), rect.Bottom);
        Point closestOnLeft = new(rect.Left, Math.Clamp(point.Y, rect.Top, rect.Bottom));
        Point closestOnRight = new(rect.Right, Math.Clamp(point.Y, rect.Top, rect.Bottom));

        float distTopSquared = (point - closestOnTop).LengthSquared;
        float distBottomSquared = (point - closestOnBottom).LengthSquared;
        float distLeftSquared = (point - closestOnLeft).LengthSquared;
        float distRightSquared = (point - closestOnRight).LengthSquared;

        float min = Math.Min(Math.Min(distLeftSquared, distRightSquared), Math.Min(distTopSquared, distBottomSquared));

        if (min == distLeftSquared)
        {
            return Direction.Left;
        }
        if (min == distRightSquared)
        {
            return Direction.Right;
        }
        if (min == distTopSquared)
        {
            return Direction.Up;
        }
        return Direction.Down;
    }
}
