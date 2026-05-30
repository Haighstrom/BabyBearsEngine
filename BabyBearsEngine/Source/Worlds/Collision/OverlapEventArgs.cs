namespace BabyBearsEngine.Worlds.Collision;

/// <summary>
/// Event payload describing an overlap state change between two colliders. <see cref="Self"/> is
/// the collider whose event is being raised; <see cref="Other"/> is the collider it now overlaps
/// (for <see cref="Collider.OverlapEntered"/>) or has stopped overlapping (for
/// <see cref="Collider.OverlapExited"/>).
/// </summary>
public sealed class OverlapEventArgs(Collider self, Collider other) : EventArgs
{
    /// <summary>The collider raising the event.</summary>
    public Collider Self { get; } = self;

    /// <summary>The collider on the other side of the overlap.</summary>
    public Collider Other { get; } = other;
}
