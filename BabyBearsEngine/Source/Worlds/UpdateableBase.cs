namespace BabyBearsEngine.Worlds;

/// <summary>
/// Lightweight base class for logic objects that participate in the update loop and scene graph
/// but have no position or rendering concerns — timers, state machines, monitors, etc.
/// Inherits scene-graph membership (<see cref="IAddable"/>) from <see cref="AddableBase"/> and
/// update-loop participation (<see cref="IUpdateable"/>) directly.
/// </summary>
public abstract class UpdateableBase : AddableBase, IUpdateable
{
    /// <inheritdoc/>
    public virtual bool Active { get; set; } = true;

    /// <inheritdoc/>
    public abstract void Update(double elapsed);
}
