using BabyBearsEngine.Input;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Worlds.UI;

public class UIElementBase : ContainerEntity, IClickable
{
    private const double HoverDelaySeconds = 0.5;

    private readonly ClickController _buttonHandler;
    private readonly int _x;
    private readonly int _y;
    private readonly int _width;
    private readonly int _height;

    public UIElementBase(int x, int y, int width, int height)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _buttonHandler = new(this, HoverDelaySeconds);
    }

    private bool MouseOver => 
        Mouse.ClientX >= _x && Mouse.ClientX < _x + _width &&
        Mouse.ClientY >= _y && Mouse.ClientY < _y + _height;

    public override void Update(double elapsed)
    {
        if (MouseOver)
        {
            MouseSolver.RegisterMouseOver(_buttonHandler);
        }
        _buttonHandler.Update(elapsed);

        base.Update(elapsed);
    }

    // --- Interface Implementation (The Bridge) ---
    void IClickable.TriggerLeftPressed() => OnLeftPressed(); 
    void IClickable.TriggerLeftReleased() => OnLeftReleased(); 
    void IClickable.TriggerMouseEntered() => OnMouseEntered(); 
    void IClickable.TriggerMouseExited() => OnMouseExited(); 
    void IClickable.TriggerHover() => OnMouseHovered();
    void IClickable.TriggerCancelHover() => OnStopMouseHovered();

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

    public event EventHandler? LeftPressed;
    public event EventHandler? LeftReleased;
    public event EventHandler? MouseEntered;
    public event EventHandler? MouseExited;
    public event EventHandler? MouseHovered;
    public event EventHandler? MouseHoverStopped;

}
