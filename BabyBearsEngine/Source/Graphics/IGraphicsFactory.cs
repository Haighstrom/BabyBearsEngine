using BabyBearsEngine.Source.Graphics.Text;

namespace BabyBearsEngine.Source.Graphics;

public interface IGraphicsFactory
{
    ITextGraphic CreateTextGraphic(FontDefinition font, float x, float y, float width, float height, string text);
}
