using BabyBearsEngine.Graphics;
using BabyBearsEngine.Rendering.Graphics.Text;

namespace BabyBearsEngine.Worlds.UI;

public class Button : Entity
{
    private readonly Colour _colour;
    private readonly Colour _hoverColour;
    private readonly Colour _pressedColour;
    private readonly ColouredRectangle _rectangle;
    private readonly bool _autoRecolour;

    public Button(int x, int y, int width, int height, Colour colour, string textToDisplay = "", bool autoRecolour = true)
        : base(x, y, width, height, clickable: true)
    {
        _colour = colour;
        _hoverColour = colour.Lightened(0.05f);
        _pressedColour = colour.Darkened(0.05f);
        _autoRecolour = autoRecolour;

        _rectangle = new ColouredRectangle(colour, 0, 0, width, height);
        var textImage = new TextImage(new FontDefinition("Times New Roman", 16), textToDisplay, Colour.Black, 0, 0, width, height);

        Add(_rectangle);
        Add(textImage);
    }

    protected override void OnLeftPressed()
    {
        base.OnLeftPressed();

        if (_autoRecolour)
        {
            _rectangle.Colour = _pressedColour;
        }
    }

    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();

        if (_autoRecolour)
        {
            _rectangle.Colour = _hoverColour;
        }
    }

    protected override void OnMouseEntered()
    {
        base.OnMouseEntered();

        if (_autoRecolour)
        {
            _rectangle.Colour = _hoverColour;
        }
    }

    protected override void OnMouseExited()
    {
        base.OnMouseExited();

        if (_autoRecolour)
        {
            _rectangle.Colour = _colour;
        }
    }
}
