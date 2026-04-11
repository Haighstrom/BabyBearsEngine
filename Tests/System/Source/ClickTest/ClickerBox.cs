using BabyBearsEngine.Source.Geometry;

namespace BabyBearsEngine.Tests.System.Source.ClickTest;

internal class ClickerBox(int x, int y, int width, int height, Colour defaultColour) 
    : ClickableEntity(x, y, width, height)
{
    private readonly ColouredRectangle _rectangle = new(defaultColour, x, y, width, height);

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        base.Render(ref projection, ref modelView);

        _rectangle.Render(ref projection, ref modelView);
    }

    protected override void OnMouseEntered()
    {
        base.OnMouseEntered();

        _rectangle.Colour = Colour.LightBlue;
    }

    protected override void OnMouseExited()
    {
        base.OnMouseExited();

        _rectangle.Colour = defaultColour;
    }

    protected override void OnLeftPressed()
    {
        base.OnLeftPressed();

        _rectangle.Colour = Colour.Blue;
    }

    protected override void OnLeftReleased()
    {
        base.OnLeftReleased();

        _rectangle.Colour = Colour.LightBlue;
    }
}
