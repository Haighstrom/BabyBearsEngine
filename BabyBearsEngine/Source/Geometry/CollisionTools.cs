namespace BabyBearsEngine.Geometry;

/// <summary>
/// Pure overlap-detection helpers for the built-in collision shapes. All checks are inclusive —
/// shapes that touch exactly along an edge or at a single point count as overlapping.
/// </summary>
public static class CollisionTools
{
    /// <summary>True if circles <paramref name="a"/> and <paramref name="b"/> overlap or touch.</summary>
    public static bool CircleVsCircle(CircleShape a, CircleShape b)
    {
        float dx = a.CentreX - b.CentreX;
        float dy = a.CentreY - b.CentreY;
        float sumRadii = a.Radius + b.Radius;
        return (dx * dx) + (dy * dy) <= sumRadii * sumRadii;
    }

    /// <summary>True if <paramref name="point"/> lies inside or on the boundary of <paramref name="circle"/>.</summary>
    public static bool PointVsCircle(Point point, CircleShape circle)
    {
        float dx = point.X - circle.CentreX;
        float dy = point.Y - circle.CentreY;
        return (dx * dx) + (dy * dy) <= circle.Radius * circle.Radius;
    }

    /// <summary>True if <paramref name="point"/> lies inside or on the boundary of <paramref name="rect"/>.</summary>
    public static bool PointVsRect(Point point, RectShape rect)
    {
        return point.X >= rect.X
            && point.X <= rect.Right
            && point.Y >= rect.Y
            && point.Y <= rect.Bottom;
    }

    /// <summary>True if <paramref name="rect"/> and <paramref name="circle"/> overlap or touch.</summary>
    public static bool RectVsCircle(RectShape rect, CircleShape circle)
    {
        float closestX = Math.Clamp(circle.CentreX, rect.X, rect.Right);
        float closestY = Math.Clamp(circle.CentreY, rect.Y, rect.Bottom);
        float dx = circle.CentreX - closestX;
        float dy = circle.CentreY - closestY;
        return (dx * dx) + (dy * dy) <= circle.Radius * circle.Radius;
    }

    /// <summary>True if rectangles <paramref name="a"/> and <paramref name="b"/> overlap or touch.</summary>
    public static bool RectVsRect(RectShape a, RectShape b)
    {
        return a.X <= b.Right
            && a.Right >= b.X
            && a.Y <= b.Bottom
            && a.Bottom >= b.Y;
    }
}
