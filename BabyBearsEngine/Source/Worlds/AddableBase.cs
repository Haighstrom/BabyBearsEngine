namespace BabyBearsEngine.Worlds;

/// <summary>
/// Default <see cref="IAddable"/> implementation. Inherit when building entities/graphics so the
/// parent-tracking and self-removal plumbing comes for free.
/// </summary>
public abstract class AddableBase : IAddable
{
    private IContainer? _parent;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when assigning a non-null parent while <see cref="Parent"/> is already set — cannot switch parents directly; detach first by setting to <c>null</c>.</exception>
    /// <exception cref="NullReferenceException">Thrown when assigning <c>null</c> while <see cref="Parent"/> is already <c>null</c> — already detached.</exception>
    public IContainer? Parent
    {
        get => _parent;
        set
        {
            // Only allow flipping between having a parent and not having a parent; never switch
            // directly from one parent to another without detaching first.
            if (value is null)
            {
                Ensure.NotNull(_parent);
                _parent = null;
                OnRemoved();
            }
            else
            {
                Ensure.IsNull(_parent);
                _parent = value;
            }
        }
    }

    /// <inheritdoc/>
    public bool Exists => _parent is not null;

    /// <summary>
    /// Called immediately after this object has been removed from its parent. Override to react
    /// to removal without needing to override <see cref="Remove"/> and remember to call
    /// <c>base.Remove()</c>.
    /// </summary>
    protected virtual void OnRemoved() { }

    /// <inheritdoc/>
    /// <exception cref="NullReferenceException">Thrown when this addable has no parent to remove from.</exception>
    public void Remove()
    {
        Ensure.NotNull(_parent);

        _parent.Remove(this);
    }
}
