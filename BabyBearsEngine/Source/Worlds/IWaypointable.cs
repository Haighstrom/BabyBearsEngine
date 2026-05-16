namespace BabyBearsEngine.Worlds;

/// <summary>
/// Implemented by entities that can be driven along a path by a <see cref="IWaypointController"/>.
/// </summary>
public interface IWaypointable
{
    /// <summary>Movement speed in pixels per second.</summary>
    float Speed { get; }

    /// <summary>X position in world space. The controller writes this each frame.</summary>
    float X { get; set; }

    /// <summary>Y position in world space. The controller writes this each frame.</summary>
    float Y { get; set; }
}
