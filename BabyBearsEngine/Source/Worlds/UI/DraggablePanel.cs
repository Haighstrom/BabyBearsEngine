using BabyBearsEngine.Graphics;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>
/// A <see cref="Panel"/> that can be repositioned by clicking and dragging a grab strip along its top edge.
/// </summary>
public sealed class DraggablePanel : Panel
{
    private const int GrabBarHeight = 30;

    public DraggablePanel(int x, int y, int width, int height, Colour colour)
        : base(x, y, width, height, colour)
    {
        var controller = new DragController(
            () => GrabArea,
            () => (X, Y),
            () => Draggable);

        controller.DragStarted += OnDragStarted;
        controller.DragStopped += OnDragStopped;
        controller.PositionChanged += OnPositionChanged;

        Add(controller);
    }

    public bool Draggable { get; set; } = true;
    public bool Dragging { get; private set; }

    private Rect GrabArea => new(X, Y, Width, GrabBarHeight);

    public event EventHandler? DragStarted;
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
