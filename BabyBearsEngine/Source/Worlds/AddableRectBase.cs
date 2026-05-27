namespace BabyBearsEngine.Worlds;

/// <summary>
/// Default <see cref="IRectAddable"/> implementation — combines the parent-tracking plumbing
/// from <see cref="AddableBase"/> with float position and size fields. Subclasses
/// (<c>GraphicBase</c>, <c>ContainerEntity</c>) inherit this so the entity tree and the
/// graphic tree share one X/Y/Width/Height representation.
/// </summary>
/// <remarks>
/// <para>Subclasses that need to react to position or size changes should override the
/// virtual <see cref="OnPositionChanged"/> / <see cref="OnSizeChanged"/> hooks. Concrete
/// graphic types do this to flip their "rebuild vertices" flag without subscribing to
/// their own events.</para>
/// </remarks>
public abstract class AddableRectBase : AddableBase, IRectAddable
{
    private float _x = 0f;
    private float _y = 0f;
    private float _width = 0f;
    private float _height = 0f;

    /// <summary>Creates an addable rectangle at the origin with zero size.</summary>
    protected AddableRectBase()
    {
    }

    /// <summary>Creates an addable rectangle at (<paramref name="x"/>, <paramref name="y"/>) with the given size.</summary>
    protected AddableRectBase(float x, float y, float width, float height)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
    }

    /// <inheritdoc/>
    public virtual float X
    {
        get => _x;
        set
        {
            if (_x == value)
            {
                return;
            }
            _x = value;
            OnPositionChanged();
        }
    }

    /// <inheritdoc/>
    public virtual float Y
    {
        get => _y;
        set
        {
            if (_y == value)
            {
                return;
            }
            _y = value;
            OnPositionChanged();
        }
    }

    /// <inheritdoc/>
    public virtual float Width
    {
        get => _width;
        set
        {
            if (_width == value)
            {
                return;
            }
            _width = value;
            OnSizeChanged();
        }
    }

    /// <inheritdoc/>
    public virtual float Height
    {
        get => _height;
        set
        {
            if (_height == value)
            {
                return;
            }
            _height = value;
            OnSizeChanged();
        }
    }

    /// <summary>Raised after <see cref="X"/> or <see cref="Y"/> changes.</summary>
    public event EventHandler? PositionChanged;

    /// <summary>Raised after <see cref="Width"/> or <see cref="Height"/> changes.</summary>
    public event EventHandler? SizeChanged;

    /// <summary>Called from the X / Y setters after the value changes. Default raises <see cref="PositionChanged"/>; override to do more.</summary>
    protected virtual void OnPositionChanged()
    {
        PositionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Called from the Width / Height setters after the value changes. Default raises <see cref="SizeChanged"/>; override to do more.</summary>
    protected virtual void OnSizeChanged()
    {
        SizeChanged?.Invoke(this, EventArgs.Empty);
    }
}
