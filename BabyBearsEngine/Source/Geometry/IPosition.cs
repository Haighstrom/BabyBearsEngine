namespace BabyBearsEngine.Geometry;

/// <summary>
/// Type that exposes positional X and Y coordinates. Implemented by anything with a 2D position —
/// points, entities, pathfinding nodes, custom owner classes for colliders, etc.
/// </summary>
public interface IPosition
{
    /// <summary>
    /// The x-coordinate.
    /// </summary>
    float X { get; }

    /// <summary>
    /// The y-coordinate.
    /// </summary>
    float Y { get; }
}
