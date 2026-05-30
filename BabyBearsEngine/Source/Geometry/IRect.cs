namespace BabyBearsEngine.Geometry;

/// <summary>
/// Anything that has a position and a size — a rectangle in 2D space. Entities, graphics,
/// cameras, layout cells all implement this so utilities (layout, hit-testing, debug
/// drawing) can target "anything sized" without picking a side.
/// </summary>
/// <remarks>
/// <para>Rectangle algebra (Intersects, Contains, Grow, etc.) lives on the <see cref="Rect"/>
/// value type, not on this interface. Implementers expose only the four geometry fields and
/// a handful of computed conveniences; callers wanting algebra construct a <see cref="Rect"/>
/// or operate on the fields directly.</para>
/// <para>Coordinates use a top-left origin: increasing <see cref="Y"/> moves downward, so
/// <see cref="Bottom"/> = <see cref="Y"/> + <see cref="Height"/>.</para>
/// </remarks>
public interface IRect : IPosition
{
    /// <summary>X position in the parent's local space.</summary>
    new float X { get; set; }

    /// <summary>Y position in the parent's local space.</summary>
    new float Y { get; set; }

    /// <summary>Width in pixels.</summary>
    float Width { get; set; }

    /// <summary>Height in pixels.</summary>
    float Height { get; set; }

    /// <summary>Right edge — <see cref="X"/> + <see cref="Width"/>.</summary>
    float Right => X + Width;

    /// <summary>Bottom edge — <see cref="Y"/> + <see cref="Height"/>.</summary>
    float Bottom => Y + Height;

    /// <summary>Centre point.</summary>
    Point Centre => new(X + Width * 0.5f, Y + Height * 0.5f);
}
