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
                Removed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Ensure.IsNull(_parent);
                _parent = value;
                OnAdded();
                Added?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <inheritdoc/>
    public bool Exists => _parent is not null;

    /// <inheritdoc/>
    public bool IsConnectedToTree
    {
        get
        {
            for (IAddable? current = this; current is not null; current = current.Parent as IAddable)
            {
                if (current.Parent is null)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <inheritdoc/>
    public event EventHandler? Added;

    /// <inheritdoc/>
    public event EventHandler? Removed;

    /// <summary>
    /// Called immediately after this object has been attached to a parent. Override to react
    /// to attachment synchronously; outside observers should subscribe to <see cref="Added"/>
    /// instead.
    /// </summary>
    protected virtual void OnAdded() { }

    /// <summary>
    /// Called immediately after this object has been removed from its parent. Override to react
    /// to removal synchronously; outside observers should subscribe to <see cref="Removed"/>
    /// instead.
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
