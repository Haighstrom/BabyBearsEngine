using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A <see cref="Panel"/> that can be repositioned by clicking and dragging a grab strip along its top edge.
/// </summary>
public sealed class DraggablePanel : Panel
{
    private const int GrabBarHeight = 30;

    /// <param name="x">X position relative to the parent container.</param>
    /// <param name="y">Y position relative to the parent container.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="colour">Background fill colour.</param>
    public DraggablePanel(float x, float y, float width, float height, Colour colour)
        : base(x, y, width, height, colour)
    {
        var controller = new DragController(
            () => GrabArea,
            () => ((int)X, (int)Y),
            () => Draggable);

        controller.DragStarted += OnDragStarted;
        controller.DragStopped += OnDragStopped;
        controller.PositionChanged += OnPositionChanged;

        Add(controller);
    }

    /// <summary>When <c>false</c>, the drag grab strip is disabled and the panel cannot be moved.</summary>
    public bool Draggable { get; set; } = true;

    /// <summary><c>true</c> while the user is actively dragging the panel.</summary>
    public bool Dragging { get; private set; }

    private Rect GrabArea => new(X, Y, Width, GrabBarHeight);

    /// <summary>Raised when the user begins dragging the panel.</summary>
    public event EventHandler? DragStarted;

    /// <summary>Raised when the user releases the panel after dragging.</summary>
    public event EventHandler? DragStopped;

    private void OnDragStarted()
    {
        Dragging = true;
        DragStarted?.Invoke(this, EventArgs.Empty);
    }

    private void OnDragStopped()
    {
        Dragging = false;
        DragStopped?.Invoke(this, EventArgs.Empty);
    }

    private void OnPositionChanged(int newX, int newY)
    {
        X = newX;
        Y = newY;
    }
}
