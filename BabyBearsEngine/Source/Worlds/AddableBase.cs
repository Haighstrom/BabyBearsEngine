namespace BabyBearsEngine.Worlds;

/// <summary>
/// Default <see cref="IAddable"/> implementation. Inherit when building entities/graphics so the
/// parent-tracking and self-removal plumbing comes for free.
/// </summary>
public abstract class AddableBase : IAddable
{
    /// <inheritdoc/>
    public IContainer? Parent { get; private set; }

    /// <inheritdoc/>
    public bool Exists => Parent is not null;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="container"/> is non-null while <see cref="Parent"/> is already set — cannot switch parents directly; detach first.</exception>
    /// <exception cref="NullReferenceException">Thrown when <paramref name="container"/> is null and <see cref="Parent"/> is also null — already detached.</exception>
    public void SetParent(IContainer? container)
    {
        //only allow flipping between having a parent and not having a parent, not switching from one parent to another without first removing from the first
        if (container is null)
        {
            Ensure.NotNull(Parent);
        }
        else
        {
            Ensure.IsNull(Parent);
        }

        Parent = container;
    }

    /// <inheritdoc/>
    /// <exception cref="NullReferenceException">Thrown when this addable has no parent to remove from.</exception>
    public void Remove()
    {
        Ensure.NotNull(Parent);

        Parent.Remove(this);
    }
}
