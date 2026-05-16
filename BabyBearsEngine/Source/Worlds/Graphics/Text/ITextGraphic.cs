using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics;

namespace BabyBearsEngine.Worlds.Graphics.Text;

public interface ITextGraphic : IRenderable
{
    Colour Colour { get; set; }

    float ExtraCharacterSpacing { get; set; }

    float ExtraLineSpacing { get; set; }

    float ExtraSpaceWidth { get; set; }

    int FirstCharToDraw { get; set; }

    FontDefinition Font { get; set; }

    bool Multiline { get; set; }

    int NumCharsToDraw { get; set; }

    TextDecoration? Strikethrough { get; set; }

    string Text { get; set; }

    TextDecoration? Underline { get; set; }

    Point MeasureString(string text);

    Point MeasureString();
}
