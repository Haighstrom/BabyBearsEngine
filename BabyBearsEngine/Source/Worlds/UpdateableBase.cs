namespace BabyBearsEngine.Worlds;

/// <summary>
/// Lightweight base class for objects that participate in the update loop but are not scene-graph
/// nodes. Use this when an object needs <see cref="IUpdateable"/> but has no position, rendering,
/// or parent-container concerns — plain logic objects, timers, state machines, etc.
/// </summary>
public abstract class UpdateableBase : IUpdateable
{
    /// <inheritdoc/>
    public virtual bool Active { get; set; } = true;

    /// <inheritdoc/>
    public abstract void Update(double elapsed);
}
