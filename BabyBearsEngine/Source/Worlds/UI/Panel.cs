using BabyBearsEngine.Graphics;

namespace BabyBearsEngine.Worlds.UI;

/// <summary>A container entity with a solid coloured background.</summary>
public class Panel : Entity
{
    public Panel(int x, int y, int width, int height, Colour colour)
        : base(x, y, width, height)
    {
        Add(new ColouredRectangle(colour, 0, 0, width, height));
    }
}
