using BabyBearsEngine.Graphics;
using BabyBearsEngine.Source.Geometry;
using BabyBearsEngine.Source.Rendering.Graphics.Text;

namespace BabyBearsEngine.Worlds.UI;

public class Button(int x, int y, int width, int height, Colour colour, string textToDisplay = "") 
    : ClickableEntity(x, y, width, height)
{
    private readonly ColouredRectangle _rectangle = new(colour, x, y, width, height);
    private readonly TextImage _textImage = new(new FontDefinition("Times New Roman", 16), textToDisplay, Colour.Black, x, y, width, height);

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        _rectangle.Render(ref projection, ref modelView);
        _textImage.Render(ref projection, ref modelView);
    }

    protected override void OnMouseEntered()
    {
        base.OnMouseEntered();

        _rectangle.Colour = Colour.LightGray;
    }

    protected override void OnMouseExited()
    {
        base.OnMouseExited();

        _rectangle.Colour = colour;
    }

    protected override void OnLeftPressed()
    {
        base.OnLeftPressed();

        _rectangle.Colour = Colour.LightBlue;
    }

    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();

        _rectangle.Colour = Colour.LightGray;
    }
}
