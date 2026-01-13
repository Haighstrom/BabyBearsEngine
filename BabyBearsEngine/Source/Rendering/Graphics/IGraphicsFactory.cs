using BabyBearsEngine.Source.Rendering.Graphics.Text;

namespace BabyBearsEngine.Graphics;

public interface IGraphicsFactory
{
    ITextGraphic CreateTextGraphic(FontDefinition font, float x, float y, float width, float height, string text);
}
