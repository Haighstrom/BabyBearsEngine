namespace BabyBearsEngine.Geometry;

/// <summary>
/// A circular collision shape defined by its centre and radius.
/// </summary>
public sealed class CircleShape(float centreX, float centreY, float radius) : ICollisionShape
{
    /// <summary>X-coordinate of the centre.</summary>
    public float CentreX { get; } = centreX;

    /// <summary>Y-coordinate of the centre.</summary>
    public float CentreY { get; } = centreY;

    /// <summary>Radius.</summary>
    public float Radius { get; } = radius;

    /// <summary>Centre point.</summary>
    public Point Centre => new(CentreX, CentreY);

    /// <inheritdoc/>
    public Rect Bounds => new(CentreX - Radius, CentreY - Radius, Radius * 2f, Radius * 2f);

    /// <summary>Creates a circular shape with the given centre and radius.</summary>
    public CircleShape(Point centre, float radius) 
        : this(centre.X, centre.Y, radius)
    {
    }

    /// <inheritdoc/>
    public bool ContainsPoint(Point point) => CollisionTools.PointVsCircle(point, this);

    /// <inheritdoc/>
    public bool Overlaps(ICollisionShape other) => other switch
    {
        CircleShape circle => CollisionTools.CircleVsCircle(this, circle),
        RectShape rect => CollisionTools.RectVsCircle(rect, this),
        _ => throw new NotSupportedException($"Overlap between CircleShape and {other.GetType().Name} is not supported."),
    };

    /// <inheritdoc/>
    public ICollisionShape Translate(float dx, float dy) => new CircleShape(CentreX + dx, CentreY + dy, Radius);
}
