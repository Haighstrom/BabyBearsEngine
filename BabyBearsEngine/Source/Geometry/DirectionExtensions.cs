namespace BabyBearsEngine.Geometry;

/// <summary>Helpers for the <see cref="Direction"/> enum and for translating geometry by a cardinal direction.</summary>
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

    /// <summary>Returns the direction facing the opposite way.</summary>
    public static Direction Opposite(this Direction direction) => direction switch
    {
        Direction.Up => Direction.Down,
        Direction.Right => Direction.Left,
        Direction.Down => Direction.Up,
        Direction.Left => Direction.Right,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
    };

    /// <summary>Returns a copy of <paramref name="point"/> shifted by <paramref name="distance"/> in <paramref name="direction"/>.</summary>
    /// <remarks>Y grows downward — <see cref="Direction.Up"/> subtracts from Y.</remarks>
    public static Point Shift(this Point point, Direction direction, float distance) => direction switch
    {
        Direction.Up => point.Shift(0, -distance),
        Direction.Right => point.Shift(distance, 0),
        Direction.Down => point.Shift(0, distance),
        Direction.Left => point.Shift(-distance, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
    };

    /// <summary>Returns a copy of <paramref name="rect"/> shifted by <paramref name="distance"/> in <paramref name="direction"/>.</summary>
    /// <remarks>Y grows downward — <see cref="Direction.Up"/> subtracts from Y.</remarks>
    public static Rect Shift(this Rect rect, Direction direction, float distance) => direction switch
    {
        Direction.Up => rect.Shift(0, -distance),
        Direction.Right => rect.Shift(distance, 0),
        Direction.Down => rect.Shift(0, distance),
        Direction.Left => rect.Shift(-distance, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
    };
}
