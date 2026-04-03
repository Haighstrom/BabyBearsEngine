using BabyBearsEngine.Source.Geometry;

namespace BabyBearsEngine.Tests.System.Source.ClickTest;

internal class HoverBox(int x, int y, int width, int height) : ClickableEntity(x, y, width, height)
{
    private readonly ColouredRectangle _rectangle = new(Colour.LightBlue, x, y, width, height);

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        base.Render(ref projection, ref modelView);

        _rectangle.Render(ref projection, ref modelView);
    }

    protected override void OnMouseEntered()
    {
        base.OnMouseEntered();

        _rectangle.Colour = Colour.Green;
    }

    protected override void OnMouseExited()
    {
        base.OnMouseExited();

        _rectangle.Colour = Colour.Red;
    }
}
