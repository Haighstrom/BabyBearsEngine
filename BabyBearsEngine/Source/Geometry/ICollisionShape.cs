namespace BabyBearsEngine.Geometry;

/// <summary>
/// A 2D shape that can be tested for overlap with another shape via <see cref="Overlaps"/>.
/// Concrete implementations: <see cref="RectShape"/> and <see cref="CircleShape"/>. Typically
/// attached to a <see cref="BabyBearsEngine.Worlds.Collision.Collider"/> so a
/// <see cref="BabyBearsEngine.Worlds.Collision.CollisionSolver"/> raises overlap events on state
/// changes, but <see cref="Overlaps"/> can also be called directly for ad-hoc checks.
/// </summary>
/// <remarks>
/// Both shapes in an <see cref="Overlaps"/> call must be in the same coordinate space — the shape
/// has no concept of "local" or "world" itself and will silently produce nonsense if you mix them.
/// </remarks>
public interface ICollisionShape
{
    /// <summary>Axis-aligned bounding rectangle that fully contains this shape.</summary>
    Rect Bounds { get; }

    /// <summary>True if <paramref name="point"/> lies inside (or on the boundary of) this shape.</summary>
    bool ContainsPoint(Point point);

    /// <summary>True if this shape overlaps <paramref name="other"/>. Touching edges count as overlap.</summary>
    bool Overlaps(ICollisionShape other);

    /// <summary>Returns a new shape translated by (<paramref name="dx"/>, <paramref name="dy"/>).</summary>
    ICollisionShape Translate(float dx, float dy);
}
