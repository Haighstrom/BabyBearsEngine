using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Worlds.Graphics.Text;

public interface ITextGraphic : IRenderable
{
    Colour Colour { get; set; }

    bool Multiline { get; set; }

    string Text { get; set; }

    Point MeasureString(string text);

    Point MeasureString();
}
