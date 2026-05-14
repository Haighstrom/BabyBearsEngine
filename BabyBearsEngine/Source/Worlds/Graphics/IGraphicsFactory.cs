using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Worlds.Graphics;

public interface IGraphicsFactory
{
    ITextGraphic CreateTextGraphic(FontDefinition font, float x, float y, float width, float height, string text);
}
