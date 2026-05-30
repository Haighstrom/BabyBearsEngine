namespace BabyBearsEngine.Geometry;

/// <summary>
/// An axis-aligned rectangular collision shape. Uses the same top-left origin convention as
/// <see cref="Rect"/>: increasing <see cref="Y"/> moves downward.
/// </summary>
public sealed class RectShape(float x, float y, float width, float height) : ICollisionShape
{
    /// <summary>X-coordinate of the top-left corner.</summary>
    public float X { get; } = x;

    /// <summary>Y-coordinate of the top-left corner.</summary>
    public float Y { get; } = y;

    /// <summary>Width.</summary>
    public float Width { get; } = width;

    /// <summary>Height.</summary>
    public float Height { get; } = height;

    /// <summary>Right edge — <see cref="X"/> + <see cref="Width"/>.</summary>
    public float Right => X + Width;

    /// <summary>Bottom edge — <see cref="Y"/> + <see cref="Height"/>.</summary>
    public float Bottom => Y + Height;

    /// <inheritdoc/>
    public Rect Bounds => new(X, Y, Width, Height);

    /// <summary>Creates a rectangular shape from a <see cref="Rect"/>.</summary>
    public RectShape(Rect rect) 
        : this(rect.X, rect.Y, rect.W, rect.H)
    {
    }

    /// <inheritdoc/>
    public bool ContainsPoint(Point point) => CollisionTools.PointVsRect(point, this);

    /// <inheritdoc/>
    public bool Overlaps(ICollisionShape other) => other switch
    {
        RectShape rect => CollisionTools.RectVsRect(this, rect),
        CircleShape circle => CollisionTools.RectVsCircle(this, circle),
        _ => throw new NotSupportedException($"Overlap between RectShape and {other.GetType().Name} is not supported."),
    };

    /// <inheritdoc/>
    public ICollisionShape Translate(float dx, float dy) => new RectShape(X + dx, Y + dy, Width, Height);
}
