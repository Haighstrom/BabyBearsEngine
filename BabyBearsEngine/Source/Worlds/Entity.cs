using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds;

/// <summary>
/// Base class for all entities. Always acts as a container for child entities.
/// Pass <c>clickable: true</c> to enable mouse interaction events.
/// </summary>
public class Entity : ContainerEntity, IMouseInteractable
{
    private const double HoverDelaySeconds = 0.5;

    /// <param name="x">X position relative to parent.</param>
    /// <param name="y">Y position relative to parent.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="clickable">When true, wires up mouse interaction and raises click events.</param>
    public Entity(int x, int y, int width, int height, bool clickable = false)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;

        if (clickable)
        {
            var controller = new ClickController(this, HoverDelaySeconds);
            controller.HoverCancelled += OnStopMouseHovered;
            controller.Hovered += OnMouseHovered;
            controller.LeftPressed += OnLeftPressed;
            controller.LeftReleased += OnLeftReleased;
            controller.MouseEntered += OnMouseEntered;
            controller.MouseExited += OnMouseExited;
            Add(controller);
        }
    }

    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Rect PositionOnScreen
    {
        get
        {
            var (wx, wy) = Parent?.GetWindowCoordinates(X, Y) ?? (X, Y);
            return new((int)wx, (int)wy, Width, Height);
        }
    }

    public override (float x, float y) GetWindowCoordinates(float x, float y) =>
        Parent?.GetWindowCoordinates(x + X, y + Y) ?? (x + X, y + Y);

    public sealed override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        var mv = Matrix3.Translate(ref modelView, X, Y);
        base.Render(ref projection, ref mv);
    }

    public event EventHandler? LeftPressed;
    public event EventHandler? LeftReleased;
    public event EventHandler? MouseEntered;
    public event EventHandler? MouseExited;
    public event EventHandler? MouseHovered;
    public event EventHandler? MouseHoverStopped;

    protected virtual void OnLeftPressed()
    {
        LeftPressed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnLeftReleased()
    {
        LeftReleased?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnMouseEntered()
    {
        MouseEntered?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnMouseExited()
    {
        MouseExited?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnMouseHovered()
    {
        MouseHovered?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnStopMouseHovered()
    {
        MouseHoverStopped?.Invoke(this, EventArgs.Empty);
    }
}
