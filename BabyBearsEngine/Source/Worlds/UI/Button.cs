using BabyBearsEngine.Graphics;
using BabyBearsEngine.Input;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Rendering.Graphics.Text;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Worlds.UI;

public class Button : IEntity, IClickable
{
    private const double HoverDelaySeconds = 0.5; // tweak as needed

    private readonly ColouredRectangle _graphic;
    private readonly TextImage _textImage;
    private readonly ClickController _buttonHandler;
    private readonly int _x;
    private readonly int _y;
    private readonly int _width;
    private readonly int _height;

    public Button(int x, int y, int width, int height, Colour colour, string textToDisplay = "")
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _graphic = new(colour, x, y, width, height);
        _textImage = new(new FontDefinition("Times New Roman", 16), textToDisplay, Colour.Black, x, y, width, height);
        _buttonHandler = new ClickController(this, HoverDelaySeconds);
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
    }

    // --- Interface Implementation (The Bridge) ---
    void IClickable.TriggerLeftClicked() => OnLeftClicked();
    void IClickable.TriggerLeftPressed() => OnLeftPressed(); 
    void IClickable.TriggerLeftReleased() => OnLeftReleased(); 
    void IClickable.TriggerMouseEntered() => OnMouseEntered(); 
    void IClickable.TriggerMouseExited() => OnMouseExited(); 
    void IClickable.TriggerHover() => OnMouseHovered(); 
    void IClickable.TriggerStopHover() => OnStopMouseHovered();

    private void OnStopMouseHovered()
    {
        throw new NotImplementedException();
    }

    protected virtual void OnLeftClicked() 
    {
        LeftClicked?.Invoke(this, EventArgs.Empty);
    }
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

    public event EventHandler? LeftClicked;
    public event EventHandler? LeftPressed;
    public event EventHandler? LeftReleased;
    public event EventHandler? MouseEntered;
    public event EventHandler? MouseExited;
    public event EventHandler? MouseHovered;
    public event EventHandler? NoMouseEvent;

    public void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        _graphic.Render(ref projection, ref modelView);
        _textImage.Render(ref projection, ref modelView);
    }
}
