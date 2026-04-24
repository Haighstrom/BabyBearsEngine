using BabyBearsEngine.Graphics;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Rendering.Graphics.Text;

namespace BabyBearsEngine.Worlds.UI;

public class Button(int x, int y, int width, int height, Colour colour, string textToDisplay = "", bool autoRecolour = true)
    : ClickableEntity(x, y, width, height)
{
    private readonly Colour _hoverColour = colour.Lightened(0.05f);
    private readonly Colour _pressedColour = colour.Darkened(0.05f);
    private readonly ColouredRectangle _rectangle = new(colour, x, y, width, height);
    private readonly TextImage _textImage = new(new FontDefinition("Times New Roman", 16), textToDisplay, Colour.Black, x, y, width, height);

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        _rectangle.Render(ref projection, ref modelView);
        _textImage.Render(ref projection, ref modelView);
    }

    protected override void OnLeftPressed()
    {
        base.OnLeftPressed();

        if (autoRecolour)
        {
            _rectangle.Colour = _pressedColour;
        }
    }

    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();

        if (autoRecolour)
        {
            _rectangle.Colour = _hoverColour;
        }
    }

    protected override void OnMouseEntered()
    {
        base.OnMouseEntered();

        if (autoRecolour)
        {
            _rectangle.Colour = _hoverColour;
        }
    }

    protected override void OnMouseExited()
    {
        base.OnMouseExited();

        if (autoRecolour)
        {
            _rectangle.Colour = colour;
        }
    }
}
