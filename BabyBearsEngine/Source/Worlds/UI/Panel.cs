using BabyBearsEngine.Graphics;
using BabyBearsEngine.Source.Geometry;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>A container entity with a solid coloured background.</summary>
public class Panel(int x, int y, int width, int height, Colour colour)
    : Entity(x, y, width, height)
{
    private readonly ColouredRectangle _background = new(colour, x, y, width, height);

    public override void Render(ref Matrix3 projection, ref Matrix3 modelView)
    {
        _background.Render(ref projection, ref modelView);
        
        base.Render(ref projection, ref modelView);
    }
}
