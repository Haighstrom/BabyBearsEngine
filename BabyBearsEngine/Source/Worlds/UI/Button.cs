using BabyBearsEngine.Graphics;
using BabyBearsEngine.Input;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Rendering.Graphics.Text;
using BabyBearsEngine.Source.Worlds;

namespace BabyBearsEngine.Worlds.UI;

public class Button : IEntity, IButtonTrigger
{
    private const double HoverDelaySeconds = 0.5; // tweak as needed

    private readonly ColouredRectangle _graphic;
    private readonly TextImage _textImage;
    private readonly ButtonInteractionHandler _interaction;
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
        _interaction = new ButtonInteractionHandler(this, HoverDelaySeconds);
    }

    public void Update(double elapsed)
    {
        bool isInside = Mouse.ClientX >= _x && Mouse.ClientX < _x + _width &&
                        Mouse.ClientY >= _y && Mouse.ClientY < _y + _height;

        _interaction.Update(elapsed, isInside, Mouse.LeftPressed);
    }

    // --- Interface Implementation (The Bridge) ---
    void IButtonTrigger.TriggerLeftClicked() => OnLeftClicked();
    void IButtonTrigger.TriggerLeftPressed() => OnLeftPressed(); 
    void IButtonTrigger.TriggerLeftReleased() => OnLeftReleased(); 
    void IButtonTrigger.TriggerMouseEntered() => OnMouseEntered(); 
    void IButtonTrigger.TriggerMouseExited() => OnMouseExited(); 
    void IButtonTrigger.TriggerMouseHovered() => OnMouseHovered(); 

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
