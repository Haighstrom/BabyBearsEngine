using BabyBearsEngine.Input;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Worlds.UI;

public class UIElementBase : AddableBase, IEntity, IClickable, IContainer
{
    private const double HoverDelaySeconds = 0.5;

    private readonly ClickController _buttonHandler;
    private readonly int _x;
    private readonly int _y;
    private readonly int _width;
    private readonly int _height;
    private readonly Container _container = new();
    // Properties
    public bool Visible { get; set; } = true;

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

    public void Update(double elapsed)
    {
        if (MouseOver)
        {
            MouseSolver.RegisterMouseOver(_buttonHandler);
        }
        _buttonHandler.Update(elapsed);

        foreach (var entity in _container.GetUpdatables())
        {
            entity.Update(elapsed);
        }
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

    public virtual void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        foreach (var entity in _container.GetRenderables())
        {
            if (!entity.Visible)
            {
                continue;
            }

            entity.Render(ref projection, ref modelView);
        }
    }

    public void Add(IAddable entity) => _container.Add(entity);
    public void Remove(IAddable entity) => _container.Remove(entity);
    public void RemoveAll() => _container.RemoveAll();
}
