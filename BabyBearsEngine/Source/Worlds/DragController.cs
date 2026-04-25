using BabyBearsEngine.Input;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Worlds;

namespace BabyBearsEngine.Source.Worlds;

/// <summary>
/// Tracks mouse drag interactions for a region, raising events when a drag starts, moves, or stops.
/// Add to a container as an <see cref="IUpdateable"/> to drive it automatically each frame.
/// </summary>
/// <param name="grabArea">Delegate returning the screen-space rect the user must press to begin a drag.</param>
/// <param name="getPosition">Delegate returning the current position of the dragged object, used to compute the grab offset.</param>
/// <param name="canDrag">Optional delegate returning whether dragging is permitted. Defaults to always true. If it returns false mid-drag, the drag is cancelled.</param>
internal sealed class DragController(
    Func<Rect> grabArea,
    Func<(int X, int Y)> getPosition,
    Func<bool>? canDrag = null) : AddableBase, IUpdateable
{
    private int _startOffsetX;
    private int _startOffsetY;

    public bool Active { get; set; } = true;
    public bool Dragging { get; private set; }

    public event Action? DragStarted;
    public event Action? DragStopped;
    /// <summary>Raised each frame while dragging, with the new absolute position the dragged object should move to.</summary>
    public event Action<int, int>? PositionChanged;

    public void Update(double elapsed)
    {
        if (Dragging && (Mouse.LeftReleased || !(canDrag?.Invoke() ?? true)))
        {
            Dragging = false;
            DragStopped?.Invoke();
            return;
        }

        if (!Dragging && Mouse.LeftPressed && (canDrag?.Invoke() ?? true) && grabArea().Contains(Mouse.ClientX, Mouse.ClientY))
        {
            var (x, y) = getPosition();
            _startOffsetX = Mouse.ClientX - x;
            _startOffsetY = Mouse.ClientY - y;
            Dragging = true;
            DragStarted?.Invoke();
        }

        if (Dragging)
        {
            PositionChanged?.Invoke(Mouse.ClientX - _startOffsetX, Mouse.ClientY - _startOffsetY);
        }
    }
}
