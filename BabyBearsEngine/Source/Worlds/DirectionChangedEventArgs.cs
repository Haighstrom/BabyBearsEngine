using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>Payload for <see cref="IWaypointController.DirectionChanged"/>.</summary>
public sealed class DirectionChangedEventArgs(Direction? oldDirection, Direction newDirection) : EventArgs
{
    /// <summary>The direction the entity was moving before the change, or <c>null</c> if movement is just starting.</summary>
    public Direction? OldDirection { get; } = oldDirection;

    /// <summary>The direction the entity is now moving.</summary>
    public Direction NewDirection { get; } = newDirection;
}
